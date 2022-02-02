"""
ImageNode - a Python Class to govern the data and metadata of image data (JPG,PNG,XML,XMP)
"""
#IMPORT PACKAGES
from ast import Try
from logging import NullHandler
from pathlib import Path
from typing import Type
from urllib.request import ProxyBasicAuthHandler
import xml.etree.ElementTree as ET
from xmlrpc.client import Boolean 
import cv2 
import open3d as o3d 
# from pathlib import Path # https://docs.python.org/3/library/pathlib.html
import numpy as np 
import os
import sys
import math

# from pathvalidate import ValidationError, validate_filename # conda install -c thombashi pathvalidate

import rdflib #https://rdflib.readthedocs.io/en/stable/
# from rdflib import Graph, plugin
# from rdflib.serializer import Serializer #pip install rdflib-jsonld https://pypi.org/project/rdflib-jsonld/
from rdflib import Graph
from rdflib import URIRef, BNode, Literal
from rdflib.namespace import CSVW, DC, DCAT, DCTERMS, DOAP, FOAF, ODRL2, ORG, OWL, \
                           PROF, PROV, RDF, RDFS, SDO, SH, SKOS, SOSA, SSN, TIME, \
                           VOID, XMLNS, XSD
import PIL
import PIL.Image as PILimage
from PIL import ImageDraw, ImageFont, ImageEnhance
from PIL.ExifTags import TAGS, GPSTAGS
from scipy.spatial.transform import Rotation as R

import pye57 #conda install xerces-c  =>  pip install pye57

#IMPORT MODULES
import Algorithms.scan2bim as s2b
import Algorithms.trainingdata as td
import Algorithms.linkeddatatools as ld

SUPPORTED_POINT_FIELDS = {
    "name": "string",
    "guid": "string",
    "session_name": "string",
    "timestamp": "string",
    "Pose": "Pose(Translation(tx,ty,tz),Quaternion(qw,qx,qy,qz)",
    "GlobalPose": "GlobalPose(SphericalTranslation(lat,long,alt),Quaternion(qw,qx,qy,qz)",
    "CartesianBounds": "CartesianBounds(x_min,x_max,y_min,y_max,z_min,z_max)",
    "point_count": "int",
    "accuracy": "float",
    "cartesian_transform": "Pose(Translation(tx,ty,tz),Quaternion(qw,qx,qy,qz)",
    "geospatial_transform": "GlobalPose(SphericalTranslation(lat,long,alt),Quaternion(qw,qx,qy,qz)",
    "coordinate_system": "string (Lambert72, Lambert2008, geospatial-wgs84, local)",
    "session_path": "string",
    "e57_xml_path": "string",
    "e57_path": "string",
    "pcd_path": "string",
    "rdf_graph_path": "string",
    "images2d_path": "string",
    "features3d_path": "string",
    "rdf_graph": "Graph (RDFLIB)",
    "e57_xml_node": " E57 NODE (PYE57)",
    "e57_index": "int (PYE57)",
    "images2D": "ImageNode[] (IMAGENODE)",
}

