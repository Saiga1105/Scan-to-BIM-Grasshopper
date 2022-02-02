import os
import shlex
import glob
import time
import subprocess
import pandas as pd
import geopandas as gpd

import pylas
import numpy as np
import cv2
import rasterio
import fiona
import argparse
import sys

from osgeo import gdal, ogr, osr
from matplotlib import pyplot as plt
import matplotlib.colors as colors

from vsolar.PolygonRansac import PolygonRansac
from vsolar.RefinePolygonEdges import remove_small_polygons, compute_stats
from vsolar.BuildingFootprint import BuildingFootprint
from vsolar.utils import from_raster_to_vector, write_geotiff,get_plane_coeff, \
                        get_bigger_poly_from_shp, write_poly_to_shp, plane_clustering, OBB
from raster2xyz.raster2xyz import Raster2xyz
from rasterio import features

from skimage.filters import median
from skimage import morphology

from shapely.geometry.polygon import Polygon
from shapely.ops import cascaded_union, snap
from sys import platform

if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='Building Roof RANSAC Segmentation Script')
    parser.add_argument('input_file', help='input GeoTIFF file containing the high-resolution DSM')
    parser.add_argument('output_folder', help='output folder for saving the results')
    parser.add_argument('--elev_th', type=float, default=4.5,
                        help='Elevation threshold: ignore roof elements with elevation < elev_th (Default 4.5 m)')
    parser.add_argument('--lidar', action='store_true', help='Turn on this flag for using LiDAR n-returns band to remove vegetation')
    parser.add_argument('--verbose', action='store_true',
                        help='Turn on this flag for printing and plotting intermediate results')
    parser.add_argument('--min_plane_points', type=int, default=10, help='Min number of points to support a plane during'
                                                                         ' the RANASAC 3D plane segmentation (default value = 10 pts) Type: int')
    parser.add_argument('--min_obj_area', type=float, default=5.0, help='Min area of roof planes (default value 5 m^2) Type: float')
    parser.add_argument('--ransac3d', type=float, metavar=['EPSILON', 'NORMAL_TH'], nargs=2, default=[0.05, 0.001],
                        help='RANSAC 3D parameters epsilon, normal_th for plane segmentation: EPSILON-> max 3d '
                             'discrepancy in m \n NORMAL_TH-> max angle deviation from point normals. Note:The error between a point-with-normal p '
                             'and a plane S is defined by its Euclidean distance and normal deviation to S. '
                             'The normal deviation is computed between the normal at p and the S normal at the closest projection of p into S. '
                             'The parameter epsilon defines the absolute maximum tolerance Euclidean distance between a point and a shape. (default values 0.05 m, 0.001 Radians) Type: float')
    parser.add_argument('--ransac2d', type=float, default=2,
                        help='RANSAC Planimetric max discrepancy for polygon simplification (Default 2.0 m) Type: float')

    args = parser.parse_args()

    if not os.path.exists(args.input_file):
        parser.error("The file %s does not exist!" % os.path.abspath(args.input_file))

    inputFile = os.path.abspath(args.input_file)
    fileName = os.path.basename(args.input_file)   # TODO test "drone_area_6.tif"

    output_folder = os.path.abspath(args.output_folder)
    tmp_dir = os.path.abspath(args.output_folder+os.sep+"tmp")

    if not os.path.exists(output_folder):
        os.makedirs(output_folder)
    if not os.path.exists(tmp_dir):
        os.mkdir(tmp_dir)

    print("Processing input file: %s" % os.path.abspath(args.input_file))
    print("Output directory: %s" % os.path.abspath(args.output_folder))

    try:
        raster = gdal.Open(inputFile)
    except FileNotFoundError:
        print("Error: Invalid file format, please provide geotif file")
        sys.exit(-1)

    GSD = abs(raster.GetGeoTransform()[1])
    print("Input file GSD %6.3f" % GSD)

    # Setting the processing parameters
    ELEV_TH = args.elev_th
    LIDAR = args.lidar
    RANSAC_3D_TH = args.ransac3d[0]
    RANSAC_3D_NORM_TH = args.ransac3d[1]
    OBJ_TH = args.min_obj_area  # TODO: improvement? change to Parametric with number of points, GSD and area line 295
    RANSAC_TH = args.ransac2d*GSD
    MIN_PLANE_POINTS = args.min_plane_points
    MIN_NUM_CELL = 8  #4
    VERB_FLAG = args.verbose

    print("*** Processing Parameters: ***")
    print(" - Elevation Th: %6.3f m" % args.elev_th)
    print(" - No Lidar Flag: ", args.lidar)
    print(" - Min Roof Plane Area = %6.3f m^2" % args.min_obj_area)
    print(" - RANSAC 3D Parameters: a = %6.3f m" % args.ransac3d[0], ", b = %8.6f deg" % args.ransac3d[1])
    print(" - RANSAC 2D Th: %6.3f m" % RANSAC_TH)
    print(" - Verbose: ", args.verbose)

    elev = np.array(raster.GetRasterBand(1).ReadAsArray())
    elev = morphology.closing(elev, morphology.disk(1))
    # elev = cv2.GaussianBlur(elev, (3, 3), 0)

    min5, max95 = np.percentile(elev[np.where(elev > 0)], [5, 95])

    if LIDAR:
        print("Removing trees using LiDAR n returns... ")
        returns = np.array(raster.GetRasterBand(6).ReadAsArray())
        # returns = median(returns, morphology.square(3))
        retval, mask_tree = cv2.threshold(returns, 1.9, 1, cv2.THRESH_BINARY)
        elev[np.where(mask_tree > 0)] = 0

    retval1, dst = cv2.threshold(elev, min5 + ELEV_TH, 1, cv2.THRESH_BINARY)

    #plt.imshow(dst)
    #plt.show()

    write_geotiff(os.path.join(tmp_dir,fileName.split('.')[0] + "_bin.tif"), dst, raster.GetGeoTransform(), raster.GetProjection())
    srs = from_raster_to_vector(os.path.join(tmp_dir, fileName.split('.')[0] + "_bin.tif"), os.path.join(tmp_dir,"polygon_" + fileName[0:-4]))

    poly, bbox = get_bigger_poly_from_shp(os.path.join(tmp_dir, "polygon_" + fileName[0:-4] + '.shp'))

    X, Y = poly.exterior.coords.xy
    #plt.axis('equal')
    #plt.plot(X, Y)
    #plt.show()

    poly_bbox, axis = OBB(poly)

    # print("Footprint area-bbox ratio: ", Polygon(poly.exterior).area / poly_bbox.area)
    # if Polygon(poly.exterior).area/poly_bbox.buffer(-GSD).area > 0.90:
    #     best_poly = poly_bbox.buffer(-GSD)
    # else:

    footprint = BuildingFootprint(poly, RANSAC_TH)
    footprint.compute(verbose=VERB_FLAG)
    VERB_FLAG and footprint.plot_detected_lines()
    best_poly = footprint.get_poly_footprint(verbose=VERB_FLAG)

    # Handle exeption in case of MultiPolygon footprint
    if best_poly.geometryType() == 'MultiPolygon':
        polygons = list(best_poly)
        max_area = 0
        for poly in polygons:
            if poly.area > max_area:
                best_poly = poly
                max_area = poly.area

    hull_pts = best_poly.exterior.coords.xy
    X, y = best_poly.exterior.coords.xy
    X1, y1 = bbox.exterior.coords.xy
    plt.plot(X, y)
    plt.plot(X1, y1)
    plt.show()

    write_poly_to_shp(os.path.join(output_folder, fileName[0:-4] + '_building_footprint.shp'), best_poly, srs)

    rtxyz = Raster2xyz()
    rtxyz.translate(inputFile, os.path.join(tmp_dir, fileName[0:-4] + "_xyz.csv"))

    df = pd.read_csv(os.path.join(tmp_dir, fileName[0:-4] + "_xyz.csv"))
    gdf = gpd.GeoDataFrame(df, geometry=gpd.points_from_xy(df.x, df.y))

    poly_gdf = gpd.GeoDataFrame.from_file(os.path.join(output_folder, fileName[0:-4] + '_building_footprint.shp'))

    gdf.crs = poly_gdf.crs
    print("Reference System Code: ", gdf.crs)
    #print(poly_gdf.crs)

    merge = gpd.tools.sjoin(gdf, poly_gdf, how='left', op='within')
    points = merge.dropna()
    points = points[points.z > min5+ELEV_TH]
    points.z = points.z - min5

    points['snap_dist'] = points.distance(best_poly.boundary)

    point_border = points[points.snap_dist < 1.5*GSD]
    point_inside = points[points.snap_dist >= 1.5*GSD]

    snap_xx = []
    snap_yy = []
    snap_zz = []

    snap_xx_facade = []
    snap_yy_facade = []
    snap_zz_facade = []

    for id, p in enumerate(point_border.geometry):
        snap_p = best_poly.exterior.interpolate(best_poly.exterior.project(p))

        if best_poly.contains(p):
            snap_xx.append(p.x)
            snap_yy.append(p.y)
            snap_zz.append(point_border.z.to_numpy()[id])

        snap_xx.append(snap_p.x)
        snap_yy.append(snap_p.y)
        snap_zz.append(point_border.z.to_numpy()[id])

        dst = np.sqrt((snap_p.x-p.x)**2+(snap_p.y-p.y)**2)
        norm_x = (snap_p.x-p.x)/dst
        norm_y = (snap_p.y-p.y)/dst
        #print(norm_x, norm_y)

        for z in range(int(point_border.z.to_numpy()[id])):
            snap_xx_facade.append(snap_p.x)
            snap_yy_facade.append(snap_p.y)
            snap_zz_facade.append(z)

    # plt.scatter(point_border.x, point_border.y)
    # plt.scatter(snap_xx, snap_yy)
    # plt.show()

    #pos = best_poly.geometry.project(point_border)
    if VERB_FLAG:
        las = pylas.create()
        las.x = np.concatenate([point_inside.x, snap_xx])
        las.y = np.concatenate([point_inside.y, snap_yy])
        las.z = np.concatenate([point_inside.z, snap_zz])
        las.write(os.path.join(output_folder, fileName[0:-4] + '_roof_points_snap.las'))

        las2 = pylas.create()
        las2.x = np.concatenate([snap_xx_facade])
        las2.y = np.concatenate([snap_yy_facade])
        las2.z = np.concatenate([snap_zz_facade])
        las2.write(os.path.join(output_folder, fileName[0:-4] + '_facade_points_snap.las'))

        las3 = pylas.create()
        las3.x = np.concatenate([point_inside.x, snap_xx, snap_xx_facade])
        las3.y = np.concatenate([point_inside.y, snap_yy, snap_yy_facade])
        las3.z = np.concatenate([point_inside.z, snap_zz, snap_zz_facade])
        las3.write(os.path.join(output_folder , fileName[0:-4] + '_merged_points_snap.las'))

    # Default values -> min_points=10, epsilon=0.05, normal_threshold=0.01
    xx_corr, yy_corr, zz_corr, cc, planes = plane_clustering(np.concatenate([point_inside.x, snap_xx]),
                                                     np.concatenate([point_inside.y, snap_yy]),
                                                     np.concatenate([point_inside.z, snap_zz]),
                                                             epsilon= RANSAC_3D_TH,
                                                             min_points=MIN_PLANE_POINTS,
                                                             normal_threshold=RANSAC_3D_NORM_TH,
                                                             verbose=VERB_FLAG)

    las = pylas.create()
    las.add_extra_dim("classes", 'uint8')
    las.x = xx_corr
    las.y = yy_corr
    las.z = zz_corr
    las.classes = cc
    las.write(os.path.join(tmp_dir, fileName[0:-4] + '_planefit_labeled.las'))

    #Create the raster labeled tif using CloudCompare
    file_path = os.path.join(tmp_dir, fileName[0:-4] + '_planefit_labeled.las')

    # Generate a log file
    log_file_path = os.path.join(tmp_dir, fileName[0:4] +"_log.txt")
    f = open(log_file_path, "w")
    f.close()
        
    if platform == "linux" or platform == "linux2":
    # External call to CloudCompare command line tools -> Linux TODO Check right exe name in linux
        command = shlex.split("open -a cloudcompare.CloudCompare --args -SILENT -LOG_FILE "+ log_file_path +" -O " + file_path + " -RASTERIZE -GRID_STEP 0.25 -PROJ MAX -SF_PROJ MAX -OUTPUT_RASTER_Z ")
    elif platform == "darwin":
    # External call to CloudCompare command line tools -> OS X
        command = shlex.split("open -a CloudCompare.app --args -SILENT -LOG_FILE "+ log_file_path +" -O " + file_path + " -RASTERIZE -GRID_STEP 0.25 -PROJ MAX -SF_PROJ MAX -OUTPUT_RASTER_Z ")
    elif platform == "win32":
    # External call to CloudCompare command line tools -> Win
        commandoneline =  "CloudCompare.exe -SILENT -LOG_FILE "+ log_file_path +" -O " + file_path + " -RASTERIZE -GRID_STEP 0.25 -PROJ MAX -SF_PROJ MAX -OUTPUT_RASTER_Z"
        command = commandoneline.split(" ")

    print("Generation of labeled Geotif using Cloud Compare CLI... ")
    VERB_FLAG and print(command)
    p1 = subprocess.Popen(command)
    exit_codes = p1.wait()

    # While loop to wait until the CloudCompare process is finished (Max waiting time 2 minutes)
    with open(log_file_path) as f:
        total_time = 0
        while total_time < 120:
            line = f.readline()
            if not line:
                time.sleep(1)
                total_time = total_time+1
            else:
                VERB_FLAG and print(line)
                if "finished" in line:
                    break

    list_of_files = glob.glob(file_path[0:-4] + "_RASTER_Z*")
    latest_file = max(list_of_files, key=os.path.getctime)
    os.rename(latest_file, os.path.join(tmp_dir, fileName[0:-4] + "_planefit_labeled_raster.tif"))

    src_raw = rasterio.open(inputFile)
    src = rasterio.open(os.path.join(tmp_dir, fileName[0:-4] + "_planefit_labeled_raster.tif"))

    image = src.read(2) + 1
    dsm = src.read(1)
    normalizedDSM = np.zeros(dsm.size)
    normalizedDSM = cv2.normalize(dsm, normalizedDSM, 0, 255, cv2.NORM_MINMAX)
    label_img = np.array(image, dtype=np.uint8)

    # Close small data cap and smooth the label edges
    # label_img = morphology.opening(label_img, morphology.disk(1))
    label_img = morphology.closing(label_img, morphology.disk(1))
    label_img = median(label_img, morphology.square(3))
    # label_img = morphology.opening(label_img, morphology.disk(1))

    normalizedLabel = np.zeros(label_img.size)
    normalizedLabel = cv2.normalize(label_img, normalizedLabel, 0, 255, cv2.NORM_MINMAX)
    cmap = colors.ListedColormap(np.random.rand(256, 3))

    mask = label_img > 0

    if VERB_FLAG:
        # TODO Create one plot with the 3 images
        plt.imshow(normalizedDSM, cmap='gray')
        plt.show()

        plt.imshow(normalizedLabel, cmap=cmap)
        plt.show()

        plt.imshow(mask)
        plt.show()

    # Iterate to fetures and generate the polygons
    print('Generation of the planes polygons...')
    results = []
    gen = features.shapes(np.array(label_img, dtype=np.uint8), mask=mask, connectivity=4, transform=src.transform)
    OBJ_TH = max(OBJ_TH, 5, 0.01*Polygon(best_poly.exterior).area)

    for s, v in gen:
        # print(s, v)
        poly_i = Polygon(s['coordinates'][0])
        bbox, axis = OBB(poly_i)
        # print(axis)
        roof_id = 0
        if poly_i.area < OBJ_TH: #-> small obj
            roof_id = 1
        if bbox.area / poly_i.area > 5:  # empty obj
            roof_id = 1
        if axis[0] / axis[1] > 5 and poly_i.area < min(10 * OBJ_TH, Polygon(best_poly.exterior).area*0.10):
            roof_id = 1

        if v != 1:
            a, b, c, d = get_plane_coeff(planes[int(v-2)])
            results.append({'properties': {'plane_id': v, 'area': poly_i.area, 'plane_eq': planes[int(v-2)],
                                           'a': a, 'b': b, 'c': c, 'd': d, 'obj_type': roof_id}, 'geometry': s})

        else:
            results.append({'properties': {'plane_id': v, 'area': poly_i.area, 'plane_eq': 'none',
                                           'a': 0.0, 'b': 0.0, 'c': 0.0, 'd': 0.0, 'obj_type': roof_id}, 'geometry': s})


    # with fiona.open(os.path.join(output_folder, fileName[0:-4] + "_polygons.geojson"), 'w', driver='GeoJSON', crs=src_raw.crs,
    #                 schema={'properties': [('plane_id', 'int'), ('area', 'float'), ('a', 'float'), ('b', 'float'),
    #                                        ('c', 'float'), ('d', 'float'), ('obj_type', 'int'), ('plane_eq', 'str')],
    #                         'geometry': 'Polygon'}) as dst:
    #     dst.writerecords(results)
    #
    with fiona.open(os.path.join(output_folder, fileName[0:-4] + "_polygons.geojson"), 'w', driver='GeoJSON', crs=src_raw.crs,
                    schema={'properties': [('plane_id', 'int'), ('area', 'float'), ('obj_type', 'int'), ('plane_eq', 'str'),
                                           ('a', 'float:10.6'), ('b', 'float:10.6'),
                                           ('c', 'float:10.6'), ('d', 'float:10.6')],
                            'geometry': 'Polygon'}) as dst:
        dst.writerecords(results)

    # Remove small polygons
    gdf = gpd.read_file(os.path.join(output_folder, fileName[0:-4] + "_polygons.geojson"))
    good_poly = remove_small_polygons(gdf, MIN_NUM_CELL * GSD ** 2)

    # Save Clean Good Polygons
    VERB_FLAG and good_poly.to_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good.geojson"), driver="GeoJSON")

    # PROCEDURE: Refine the polygons' edges using 2D RANSAC line detector
    X0 = min(src_raw.bounds[0], src_raw.bounds[2])
    Y0 = min(src_raw.bounds[1], src_raw.bounds[3])

    #good_poly = gpd.read_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good.geojson"), driver="GeoJSON")
    good_poly = good_poly.sort_values('area', ascending=False)

    def compute_point_unique_id(x, y):
        return np.array(np.round(x - X0, 2) * 10000 * 1000 * 100 + np.round(y - Y0, 2) * 100, dtype=int)

    # Step1: refine the polygon edges
    global_xx = np.array([])
    global_yy = np.array([])
    global_xx_clean = np.array([])
    global_yy_clean = np.array([])
    global_id = np.array([], dtype=int)

    # Iterate polygons and compute refined coord -> save in global unique indexed array
    for index, row in good_poly.iterrows():
        xxi, yyi = row.geometry.exterior.xy
        ids = compute_point_unique_id(np.array(xxi), np.array(yyi))
        global_xx = np.concatenate([global_xx, xxi])
        global_yy = np.concatenate([global_yy, yyi])
        global_id = np.concatenate([global_id, ids])

        if len(xxi) > 50: #TODO maybe expose this parameters of min number of polygon points
            detect = PolygonRansac(xxi, yyi, ransac_th=RANSAC_TH*0.75, line_density=1.5 * GSD)
            detect.compute(verbose=VERB_FLAG)
            xxi_clean, yyi_clean = detect.get_cleaned_points()
            global_xx_clean = np.concatenate([global_xx_clean, xxi_clean])
            global_yy_clean = np.concatenate([global_yy_clean, yyi_clean])
        else:
            global_xx_clean = np.concatenate([global_xx_clean, xxi])
            global_yy_clean = np.concatenate([global_yy_clean, yyi])

    data_points = pd.DataFrame(data=np.vstack([global_xx, global_yy, global_id,
                                               global_xx_clean, global_yy_clean]).T,
                               columns=['x', 'y', 'id', 'clean_x', 'clean_y'])

    unique_points = data_points.drop_duplicates(subset='id')

    # Step 2: recreate each polygon with the adjusted coordinates
    for index, row in good_poly.iterrows():
        xxi, yyi = row.geometry.exterior.xy
        ids = compute_point_unique_id(np.array(xxi), np.array(yyi))

        poly_clean_points = unique_points.loc[unique_points['id'].isin(ids)].copy(deep=True)

        # TODO: check possible bug with set_categories
        if len(ids)-1==len(poly_clean_points):
            poly_clean_points.loc[:, 'id'] = poly_clean_points.id.astype("category")
            poly_clean_points.id.cat.set_categories(ids[:-1], inplace=True)
            poly_clean_points = poly_clean_points.sort_values('id')
            fit_poly = Polygon(np.vstack([np.round(poly_clean_points['clean_x'], 4), np.round(poly_clean_points['clean_y'], 4)]).T)
        else:
            ordered_x = []
            ordered_y = []
            for i in ids:
                p = poly_clean_points[poly_clean_points['id'] == i]
                ordered_x.append(p['clean_x'].loc[p.index[0]])
                ordered_y.append(p['clean_y'].loc[p.index[0]])

            fit_poly = Polygon(np.vstack([np.round(ordered_x, 4), np.round(ordered_y, 4)]).T)

        good_poly.loc[index, 'geometry'] = fit_poly

    VERB_FLAG and good_poly.to_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good_ransac.geojson"),
                      driver="GeoJSON")

    good_poly.loc[:, 'geometry'] = good_poly.buffer(0)

    good_poly['diss_id'] = np.ones(len(good_poly['geometry']))

    data_temp_diss = good_poly.dissolve(by='diss_id').loc[1, 'geometry']

    if data_temp_diss.geom_type == 'Polygon':
        interiors = list(data_temp_diss.interiors)
        gap_list = []
        if len(interiors) > 0:
            for i in interiors:
                gap_list.append(Polygon(i))

    elif data_temp_diss.geom_type == 'MultiPolygon':
        polys = list(data_temp_diss)
        gap_list = []
        for p in polys:
            if p.area > 4 * GSD ** 2:
                interiors = list(p.interiors)
                if len(interiors) > 0:
                    for i in interiors:
                        gap_list.append(Polygon(i))
    else:
        print("ERROR: Unhandled geometry type ", data_temp_diss.geom_type)

    data_gaps = gpd.GeoDataFrame(geometry=gap_list, crs=good_poly.crs)

    for index, row in data_gaps.iterrows():
        # print(index)
        neighbors = good_poly[good_poly.geometry.intersects(row['geometry'])]
        neighbors = neighbors.sort_values('area', ascending=False)
        for idx, neighbor in neighbors.iterrows():
            # print(neighbor)
            try:
                union = cascaded_union([neighbor.geometry, row.geometry])
                if union.geom_type == 'Polygon':
                    good_poly.loc[idx, 'geometry'] = union
                    break
            except:
                print('no union')

    # data_gaps.to_file("output_clean_ransac3_gaps.geojson", driver="GeoJSON")
    VERB_FLAG and good_poly.to_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good_ransac_no_holes.geojson"),
                      driver="GeoJSON")


    cutted = gpd.clip(good_poly, best_poly.buffer(-0.6 * GSD))
    cutted.to_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good_ransac_no_holes_cutted.geojson"),
                      driver="GeoJSON")

    cutted = remove_small_polygons(cutted, MIN_NUM_CELL * GSD ** 2)

    # Create BBoxes for small details and simplify shapes
    details = cutted[cutted['obj_type'] == 1].copy(deep=True)

    for index, row in details.iterrows():
        if row.geometry.type == 'Polygon':
            bbox, axis = OBB(row.geometry)
            #print(row.area/bbox.buffer(-0.25*GSD).area)
            if row.area/bbox.buffer(-0.25*GSD).area > 0.8:
                details.loc[index, 'geometry'] = bbox.buffer(-0.25*GSD)
            elif row.area/row.geometry.convex_hull.buffer(-0.25*GSD).area > 0.8:
                details.loc[index, 'geometry'] = row.geometry.convex_hull.buffer(-0.25*GSD)

    if not details.empty:
        details.to_file(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_details.geojson"), driver="GeoJSON")

    # Compute Height polygon statistics and write formatted output files
    compute_stats(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_good_ransac_no_holes_cutted.geojson"),
                  os.path.join(tmp_dir, fileName[0:-4] + "_planefit_labeled_raster.tif"),
                  os.path.join(output_folder, fileName[0:-4] + "_roof_polygons_stats.geojson"), src_raw.crs)

    if not details.empty:
        compute_stats(os.path.join(tmp_dir, fileName[0:-4] + "polygons_clean_details.geojson"),
                      os.path.join(tmp_dir, fileName[0:-4] + "_planefit_labeled_raster.tif"),
                      os.path.join(output_folder, fileName[0:-4] + "_details_polygons_stats.geojson"), src_raw.crs)


