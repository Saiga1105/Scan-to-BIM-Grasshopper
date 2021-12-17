#Importing the needed libraries
import ifcopenshell.util
import ifcopenshell.geom as geom
from ifcopenshell.util.selector import Selector
import open3d as o3d
import numpy as np
import time
import os


#Selecting a directory
def load_project(path):
    if os.path.exists(path):
        projectname = path.split("\\")[-1]
        projectname = projectname.split("-BIM-")[-1]
        projectname = projectname.replace(" ", "-")
        projectname = "TD-"+projectname
        ifc_folder = os.path.join(path,"MODELS","IFC")
        pointcloud_folder = os.path.join(path, "DATA", "PCD")
        return (projectname, ifc_folder, pointcloud_folder)
    else:
        print("Give a correct path")
        return None
    
def mergeclass_subclouds(class_config):
    for Class in class_config:
        # print(Class)
        classpcd = o3d.geometry.PointCloud()
        if len(Class[3]) > 1:
            id = 0
            while id < len(Class[3]):
                pcd = Class[3][id]
                classpcd.__iadd__(pcd)
                id = id + 1
            Class[3] = classpcd
        else:
            Class[3] = Class[3][0]
        # print(Class[3])
##Import the IFC geometry to Open3d geometry


#Load the point cloud data
# pcd = o3d.io.read_point_cloud(r"C:\Repo\SCAN2BIM-python\Samples\Test-Loesberg\pointcloud1.pcd")

#Convert the IFC Open3D meshes to point clouds for the computations

#Function that converts the IFC geometries from one given IFC class to Open3D pointcloud objects
    #INPUT
        #ifc_file = the already loaded IFC file
        #settings = the ifcopenshell geometry settings
        #IfcClass = targeted IFC class all IFC entities of this class containing a geometry will be converted to the Open3D mesh format
        #Voxelsize = the size of the voxel downsampling of the resulting pointcloud
    
    #OUTPUT
        #Pointcloud of the IFC geometry
def IFCtoO3d(ifc_folder_path, IfcClasses= ["IfcWall"], voxel_size = 0.01):
    settings = geom.settings()
    settings.set(settings.USE_WORLD_COORDS, True)
    meshinfo = []

    for ifc_file_path in os.listdir(ifc_folder_path):
        if ifc_file_path.endswith(".ifc"):
            ifc_file = ifcopenshell.open(os.path.join(ifc_folder_path,ifc_file_path))
            for IfcClass in IfcClasses:
                for ifc_entity in ifc_file.by_type(IfcClass): #possible optimalisation by inserting multiple classes here/ LOOKUP if ifcopenshell supports this.
                    try: 
                        shape = geom.create_shape(settings, ifc_entity)
                        ios_vertices = shape.geometry.verts
                        ios_faces = shape.geometry.faces

                        grouped_verts = [[ios_vertices[i], ios_vertices[i + 1], ios_vertices[i + 2]] for i in range(0, len(ios_vertices), 3)]
                        grouped_faces = [[ios_faces[i], ios_faces[i + 1], ios_faces[i + 2]] for i in range(0, len(ios_faces), 3)]

                        meshinfo.append([grouped_verts, grouped_faces])
                    except:
                        pass
                        # print("FAILED: shape creation")
    meshes = []
    for geometry in meshinfo:
        vertices = o3d.utility.Vector3dVector(np.asarray(geometry[0]))
        triangles = o3d.utility.Vector3iVector(np.asarray(geometry[1]))

        mesh = o3d.geometry.TriangleMesh(vertices,triangles)

        meshes.append(mesh)
    pointcloud = o3d.geometry.PointCloud()

    for submesh in meshes:
        mesh_points = round(submesh.get_surface_area() * 1000)
        submeshpcd = submesh.sample_points_uniformly(number_of_points = mesh_points, use_triangle_normal=True)
        pointcloud.__iadd__(submeshpcd)

    downsampled_pointcloud = pointcloud.voxel_down_sample(voxel_size)
    
    return downsampled_pointcloud