class ImageNode:
    # class attributes
    
    def __init__(self):
        #instance attributes
        self.name = None # (string) PointCloudNode name => these are instance attributes
        self.guid = None # (string) PointCloudNode guid
        self.session_name = None  # (string) session subject name
        self.timestamp = None  # (string) e.g. 2020-04-11 12:00:01

        #Geometry
        self.Pose = None # (structure) Translation(tx,ty,tz), Quaternion(qw,qx,qy,qz)
        self.GlobalPose=None # (structure) SphericalTranslation(lat,long,alt), Quaternion(qw,qx,qy,qz)
        self.accuracy = None # (Float) metric data accuracy e.g. 0.05m
        self.xResolution = None # (Float) 
        self.yResolution = None # (Float) 
        self.resolutionUnit = None # (string)
        self.image_width = None # (int) number of pixels
        self.image_height = None  # (int) number of pixels
        self.focal_length = None # (Float) focal length in mm
        self.principal_point_u= None # (Float) u parameter of principal point (mm)
        self.principal_point_v= None # (Float) v parameter of principal point (mm)
        self.distortion_coeficients = None # (Float[])         
        
        #Coordinate system information
        self.cartesian_transform=None # (offset)a 3D to 3D transform offering a change of origin, scale and rotation. Represented as a matrix and a quaternion.
        self.geospatial_transform=None # (offset) a transform from a 3D Cartesian coordinate system into an ellipsoidal GNSS style coordinate system. E.g. from map-UTM to WGS84 latitude, longitude, altitude.
        self.coordinate_system = None# (string) coordinate system i.e. Lambert72, Lambert2008, geospatial-wgs84, local

        #paths
        self.session_path = None # (string)
        self.xml_path = None # (string)
        self.xmp_path = None # (string)
        self.img_path = None # (string)
        self.rdf_graph_path = None # (string)
        self.features2d_path= None # (string)

        #metadata
        self.rdf_graph = Graph # (rdflib Graph)

        #data
        self.img= None # PIL image
        self.o3d_images = [] # (o3d.geometry.RGBDImage) # Open3D list of images
        self.features2d= None #o3d.registration.Feature() # http://www.open3d.org/docs/0.9.0/python_api/open3d.registration.Feature.html

    def add_to_rdf_graph(self):
        g=Graph()
        # bind additional ontologies that aren't in rdflib
        exif = rdflib.Namespace('http://www.w3.org/2003/12/exif/ns')
        g.bind('exif', exif)
        geo=rdflib.Namespace('http://www.opengis.net/ont/geosparql#') #coordinate system information
        g.bind('geo', geo)
        gom=rdflib.Namespace('https://w3id.org/gom#') # geometry representations => this is from mathias
        g.bind('gom', gom)
        omg=rdflib.Namespace('https://w3id.org/omg#') # geometry relations
        g.bind('omg', omg)
        fog=rdflib.Namespace('https://w3id.org/fog#')
        g.bind('fog', fog)
        v4d=rdflib.Namespace('https://w3id.org/v4d/core#')
        g.bind('v4d3D', v4d)
        v4d3D=rdflib.Namespace('https://w3id.org/v4d/3D#')
        g.bind('v4d3D', v4d3D)
        openlabel=rdflib.Namespace('https://www.asam.net/index.php?eID=dumpFile&t=f&f=3876&token=413e8c85031ae64cc35cf42d0768627514868b2f')
        g.bind('openlabel', openlabel)
        e57=rdflib.Namespace('http://libe57.org/')
        g.bind('e57', e57)
        xcr=rdflib.Namespace('http://www.w3.org/1999/02/22-rdf-syntax-ns#')
        g.bind('xcr', xcr)

        # Create an RDF URI node to use as the subject for multiple triples
        subject = URIRef('http://'+ self.name.replace(' ','_')) 

        # Add triples using store's add() method.
        g.add((subject, RDF.type, Literal(v4d3D.ImageNode)))
        g.add((subject, RDFS.label, Literal(self.guid)))
        g.add((subject, openlabel.timestamp, Literal(self.timestamp)))
        #g.add((subject, openlabel.frame, Literal(self.session_name)))

        #coordinate system information
        g.add((subject, gom.hasCoordinateSystem, Literal(self.coordinate_system)))
        g.add((subject, openlabel.cartesian_transform, Literal(self.cartesian_transform))) # these are offsets
        g.add((subject, openlabel.geospatial_transform, Literal(self.geospatial_transform)))# these are offsets

        #geometries
        g.add((subject, openlabel.quaternion, Literal(str(self.get_rotation_quaternion()))))
        g.add((subject, openlabel.pose, Literal(str(self.get_pose()))))
        g.add((subject, openlabel.global_pose, Literal(str(self.get_global_pose()))))
        g.add((subject, xcr.DistortionCoeficients, Literal(str(self.distortion_coeficients))))

        g.add((subject, v4d.accuracy, Literal(str(self.accuracy))))
        g.add((subject,exif.imageWidth, Literal(self.image_width)))
        g.add((subject,exif.imageLength, Literal(self.image_height)))
        g.add((subject,exif.xResolution, Literal(self.xResolution)))
        g.add((subject,exif.yResolution, Literal(self.yResolution)))
        g.add((subject,exif.resolutionUnit, Literal(self.resolutionUnit)))
        g.add((subject,xcr.FocalLength35mm, Literal(self.focal_length)))
        g.add((subject,xcr.PrincipalPointU, Literal(self.principal_point_u)))
        g.add((subject,xcr.PrincipalPointV, Literal(self.principal_point_v)))

        #paths
        g.add((subject, v4d.session_path, Literal(self.session_path)))
        g.add((subject, v4d.xml_path, Literal(self.xml_path)))
        g.add((subject, v4d.xmp_path, Literal(self.xmp_path)))
        g.add((subject, v4d.img_path, Literal(self.img_path)))
        g.add((subject, v4d.features2d_path, Literal(self.features2d_path)))

        self.rdf_graph=g        

    def get_rotation_quaternion(self):
        if self.Pose is not None:
            return [self.Pose.Quaternion.qw,self.Pose.Quaternion.qx,self.Pose.Quaternion.qy,self.Pose.Quaternion.qz]
        else:
            return None

    def get_pose(self):
        if self.Pose is not None:
            return [self.Pose.Translation.tx,self.Pose.Translation.ty,self.Pose.Translation.tz,
                self.Pose.Quaternion.qw,self.Pose.Quaternion.qx,self.Pose.Quaternion.qy,self.Pose.Quaternion.qz]
        else:
            return None

    def get_global_pose(self):
        if self.GlobalPose.SphericalTranslation.lat is not None:
            return [self.GlobalPose.SphericalTranslation.lat,self.GlobalPose.SphericalTranslation.long,self.GlobalPose.SphericalTranslation.alt,
                    self.GlobalPose.Quaternion.qw,self.GlobalPose.Quaternion.qx,self.GlobalPose.Quaternion.qy,self.GlobalPose.Quaternion.qz]
        else:
            return None

    def get_img_path(self):
        all_session_file_paths=ld.getListOfFiles(self.session_path) 
        img_file_paths=[] 

        for file_path in all_session_file_paths:        
            if file_path.endswith(".JPG") or file_path.endswith(".png"): 
                img_file_paths.append(file_path)
        for file_path in img_file_paths:
            if self.name in file_path:
                self.img_path=file_path
                return True
        return False    

    def create_imgnode_from_xmp(xmp_path : str):
        imgnode=ImageNode

        try:
            #parse xmp file with rdflib
            imgnode.rdf_graph.parse(xmp_path)

            # imgnode.Pose = xcr:Position="46.8117249478181 74.321387248499 5.38060365736147"
            # imgnode.focal_length= xcr:FocalLength35mm="24.4831677041283"
            # imgnode.principal_point_u = xcr:PrincipalPointU="0.00115727956155745"
            # imgnode.principal_point_v = xcr:PrincipalPointV="0.00115727956155745"

            # xcr:latitude="179.992764913047210N" xcr:longitude="57.795004816049143E" xcr:altitude="642269440/10000"

            return imgnode
        except:
            print('unable to parse xmp file with RDFLIB')
            return None

    def get_exif_data(self):
        """Returns a dictionary from the exif data of an PIL Image item. Also
        converts the GPS Tags"""
        exif_data = {}
        info = self.img._getexif()
        if info:
            for tag, value in info.items():
                decoded = TAGS.get(tag, tag)
                if decoded == "GPSInfo":
                    gps_data = {}
                    for t in value:
                        sub_decoded = GPSTAGS.get(t, t)
                        gps_data[sub_decoded] = value[t]

                    exif_data[decoded] = gps_data
                else:
                    exif_data[decoded] = value
            self.exif_data=exif_data        
        # return exif_data

    def set_exif_data(self):
        self.timestamp=get_if_exist(self.exif_data, "DateTime")
        self.xResolution=get_if_exist(self.exif_data,"XResolution")
        self.yResolution=get_if_exist(self.exif_data,"YResolution")
        self.resolutionUnit=get_if_exist(self.exif_data,"ResolutionUnit")
        self.image_width=get_if_exist(self.exif_data,"ExifImageWidth")
        self.image_height=get_if_exist(self.exif_data,"ExifImageHeight")
        
        if 'GPSInfo' in self.exif_data:
            gps_info = self.exif_data["GPSInfo"]
            if gps_info is not None:
                self.GlobalPose=GlobalPose # (structure) SphericalTranslation(lat,long,alt), Quaternion(qw,qx,qy,qz)
                self.GlobalPose.SphericalTranslation=SphericalTranslation(lat=get_if_exist(gps_info, "GPSLatitude"),
                                                                        long=get_if_exist(gps_info, "GPSLongitude"),
                                                                        alt=get_if_exist(gps_info, "GPSAltitude"))
                self.GlobalPose.Quaternion=Quaternion
            # consider get_if_exist(gps_info, "GPSLatitudeRef")
            # consider get_if_exist(gps_info, "GPSLongitudeRef")
            # consider get_if_exist(gps_info, "GPSAltitudeRef")

    def read_img_xmp(self , xmp_path : str):
        mytree = ET.parse(xmp_path)
        root = mytree.getroot()       

        for img_description in root.iter('{http://www.w3.org/1999/02/22-rdf-syntax-ns#}Description'):
            self.focal_length=float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm'])
            self.principal_point_u=float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}PrincipalPointU'])
            self.principal_point_v=float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}PrincipalPointV'])
            # self.xResolution =float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm'])
            # self.yResolution =float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm'])
            # self.resolutionUnit=img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm']
            # self.image_width=float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm'])
            # self.image_length =float(img_description.attrib['{http://www.capturingreality.com/ns/xcr/1.1#}FocalLength35mm'])

            self.Pose=Pose()
            rotationnode=img_description.find('{http://www.capturingreality.com/ns/xcr/1.1#}Rotation')
            if rotationnode is not None:
                rotation_matrix=string_to_rotation_matrix(rotationnode.text)
                r=R.from_matrix(rotation_matrix) 
                quat=r.as_quat()
                self.Pose.Quaternion=Quaternion(qw=quat[0],qx=quat[1],qy=quat[2],qz=quat[3])

            positionnode=img_description.find('{http://www.capturingreality.com/ns/xcr/1.1#}Position')
            if positionnode is not None:
                self.Pose.Translation.tx=float(positionnode.text.split(' ')[0])
                self.Pose.Translation.ty=float(positionnode.text.split(' ')[1])
                self.Pose.Translation.tz=float(positionnode.text.split(' ')[2])

            coeficientnode=img_description.find('{http://www.capturingreality.com/ns/xcr/1.1#}DistortionCoeficients')
            if coeficientnode is not None:
                self.distortion_coeficients=string_to_array(coeficientnode.text)     

