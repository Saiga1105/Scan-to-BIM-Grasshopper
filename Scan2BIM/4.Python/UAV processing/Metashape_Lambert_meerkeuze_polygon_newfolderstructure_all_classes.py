# =============================================================================
# Dit is een script voor het automatiseren van de stappen voor het uitvoeren van
# de fotogrammetrische bewerkingen in Metashape. Deze stappen bestaan uit:
#     1) Project aanmaken
#     2) Foto's inladen
#     2bis) Automatisch detecteren van markers (optioneel)
#     3) Exif data inlezen en omzetten naar bepaalde projectie die te kiezen is adhv een pop up venster
#     4) De omgezette Exif data inlezen
#     5) Foto's aligneren
#     6) Dense Cloud bouwen
#     7) Mesh model bouwen
#     8) Textuur bouwen
#     9) Model exporteren naar PLY-formaat
#     10) Aanmaken en exporteren van DEM
#     11) Aanmaken en exporteren van orthofoto 
# Voorwaarden:
#     - Metashape met python geïnstalleerd
#     - Script is gebouwd rond python 3.5
# Laatste bewerking: maart 2021
# =============================================================================

import os
import Metashape
import textwrap
import subprocess
import tkinter as tk
from tkinter import ttk

# Checking compatibility
compatible_major_version = "1.7"
found_major_version = ".".join(Metashape.app.version.split('.')[:2])
if found_major_version != compatible_major_version:
    raise Exception("Incompatible Metashape version: {} != {}".format(found_major_version, compatible_major_version))

root = r"C:\metashape"

#Locatie waar in dronevlucht staat selecteren (onder dit mapje zitten alle fotos in de map 'photos') In dit mapje zal alles opgeslaan worden
def getProjectnr():
    global workingdir
    global workingdirtemp1
    global workingdirnew 
    global workingdirnewphoto
    global psxpath
    global workingdirPCD
    global workingdirDEM
    global workingdirOrtho
    global workingdirMesh
    global projection
    global Zoption
    popupmsg("Kies de projectie")
    # Popupvenster laten komen om projectie te kiezen (hoe sneller hoe beter)
    workingdir = Metashape.app.getExistingDirectory("K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames")	
    print (workingdir)
    workingdirtemp1 = os.path.split(workingdir)
    print (workingdirtemp1[0])
    print (workingdirtemp1[1])
    workingdirtemp2 = os.path.split(workingdirtemp1[0])
    print (workingdirtemp2[0])
    workingdirnew = os.path.split(workingdirtemp2[0])
    print ("workingdirnew :" + workingdirnew[0])
    workingdirnewphoto = str(workingdirnew[0]) + "/Photoscan" + "/" + str(workingdirtemp1[1])
    workingdirPCD = str(workingdirnew[0]) + "/Pointclouds"  + "/" + str(workingdirtemp1[1])
    workingdirDEM = str(workingdirnew[0]) + "/DEM"  + "/" + str(workingdirtemp1[1])
    workingdirOrtho = str(workingdirnew[0]) + "/Ortho"  + "/" + str(workingdirtemp1[1])
    workingdirMesh = str(workingdirnew[0]) + "/Meshes"   + "/" + str(workingdirtemp1[1])
    print ("workingdirnewphoto :" + workingdirnewphoto)
    psxpath = str(workingdirnewphoto) + "/MetashapeOutput"+ projection + "_" + Zoption + ".psx"
    #psxpath aanpassen afhankelijk van de gekozen projectie
    print("workdir: " +str(workingdirnew))
    print("psxpath: " +str(psxpath))

    os.makedirs(workingdirnewphoto, exist_ok=True)
    os.makedirs(workingdirPCD, exist_ok=True)
    os.makedirs(workingdirDEM, exist_ok=True)
    os.makedirs(workingdirOrtho, exist_ok=True)
    os.makedirs(workingdirMesh, exist_ok=True)
    return psxpath

    