def IFCtoO3d_Slabs_floors(ifc_folder_path, voxel_size = 0.01):
    settings = geom.settings()
    settings.set(settings.USE_WORLD_COORDS, True)
    meshinfo = []

    for ifc_file_path in os.listdir(ifc_folder_path):
        if ifc_file_path.endswith(".ifc"):
            ifc_file = ifcopenshell.open(os.path.join(ifc_folder_path,ifc_file_path))
            for ifc_entity in ifc_file.by_type("IfcSlab"):
                # print(ifc_entity)
                try: 
                    shape = geom.create_shape(settings, ifc_entity)
                    ios_vertices = shape.geometry.verts
                    ios_faces = shape.geometry.faces

                    grouped_verts = [[ios_vertices[i], ios_vertices[i + 1], ios_vertices[i + 2]] for i in range(0, len(ios_vertices), 3)]
                    grouped_faces = [[ios_faces[i], ios_faces[i + 1], ios_faces[i + 2]] for i in range(0, len(ios_faces), 3)]

                    meshinfo.append([grouped_verts, grouped_faces])
                except:
                    print("FAILED: shape creation")
            meshes = []
            for geometry in meshinfo:
                vertices = o3d.utility.Vector3dVector(np.asarray(geometry[0]))
                triangles = o3d.utility.Vector3iVector(np.asarray(geometry[1]))

                mesh = o3d.geometry.TriangleMesh(vertices,triangles)

                meshes.append(mesh)
            pcd_floor = o3d.geometry.PointCloud()
            pcd_ceiling = o3d.geometry.PointCloud()

            for submesh in meshes:
                floor_indeces = []
                ceiling_indeces = []
                mesh_points = round(submesh.get_surface_area() * 1000)
                submeshpcd = submesh.sample_points_uniformly(number_of_points = mesh_points, use_triangle_normal=True)
                
                i = 0
                length = len(submeshpcd.points)
                while i < length:
                    rmsxy = np.sqrt(np.square(submeshpcd.normals[i][0]) + np.square(submeshpcd.normals[i][1]))
                    z = submeshpcd.normals[i][2]
                    if rmsxy <= np.abs(z):
                        if z > 0:
                            floor_indeces.append(i)
                        if z < 0:
                            ceiling_indeces.append(i)
                    i= i+1
                subcloudfloor = submeshpcd.select_by_index(floor_indeces)
                subcloudceiling = submeshpcd.select_by_index(ceiling_indeces)
                pcd_floor.__iadd__(subcloudfloor)
                pcd_ceiling.__iadd__(subcloudceiling)
    
    downsampled_Floor_pointcloud = pcd_floor.voxel_down_sample(voxel_size)
    downsampled_Ceiling_pointcloud = pcd_ceiling.voxel_down_sample(voxel_size)

    return (downsampled_Floor_pointcloud,downsampled_Ceiling_pointcloud)