###################################################################################################################
###################################################################################################################
class Pose ():
    def __init__(self):
        self.Translation=Translation()
        self.Quaternion=Quaternion()

class GlobalPose ():
    def __init__(self):
        self.SphericalTranslation=SphericalTranslation()
        self.Quaternion=Quaternion()

class SphericalTranslation():
    def __init__(self,lat=0.0,long=0.0,alt=0.0):
        self.lat=lat #decimal
        self.long=long #decimal
        self.alt=alt #decimal

class Translation ():
    
    def __init__(self,tx=0.0,ty=0.0,tz=0.0):
        self.tx = tx
        self.ty = ty
        self.tz = tz

class Quaternion ():
    
    def __init__(self,qw=1.0,qx=0.0,qy=0.0,qz=0.0): # initialized within rotation
        self.qw = qw        
        self.qx = qx
        self.qy = qy
        self.qz = qz

class Rotation():
    def __init__(self,rx=0.0,ry=0.0,rz=0.0):
        self.rx = rx #roll
        self.ry = ry #pitch
        self.rz = rz #yaw

class CartesianBounds ():
   
    def __init__(self,x_min=0.0,x_max=0.0,y_min=0.0,y_max=0.0,z_min=0.0,z_max=0.0):
        self.xMinimum = x_min
        self.xMaximum = x_max
        self.yMinimum = y_min
        self.yMaximum = y_max
        self.zMinimum = z_min
        self.zMaximum = z_max

class Features3D():
    def __init__(self, points : o3d.utility.Vector3dVector, features3d ) :
        self.points = points
        self.features3d = features3d

class Size():
     def __init__(self,sx=None,sy=None,sz=None):
        self.sx = sx #length
        self.sy = sy #width
        self.sz = sz #height

class Cuboid():
    def __init__(self):
        self.Translation=Translation()
        self.Rotation=Rotation()
        self.Size=Size()

#############################################################################################################################################
# METHODS

def string_to_rotation_matrix(matrix_string :str):
    list=matrix_string.split(' ')
    # nplist=np.array(list)
    # npfloatlist=nplist.astype(np.float)
    rotation_matrix=[[float(list[0]),float(list[1]),float(list[2])],
                     [float(list[3]),float(list[4]),float(list[5])],
                     [float(list[6]),float(list[7]),float(list[8])]]
    return rotation_matrix

def string_to_array(string : str):
    list=string.split(' ')
    floatlist=[]
    for x in list:
        floatlist.append(float(x))
    return floatlist

# @staticmethod
def get_if_exist(data, key):
    if key in data:
        return data[key]
    return None