#saves new project @ psxpath
def m1_createProject():
    # Create and save project
    import shutil
    print("::::::::::::::::::::::::::::::::::::::CREATING PROJECT::::::::::::::::::::::::::::::::::::::")
    doc = Metashape.app.document
    for file in os.listdir(): # Remove already existing projects
        if file.endswith('.files'):
            shutil.rmtree(os.getcwd()+"\\"+file)
        elif file.endswith('.psx'):
            os.remove(os.getcwd()+"\\"+file)
        elif file.endswith('.ply'):
            os.remove(os.getcwd()+"\\"+file)
        elif file.endswith('.jpg'):
            os.remove(os.getcwd()+"\\"+file)    
    doc.save(path = psxpath) # To add: if project already exist, delete and create new one

#laad foto's uit savdir_psx/photos
#INIT Precalibrated camera (currently gopro) -> verander naar dji phantom 4
def m2_addPhotos():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Add photos
    print("::::::::::::::::::::::::::::::::::::::LOADING PHOTOS::::::::::::::::::::::::::::::::::::::")
    # workin directory photos must be in save_dir/photos
    photosDir = workingdir+"/photos"
    def absoluteFilePaths(directory):
        listimages = []
        print(directory)
        for f in os.listdir(directory):
            print(f)
            if f.endswith('.JPG'):
                print(f)
                listimages.append(os.path.join(directory,f))
        return listimages

    listPhotos = absoluteFilePaths(photosDir) 
    print(listPhotos)
    print(photosDir)
    #PUT CHECK FOR FILES THAT ARE NOT PHOTOS
    #chunk.addPhotos(listPhotos)
    chunk.addPhotos(listPhotos, layout=Metashape.UndefinedLayout, strip_extensions=True,load_reference=True, load_xmp_calibration=True, load_xmp_orientation=True,
    load_xmp_accuracy=False, load_xmp_antenna=True, load_rpc_txt=False)


    print("::::::::::::::::::::::::::::::::::::::CAMERA CALIBRATION::::::::::::::::::::::::::::::::::::::")
    #camera wordt gecalibreerd aan de hand van een geprecalibreerde vlucht die verkregen is op basis van een vlucht op verschillende hoogtes
    chunksensor = chunk.addSensor()
    chunksensor.type = Metashape.Sensor.Type.Frame
    calibration = Metashape.Calibration()
    calibration.load(r"K:\Projects\2022-01 Project VLAIO Bauwens\3. Berekeningen\Camera calibratie v2.xml")
    calibration.width=5472
    calibration.height=3648
    chunksensor.user_calib = calibration
    chunksensor.width=5472
    chunksensor.height=3648
    chunksensor.label="precalibrated"
    
    chunksensor.fixed=True
    chunksensor.fixed_calibration=True

    for cam in chunk.cameras:
        cam.sensor = chunksensor

    
    doc.save(path = psxpath)

    
def m3_detectMarkers():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Detect markers
    print("::::::::::::::::::::::::::::::::::::::DETECTING MARKERS::::::::::::::::::::::::::::::::::::::")
    chunk.detectMarkers(target_type=Metashape.TargetType.CircularTarget12bit, 
                        tolerance=50,
                        maximum_residual=5)
    doc.save(path = psxpath)