def IFCtoO3d_Slabs_roofs(ifc_folder_path, voxel_size = 0.01):
    settings = geom.settings()
    settings.set(settings.USE_WORLD_COORDS, True)
    meshinfo = []

    for ifc_file_path in os.listdir(ifc_folder_path):
        if ifc_file_path.endswith(".ifc"):
            ifc_file = ifcopenshell.open(os.path.join(ifc_folder_path,ifc_file_path))
            for ifc_entity in ifc_file.by_type("IfcRoof"):
                # print(ifc_entity)
                try: 
                    shape = geom.create_shape(settings, ifc_entity)
                    ios_vertices = shape.geometry.verts
                    ios_faces = shape.geometry.faces

                    grouped_verts = [[ios_vertices[i], ios_vertices[i + 1], ios_vertices[i + 2]] for i in range(0, len(ios_vertices), 3)]
                    grouped_faces = [[ios_faces[i], ios_faces[i + 1], ios_faces[i + 2]] for i in range(0, len(ios_faces), 3)]

                    meshinfo.append([grouped_verts, grouped_faces])
                except:
                    print("FAILED: shape creation")
            meshes = []
            for geometry in meshinfo:
                vertices = o3d.utility.Vector3dVector(np.asarray(geometry[0]))
                triangles = o3d.utility.Vector3iVector(np.asarray(geometry[1]))

                mesh = o3d.geometry.TriangleMesh(vertices,triangles)

                meshes.append(mesh)
            pcd_roof = o3d.geometry.PointCloud()
            pcd_ceiling = o3d.geometry.PointCloud()

            for submesh in meshes:
                roof_indeces = []
                ceiling_indeces = []
                mesh_points = round(submesh.get_surface_area() * 1000)
                submeshpcd = submesh.sample_points_uniformly(number_of_points = mesh_points, use_triangle_normal=True)
                
                i = 0
                length = len(submeshpcd.points)
                while i < length:
                    rmsxy = np.sqrt(np.square(submeshpcd.normals[i][0]) + np.square(submeshpcd.normals[i][1]))
                    z = submeshpcd.normals[i][2]
                    if rmsxy <= np.abs(z):
                        if z > 0:
                            roof_indeces.append(i)
                        if z < 0:
                            ceiling_indeces.append(i)
                    i= i+1
                subcloudroof = submeshpcd.select_by_index(roof_indeces)
                subcloudceiling = submeshpcd.select_by_index(ceiling_indeces)
                pcd_roof.__iadd__(subcloudroof)
                pcd_ceiling.__iadd__(subcloudceiling)
    
    downsampled_Roof_pointcloud = pcd_roof.voxel_down_sample(voxel_size)
    downsampled_Ceiling_pointcloud = pcd_ceiling.voxel_down_sample(voxel_size)

    return (downsampled_Roof_pointcloud,downsampled_Ceiling_pointcloud)

def generate_refpcd(class_config):

    meshpcd = o3d.geometry.PointCloud()
    labels = []
    for Class in class_config:
        pcd = Class[3]
        key = Class[2]
        length = len(pcd.points)
        i = 0
        while i < length:
            labels.append(key)
            i=i+1
        meshpcd.__iadd__(pcd)
    return [meshpcd, labels]

#computations

def generate_trainingdata(ref, pcd_folder_path, voxel_size = 0.01, c2c_treshold = 0.05, search_radius = 0.05):
    Inlier_clouds = []
    Inlier_labels = []
    Clutter_clouds = []

    for pcd_file_path in os.listdir(pcd_folder_path):
        if pcd_file_path.endswith(".pcd"):
            pcd = o3d.io.read_point_cloud(os.path.join(pcd_folder_path,pcd_file_path))
            print(pcd)            
            refpcd = ref[0]
            ref_kdtree = o3d.geometry.KDTreeFlann(refpcd)
            labels = ref[1]

            pcd = pcd.voxel_down_sample(voxel_size)
            if not pcd.has_normals():
                pcd.estimate_normals()
            print(pcd)
            c2c = pcd.compute_point_cloud_distance(refpcd)

            Inlier_1_indeces = []
            Outlier_indeces = []

            i=0
            while i < len(c2c):
                if c2c[i] <= c2c_treshold:
                    Inlier_1_indeces.append(i)
                elif c2c[i] > c2c_treshold:
                    Outlier_indeces.append(i)
                i=i+1
    
            Inlier_2_indeces = []
            Final_inlier_labels = []
            for index in Inlier_1_indeces:
                [k, idx, d] = ref_kdtree.search_radius_vector_3d(pcd.points[index], search_radius) #Neighbour Search radius 10cm 
                Not_found = True
                i1=0
                while Not_found and i1 < len(idx) and len(idx) > 0:
                    if np.abs(np.dot(np.asarray(pcd.normals[index]), np.asarray(refpcd.normals[idx[i1]]))) > 0.9 and np.abs(d[i1]) < c2c_treshold/5 or np.abs(np.dot(np.asarray(pcd.normals[index]), np.asarray(refpcd.normals)[idx[i1],:])) > 0.7 and np.abs(d[i1]) < c2c_treshold/10:
                        Not_found = False
                        Inlier_2_indeces.append(index)
                        Final_inlier_labels.append(labels[idx[i1]])
                        # Final_LOAs.append(d[i1]) 
                    i1 = i1+1

                if Not_found:
                    Outlier_indeces.append(index)
            Final_inlier_pcd = pcd.select_by_index(Inlier_2_indeces)
            Clutter_pcd = pcd.select_by_index(Outlier_indeces)
            Inlier_clouds.append(Final_inlier_pcd)
            Inlier_labels.append(Final_inlier_labels)
            Clutter_clouds.append(Clutter_pcd)

    inlier_pcd = o3d.geometry.PointCloud()
    inlier_label = []

    if len(Inlier_clouds) > 1 and len(Inlier_labels) == len(Inlier_clouds):
        
        id = 0
        while id < len(Inlier_clouds):
            pcd = Inlier_clouds[id]
            inlier_pcd.__iadd__(pcd)
            i = 0
            while i < len(Inlier_clouds[id].points):
                inlier_label.append(Inlier_labels[id][i])
                i = i+1
            id = id + 1
    elif len(Inlier_clouds) == 1 and len(Inlier_labels) == len(Inlier_clouds):
        inlier_pcd = Inlier_clouds[0]
        inlier_label = Inlier_labels[0]
    else:
        print("ERROR: Need same amount of pointclouds as label arrays")

    Clutter_pcd = o3d.geometry.PointCloud()

    if len(Clutter_clouds) > 1:
        id = 0
        while id < len(Clutter_clouds):
            pcd = Clutter_clouds[id]
            Clutter_pcd.__iadd__(pcd)
            id = id + 1
    else:
        Clutter_pcd = Clutter_clouds[0]

    return (inlier_pcd, inlier_label, Clutter_pcd)

