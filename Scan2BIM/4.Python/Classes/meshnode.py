"""
MeshNode - a Python Class to govern the data and metadata of mesh data (Open3D, RDF)
"""
#IMPORT PACKAGES
from ast import Try
from logging import NullHandler
from pathlib import Path
from typing import Type
from urllib.request import ProxyBasicAuthHandler
import xml.etree.ElementTree as ET
from xmlrpc.client import Boolean 
import open3d as o3d 
# from pathlib import Path # https://docs.python.org/3/library/pathlib.html
import numpy as np 
import os
import sys
import math
import rdflib #https://rdflib.readthedocs.io/en/stable/
# from rdflib.serializer import Serializer #pip install rdflib-jsonld https://pypi.org/project/rdflib-jsonld/
from rdflib import Graph
from rdflib import URIRef, BNode, Literal
from rdflib.namespace import CSVW, DC, DCAT, DCTERMS, DOAP, FOAF, ODRL2, ORG, OWL, \
                           PROF, PROV, RDF, RDFS, SDO, SH, SKOS, SOSA, SSN, TIME, \
                           VOID, XMLNS, XSD
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

class MeshNode:
    # class attributes

    #instance attributes
    self.name = None # (string) PointCloudNode name => these are instance attributes
    self.guid = None # (string) PointCloudNode guid
    self.session_name = None  # (string) session subject name
    self.timestamp = None  # (string) e.g. 2020-04-11 12:00:01
    self.sensor = None # (string) P30, BLK, Hololens2, CANON (zie exif), etc.

    #Geometry
    self.Pose = None # (structure) Translation(tx,ty,tz), Quaternion(qw,qx,qy,qz)
    self.GlobalPose=None # (structure) SphericalTranslation(lat,long,alt), Quaternion(qw,qx,qy,qz)
    self.CartesianBounds=None
    self.point_count = None # (int) number of vertices
    self.face_count = None # (int) number of faces
    self.accuracy = None # (Float) metric data accuracy e.g. 0.05m
    
    #Coordinate system information
    self.cartesian_transform=None # (offset)a 3D to 3D transform offering a change of origin, scale and rotation. Represented as a matrix and a quaternion.
    self.geospatial_transform=None # (offset) a transform from a 3D Cartesian coordinate system into an ellipsoidal GNSS style coordinate system. E.g. from map-UTM to WGS84 latitude, longitude, altitude.
    self.coordinate_system = None# (string) coordinate system i.e. Lambert72, Lambert2008, geospatial-wgs84, local

    #paths
    self.session_path = None # (string)
    self.mesh_path = None # (string)
    self.texture_paths = []
    self.materials_path = []
    self.rdf_graph_path = None # (string)
    self.features3d_path= None # (string)
    self.features2d_path= None # (string)

    #metadata
    self.rdf_graph = Graph() # (rdflib Graph)

    #data
    self.o3d_mesh = None # (o3d.geometry.Mesh) # Open3D point cloud
    self.features3d= None #o3d.registration.Feature() # http://www.open3d.org/docs/0.9.0/python_api/open3d.registration.Feature.html


    def get_mesh_path(self):
        all_session_file_paths=ld.getListOfFiles(self.session_path) 
        file_paths=[] 

        for file_path in all_session_file_paths:        
            if file_path.endswith(".obj") or file_path.endswith(".ply"): 
                file_paths.append(file_path)
        for file_path in file_paths:
            if self.name in file_path:
                self.mesh_path=file_path
                return True
        return False        
    
    def set_pose_from_cartesianbounds(self):
        if self.CartesianBounds is not None:
            self.Pose.Translation.tx=(self.CartesianBounds.xMaximum-self.CartesianBounds.xMinimum)/2
            self.Pose.Translation.ty=(self.CartesianBounds.yMaximum-self.CartesianBounds.yMinimum)/2
            self.Pose.Translation.tz=(self.CartesianBounds.zMaximum-self.CartesianBounds.zMinimum)/2
            self.Pose.Quaternion=Quaternion()
        else:
            return None
    
    def get_cuboid_bounds(self):
        if self.CartesianBounds is not None:
            return [self.CartesianBounds.xMinimum,self.CartesianBounds.yMinimum,self.CartesianBounds.zMinimum,
                    self.CartesianBounds.xMaximum,self.CartesianBounds.yMaximum,self.CartesianBounds.zMaximum]
        else:
            return None
    
    def get_cuboid(self):
        if self.Pose is not None:
            euler=s2b.euler_from_quaternion(self.Pose.Quaternion.qw,self.Pose.Quaternion.qx,self.Pose.Quaternion.qy,self.Pose.Quaternion.qz)           
            return [self.Pose.Translation.tx,self.Pose.Translation.ty,self.Pose.Translation.tz,
                    euler[0],euler[1],euler[2],
                    self.CartesianBounds.xMaximum-self.CartesianBounds.xMinimum,self.CartesianBounds.yMaximum-self.CartesianBounds.yMinimum,self.CartesianBounds.zMaximum-self.CartesianBounds.zMinimum]
        else:
            return None

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
    
    def set_global_pose_from_local_coordinate_system(self):
        if self.Pose is not None and self.coordinate_system is not None:           
            if 'Lambert72' in self.coordinate_system: #Lambert72, Lambert2008, geospatial-wgs84, local
                #1.use the conversion from Belgian to spherical coordinates
                latBel,lngBel=ld.lambert72_to_spherical_coordinates(self.Pose.Translation.tx,self.Pose.Translation.ty)                
                #2.use the Molodensky equation to transform Belgian spherical coordinates to WGS84
                LatWGS84,LngWGS84=ld.belgian_datum_to_wgs84(latBel,lngBel)
                self.GlobalPose.SphericalTranslation.lat= LatWGS84
                self.GlobalPose.SphericalTranslation.long = LngWGS84
                return True
            elif 'Lambert2008' in self.coordinate_system: #Lambert72, Lambert2008, geospatial-wgs84, local
                pass
            else:
                print('no valid coordinate system found (Lambert72, Lambert2008)')
        else:
            print('no pose information is available')
            return False

    def set_local_pose_from_wgs84(self):
        if self.GlobalPose is not None :           
            #1.use the Molodensky equation to transform WGS84 to Belgian spherical coordinates
            latBel,lngBel=ld.wgs84_to_belgian_datum(self.GlobalPose.SphericalTranslation.lat,self.GlobalPose.SphericalTranslation.long) 
            #2.use the conversion from Belgian spherical coordinates to Belgian cartesian coordinates
            x,y=ld.spherical_coordinates_to_lambert72
            self.Pose.Translation.tx=x 
            self.Pose.Translation.ty=y
            return True
        else:
            print('no global pose information is available')
            return False 

    def visualize(self):
        vis = o3d.visualization.Visualizer()
        vis.create_window()
        vis.add_geometry(self.o3d_mesh)
        vis.run()
        vis.destroy_window()

    def set_pose_from_o3d_mesh(self):
        center=self.o3d_mesh.get_center()
        self.Pose=Pose
        self.Pose.Translation=Translation(center[0],center[1],center[2])

    def set_cartesian_bounds_from_o3d_mesh(self):
        max=self.o3d_mesh.get_max_bound()
        min=self.o3d_mesh.get_min_bound()
        self.CartesianBounds=CartesianBounds(min[0],max[0],min[1],max[1],min[2],max[2])

    def set_metadata_from_obj(self):
        self.session_name = ld.get_filename(self.session_path)  # (string) session subject name

        #Geometry
        if self.o3d_mesh is not None:
            self.Pose = self.set_pose_from_o3d_mesh() # (structure) Translation(tx,ty,tz), Quaternion(qw,qx,qy,qz)
            self.CartesianBounds=self.set_cartesian_bounds_from_o3d_mesh()
            self.point_count = None # (int) number of vertices

        else:
            print('No mesh geometry found to extract the metadata. Please set meshnode.o3d_mesh first.')

        has_textures  
        has_triangle_normals  
        has_triangle_uvs
        has_triangle_material_ids
        has_vertex_colors
    self.GlobalPose=None # (structure) SphericalTranslation(lat,long,alt), Quaternion(qw,qx,qy,qz)
    self.point_count = None # (int) number of vertices
    self.face_count = None # (int) number of faces
    self.accuracy = None # (Float) metric data accuracy e.g. 0.05m
    
    #Coordinate system information
    self.cartesian_transform=None # (offset)a 3D to 3D transform offering a change of origin, scale and rotation. Represented as a matrix and a quaternion.
    self.geospatial_transform=None # (offset) a transform from a 3D Cartesian coordinate system into an ellipsoidal GNSS style coordinate system. E.g. from map-UTM to WGS84 latitude, longitude, altitude.
    self.coordinate_system = None# (string) coordinate system i.e. Lambert72, Lambert2008, geospatial-wgs84, local

    #paths
    self.session_path = None # (string)
    self.mesh_path = None # (string)
    self.texture_paths = []
    self.materials_path = []
    self.rdf_graph_path = None # (string)
    self.features3d_path= None # (string)
    self.features2d_path= None # (string)

    #metadata
    self.rdf_graph = Graph() # (rdflib Graph)

    #data
    self.o3d_mesh = None # (o3d.geometry.Mesh) # Open3D point cloud
    self.features3d= None #o3d.registration.Feature() # http://www.open3d.org/docs/0.9.0/python_api/open3d.registration.Feature.html

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