def m4_align():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Functions for progress callback
    # =============================================================================
    # def progress_matchPhotos(p):
    #     os.system("start cmd /c")
    #     elapsed = float(time.time() - start_time)
    #     if p:
    #         sec = elapsed / p * 100
    #         print('Matching photos: {:.2f}%, estimated time left: {:.0f} seconds'.format(p, sec))
    #     else:
    #         print('Matching photos: {:.2f}%, estimated time left: unknown'.format(p)) #if 0% progress
    # 
    # def progress_alignPhotos(p):
    #     elapsed = float(time.time() - start_time)
    #     if p:
    #         sec = elapsed / p * 100
    #         print('Aligning photos: {:.2f}%, estimated time left: {:.0f} seconds'.format(p, sec))
    #     else:
    #         print('Aligning photos: {:.2f}%, estimated time left: unknown'.format(p)) #if 0% progress
    # =============================================================================
        
       
    # Aligning photos
    print("::::::::::::::::::::::::::::::::::::::ALIGNING PHOTOS::::::::::::::::::::::::::::::::::::::")
    chunk.matchPhotos(downscale=1,
                      generic_preselection=True, 
                      reference_preselection=False,
                      keypoint_limit=65000,
                      tiepoint_limit=20000,
                      keep_keypoints=True)
    chunk.alignCameras(adaptive_fitting=True)
    doc.save(path = psxpath)

def popupmsg(msg):
    def set1():
        global projection
        global Zoption
        projection = 'LAMBERT08'
        Zoption = 'ELLIPS'
        popup.destroy()
    def set2():
        global projection
        global Zoption
        projection = 'LAMBERT08'
        Zoption = 'TAW'
        popup.destroy()
    def set3():
        global projection
        global Zoption
        projection = 'LAMBERT72'
        Zoption = 'ELLIPS'
        popup.destroy()
    def set4():
        global projection
        global Zoption
        projection = 'LAMBERT72'
        Zoption = 'TAW'
        popup.destroy()
    popup = tk.Tk()
    popup.wm_title("!")
    label = ttk.Label(popup, text=msg)
    label.pack(side="top", fill="x", pady=10)
    B1 = ttk.Button(popup, text="Lambert2008 Ellips", command = set1)
    B1.pack()
    B2 = ttk.Button(popup, text="Lambert2008 TAW", command = set2)
    B2.pack()
    B3 = ttk.Button(popup, text="Lambert1972 Ellips", command = set3)
    B3.pack()
    B4 = ttk.Button(popup, text="Lambert1972 TAW", command = set4)
    B4.pack()
    popup.mainloop()


def m3_2_imageToGCP():
    photosDir = workingdir+"/photos"
    targetDir = os.path.join(photosDir, "Results")
    if not os.path.exists(targetDir):
        os.makedirs(targetDir)
    
    print("::::::::::::::::::::::::::::::::::::::Exif Data Changing::::::::::::::::::::::::::::::::::::::")
    process = subprocess.run([r"K:\Software\Tools\Metashape\WGS2L72\WGS2L72.exe" ,projection, Zoption, photosDir, os.path.join(targetDir, "image_to_gcp.txt"), "0.02", "0.02", "0.05"])
    
    process
    #PS K:\Projects\2022-01 Project VLAIO Bauwens\1. Opnames\Grego\100_0975\photos> K:\Software\Tools\Metashape\WGS2L72\WGS2L72.exe .

 # JENS: Belangrijk! De GPS punten MOETEN voor de laatste bundle adjustment ingeladen worden. Zo wordt het model niet alleen gegeorefereed
 # maar ook worden de camera posities geoptimaliseerd in m5_optimizeCameras. Dit is zeer belangrijk voor de nauwkeurigheid. Anders heb je snel grote fouten (vooral z-as, hoogte) ten gevolge van drift. 
 # bij grote werven kan dit snel 1m+ worden
 # Leest gpspunten uit projectdir/targets
 # todo: assign to camera
 # ImageGPSToGCP.txt