def Split_trainingsdata(pointcloudinfo, class_config, clutter = False):
    pointcloud = pointcloudinfo[0]
    labels = pointcloudinfo[1]
    clutter_pcd = pointcloudinfo[2]
    
    cloud_indeces = []
    clutter_indeces = []
    for Class in class_config:
        cloud_indeces.append(((Class[0],Class[2]), []))
    
    i=0
    while i < len(np.asarray(pointcloud.points)):
        label = labels[i]
        Not_classified = True
        nr = 0
        while Not_classified and nr < len(class_config):
            if label == class_config[nr][2]:
                cloud_indeces[nr][1].append(i)
                Not_classified = False
            else:
                nr = nr+1
        if Not_classified:
            clutter_indeces.append(i)

        i=i+1
    clouds = []
    for indeces in cloud_indeces:
        cloud = pointcloud.select_by_index(indeces[1])
        clouds.append((indeces[0][0],cloud))

    if clutter:
        additional_clutter = pointcloud.select_by_index(clutter_indeces)
        clutter_pcd = clutter_pcd.__iadd__(additional_clutter)
        clouds.append(("Clutter", clutter_pcd))

    return(clouds)


#exports
def Save_trainingsdata(pointclouds, directory, name = "Trainingset"):

    trainingdata_directory = os.path.join(directory, "TrainingData")
    if not os.path.exists(trainingdata_directory):
        os.mkdir(trainingdata_directory)
    trainingdata_project__directory = trainingdata_directory + "\\" + name + "_"+ time.strftime("%Y%m%d-%H%M%S")
    if not os.path.exists(trainingdata_project__directory):
        os.mkdir(trainingdata_project__directory)
    annotations_directory = os.path.join(trainingdata_project__directory, "Annotations")
    if not os.path.exists(annotations_directory):
        os.mkdir(annotations_directory)
    
    
    for pointcloud in pointclouds:
        # print(pointcloud[0] + str(len(np.asarray(pointcloud[1].points))))
        if not len(np.asarray(pointcloud[1].points)) == 0 :
            filename = pointcloud[0] + ".pcd"
            filelocation = annotations_directory + "\\" + filename
            o3d.io.write_point_cloud(filelocation, pointcloud[1])
            filename = pointcloud[0] + ".xyzrgb"
            filelocation = annotations_directory + "\\" + filename
            o3d.io.write_point_cloud(filelocation, pointcloud[1])
        
            filename = pointcloud[0] + ".txt"
            filelocation1 = annotations_directory + "\\" + filename
            os.rename(filelocation,filelocation1)