def literal_to_pose(literal: Literal):
    temp=str(literal)
    if 'None' not in temp:
        temp=temp.strip('[]')        
        res = list(map(float, temp.split(', ')))
        pose=Pose
        pose.Translation=Translation(tx=res[0],ty=res[1],tz=res[2])
        pose.Quaternion=Quaternion(qw=res[3],qx=res[4],qy=res[5],qz=res[6])
        return pose
    else:
        return None  

def literal_to_global_pose(literal: Literal):
    temp=str(literal)
    if 'None' not in temp:
        temp=temp.strip('[]')        
        res = list(map(float, temp.split(', ')))
        global_pose=GlobalPose
        global_pose.SphericalTranslation=SphericalTranslation(lat=res[0],long=res[1],alt=res[2])
        global_pose.Quaternion=Quaternion(qw=res[3],qx=res[4],qy=res[5],qz=res[6])
        return global_pose 
    else:
        return None  

def literal_to_cartesian_bounds(literal: Literal):
    temp=str(literal)
    if 'None' not in temp:
        temp=str(literal)
        temp=temp.strip('[]')        
        res = list(map(float, temp.split(', ')))
        cartesian_bounds=CartesianBounds(x_min=res[0],x_max=res[1],y_min=res[2],y_max=res[3],z_min=res[4],z_max=res[5])    
        return cartesian_bounds
    else:
        return None  

def literal_to_float(literal: Literal):
    string=str(literal)
    if 'None' in string:
        return None
    else:
        return float(string)

def literal_to_string(literal: Literal):
    string=str(literal)
    if 'None' in string:
        return None
    else:
        return string

def literal_to_int(literal: Literal):
    string=str(literal)
    if 'None' in string:
        return None
    else:
        return int(string)