def m4_2_loadRef():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk

    # Load References
    print("::::::::::::::::::::::::::::::::::::::IMPORTING REFERENCE::::::::::::::::::::::::::::::::::::::")

    targetDir = workingdir+"/photos/Results"
    print(targetDir)
    files = os.listdir(targetDir)
    if len(files) > 1:
        print("Er mag slechts 1 bestand aanwezig zijn onder " + targetDir + ". Gelieve de overbodige bestanden te verwijderen en process opnieuw te starten.")
        os.system("pause")
    else:
        try:
            # JENS: Indien er nog steeds markers niet gematched worden met de bijhorende GPS punten kun je de Threshold nog hoger nemen.
            # Let op! Indien gps punten dicht bij elkaar liggen dan kunnen verkeerde gematched worden -> threshold moet terug lager 
            # Mochten er blijvende problemen zijn hiermee: Er is de optie van deze berekening aan het script over te laten (waarbij het script de GPSpunten met gevonden markers matched)
            crs = Metashape.CoordinateSystem('LOCAL_CS["Local Coordinates",LOCAL_DATUM["Local Datum",0],UNIT["metre",1,AUTHORITY["EPSG","9001"]]]')
            chunk.importReference(path=os.path.join(targetDir, files[0]),format=Metashape.ReferenceFormatCSV, columns='nxyzXYZ', delimiter=' ',items=Metashape.ReferenceItemsCameras,ignore_labels=False,threshold=2, create_markers=False,crs=crs)
        except IndexError:
            print("Er ontbreekt een bestand met GPS-coördinaten of het bestand is corrupt in de targets-folder!")
    doc.save(path = psxpath)

def m5_optimizeCameras():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # JENS: voor laatste bundle adjustment worden de camera parameters terug vrij gelaten (deze worden dus nog geoptimaliseerd)
    for cam in chunk.cameras:
        cam.sensor.fixed=False
        cam.sensor.fixed_calibration=False

    # Optimize cames
    print("::::::::::::::::::::::::::::::::::::::OPTIMIZE CAMERAS::::::::::::::::::::::::::::::::::::::")
    chunk.optimizeCameras(fit_f=True,
                         fit_cx=True,
                         fit_cy=True,
                         fit_k1=True,
                         fit_k2=True,
                         fit_k3=True,
                         fit_k4=False,
                         fit_b1=True,
                         fit_b2=False,
                         fit_p1=False,
                         fit_p2=False)
    doc.save(path = psxpath)

def m6_buildCloud():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Build dense cloud
    print("::::::::::::::::::::::::::::::::::::::BUILD DENSE CLOUD::::::::::::::::::::::::::::::::::::::")
    chunk.buildDepthMaps(downscale=4,
                         filter_mode=Metashape.AggressiveFiltering,
                         reuse_depth=False)
     # chunk.buildDepthMaps(downscale=4,
                          # filter=Metashape.AggressiveFiltering, #NoFiltering, MildFiltering, ModerateFiltering, AggressiveFiltering
                          # reuse_depth=False)
    chunk.buildDenseCloud(point_colors=True,
                          keep_depth=True) #####
    #chunk.dense_cloud.classifyGroundPoints(max_angle=15.0, max_distance=0.5, cell_size=50.0)
    doc.save(path = psxpath)

    
def m7_buildMesh():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk  
    
    # Build mesh
    print("::::::::::::::::::::::::::::::::::::::BUILD MESH::::::::::::::::::::::::::::::::::::::")
    chunk.buildModel(surface_type=Metashape.Arbitrary,
                     interpolation=Metashape.EnabledInterpolation,
                     face_count=Metashape.MediumFaceCount,
                     source_data=Metashape.DenseCloudData,
                     vertex_colors=True,)
    doc.save(path = psxpath)

def m8_buildTexture():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Build texture
    print("::::::::::::::::::::::::::::::::::::::BUILD TEXTURE::::::::::::::::::::::::::::::::::::::")
    chunk.buildUV(mapping_mode=Metashape.GenericMapping,
                  page_count=1,
                  texture_size=4096)
    chunk.buildTexture(blending_mode=Metashape.MosaicBlending,
                       texture_size=8192,
                       fill_holes=True,
                       ghosting_filter=False)
    doc.save(path = psxpath)

# JENS: Zie m4_2, identieke kopie v/d functie    
# def m9_loadRef():
    # # Open document
    # doc = Metashape.app.document
    # #doc.open(psxpath)
    # chunk = doc.chunk
    
    # # Load References
    # print("::::::::::::::::::::::::::::::::::::::IMPORTING REFERENCE::::::::::::::::::::::::::::::::::::::")
    # targetsDir = os.getcwd() + '\\targets'
    # files = os.listdir(targetsDir)
    # if len(files) > 1:
        # print("Er mag slechts 1 bestand aanwezig zijn onder " + targetsDir + ". Gelieve de overbodige bestanden te verwijderen en process opnieuw te starten.")
        # os.system("pause")
    # else:
        # try:
            # chunk.loadReference(path=targetsDir+'\\'+files[0], 
                                # format=Metashape.ReferenceFormatCSV, 
                                # columns='nxyz', 
                                # delimiter='\t',
                                # items=Metashape.ReferenceItemsMarkers,
                                # ignore_labels=True,
                                # threshold=2,
                                # create_markers=False)
        # except IndexError:
            # print("Er ontbreekt een bestand met GPS-coördinaten of het bestand is corrupt in de targets-folder!")
    # doc.save(path = psxpath)

def m9_importShape():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    # import shape
    if os.path.isfile(os.path.join(workingdir, "polyline.dxf")):
        print("::::::::::::::::::::::::::::::::::::::IMPORT SHAPE::::::::::::::::::::::::::::::::::::::")
        chunk.importShapes(path=os.path.join(workingdir, "polyline.dxf"), replace=True, boundary_type=Metashape.Shape.OuterBoundary, format=Metashape.ShapesFormatDXF)
        doc.save(path = psxpath)
# 
def m10_exportPLY():
    # Open document
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    
    # Export to .ply
    print("::::::::::::::::::::::::::::::::::::::EXPORT TO .PLY::::::::::::::::::::::::::::::::::::::")
    
    filename = "Mesh"+ projection + "_" + Zoption + ".ply"
    chunk.exportModel(path= os.path.join(workingdirMesh, filename),
                      binary=False,
                      texture_format=Metashape.ImageFormatJPEG,
                      save_texture=True,
                      save_normals=False,
                      save_colors=False,
                      save_cameras=False,
                      save_markers=False,
                      save_udim=False,
                      save_alpha=False,
                      format=Metashape.ModelFormatPLY)
    doc.save(path = psxpath)
    
def m11_exportOrtho():
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    filename = "Ortho"+ projection + "_" + Zoption + ".tif"
    print("Generate Ortho")
    # chunk.buildOrthomosaic(surface=Metashape.ModelData, blending=Metashape.MosaicBlending, fill_holes=True, cull_faces=False, refine_seamlines=False, flip_x=False, flip_y=False, flip_z=False)
    chunk.buildOrthomosaic(surface_data=Metashape.ModelData, blending_mode=Metashape.MosaicBlending, fill_holes=True,ghosting_filter=False, cull_faces=False, refine_seamlines=False, resolution=0.005, resolution_x=0.005, resolution_y=0.005, flip_x=False,flip_y=False, flip_z=False, subdivide_task=True, workitem_size_cameras=20,workitem_size_tiles=10, max_workgroup_size=100)

                           
    #chunk.exportOrthophotos(path= os.path.join(workingdir, filename), raster_transform=Metashape.RasterTransformNone)

    # chunk.exportOrthomosaic(path= os.path.join(workingdir, filename), raster_transform=Metashape.RasterTransformNone, write_kml=False, write_world=False,write_scheme=False, write_alpha=True,tiff_compression=Metashape.TiffCompressionLZW, tiff_big=False, tiff_tiled=True,tiff_overviews=True, jpeg_quality=90, network_links=True, white_background=True)
    chunk.exportRaster(path=os.path.join(workingdirOrtho, filename), format=Metashape.RasterFormatTiles, image_format=Metashape.ImageFormatNone,
    raster_transform=Metashape.RasterTransformNone, resolution=0.005,
    resolution_x=0.005, resolution_y=0.005, block_width=10000, block_height=10000,
    split_in_blocks=False, width=0, height=0, nodata_value=-
    32767, save_kml=False, save_world=False, save_scheme=False, save_alpha=True,
    image_description="Orthomosaic", network_links=True,
    min_zoom_level=-1, max_zoom_level=-1, white_background=True,
    clip_to_boundary=True, title="Orthomosaic", description="Generated by Agisoft Metashape", source_data=Metashape.OrthomosaicData, north_up=True, tile_width=256,
    tile_height=256)
                     
    doc.save(path = psxpath)

def m12_exportDEM():
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    filename = "Dem"+ projection + "_" + Zoption + ".tif"
    chunk.buildDem(source_data=Metashape.ModelData, interpolation=Metashape.EnabledInterpolation, flip_x=False, flip_y=False, flip_z=False, classes=[Metashape.Ground],resolution=0.005,subdivide_task=True,workitem_size_tiles=10, max_workgroup_size=100)

    chunk.exportRaster(path=os.path.join(workingdirDEM, filename), format=Metashape.RasterFormatTiles, image_format=Metashape.ImageFormatNone,
    raster_transform=Metashape.RasterTransformNone, resolution=0.005,
    resolution_x=0.005, resolution_y=0.005, block_width=10000, block_height=10000,
    split_in_blocks=False, width=0, height=0, nodata_value=-
    32767, save_kml=False, save_world=False, save_scheme=False, save_alpha=True,
    image_description="DEM", network_links=True,
    min_zoom_level=-1, max_zoom_level=-1, white_background=True,
    clip_to_boundary=True, title="DEM", description="Generated by Agisoft Metashape", source_data=Metashape.ElevationData, north_up=True, tile_width=256,
    tile_height=256)


    # chunk.exportDem(path=os.path.join(workingdir, filename), raster_transform=Metashape.RasterTransformNone, nodata=-32767, write_kml=False, write_world=False, write_scheme=False, tiff_big=False, tiff_tiled=True, tiff_overviews=True, network_links=True)

    doc.save(path = psxpath)

def m13_exportPCD():
    doc = Metashape.app.document
    #doc.open(psxpath)
    chunk = doc.chunk
    filename = "PCD"+ projection + "_" + Zoption + ".e57"

    chunk.exportPoints(path=os.path.join(workingdirPCD, filename), source_data=Metashape.DenseCloudData, binary=True, save_normals=True,
    save_colors=True, save_classes=True, save_confidence=True,
    raster_transform=Metashape.RasterTransformNone, colors_rgb_8bit=True, save_comment=True, format=Metashape.PointsFormatNone, image_format=Metashape.ImageFormatJPEG, clip_to_boundary=True,
    block_width=1000, block_height=1000, split_in_blocks=False, classes=[Metashape.Ground],save_images=False, subdivide_task=True)


    # chunk.exportDem(path=os.path.join(workingdir, filename), raster_transform=Metashape.RasterTransformNone, nodata=-32767, write_kml=False, write_world=False, write_scheme=False, tiff_big=False, tiff_tiled=True, tiff_overviews=True, network_links=True)

    doc.save(path = psxpath)

def Main():
    # Enable GPU and CPU
    Metashape.app.cpu_enable
    Metashape.app.gpu_mask = 2 ** (len(Metashape.app.enumGPUDevices())) - 1

    getProjectnr()
    m1_createProject()
    m2_addPhotos()
    m3_detectMarkers()
    m3_2_imageToGCP()
    m4_2_loadRef()
    m4_align()
    # m5_optimizeCameras()
    m6_buildCloud()
    m7_buildMesh()
    m8_buildTexture()
    m13_exportPCD()
    m10_exportPLY()
    m9_importShape()
    m12_exportDEM()
    m11_exportOrtho()

if __name__ == '__main__':
    Main()
