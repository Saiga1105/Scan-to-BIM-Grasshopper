"""
PointCloudNode - a Python Class to govern the data and metadata of point cloud data (Open3D, RDF, E57)
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

class PointCloudNode:
    # class attributes
 
    def __init__(self):
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
        #self.Cuboid =  Cuboid()
        self.point_count = None # (int) number of points
        self.accuracy = None # (Float) metric data accuracy e.g. 0.05m
        
        #Coordinate system information
        self.cartesian_transform=None # (offset)a 3D to 3D transform offering a change of origin, scale and rotation. Represented as a matrix and a quaternion.
        self.geospatial_transform=None # (offset) a transform from a 3D Cartesian coordinate system into an ellipsoidal GNSS style coordinate system. E.g. from map-UTM to WGS84 latitude, longitude, altitude.
        self.coordinate_system = None# (string) coordinate system i.e. Lambert72, Lambert2008, geospatial-wgs84, local
        #self.wcs_offset = Pose # (Float) offset Pose(Rotation,Translation) to the coordinate_system

        #paths
        self.session_path = None # (string)
        self.e57_xml_path = None # (string)
        self.e57_path = None # (string)
        self.pcd_path = None # (string)
        self.rdf_graph_path = None # (string)
        self.images2d_path = None # (string)    
        self.features3d_path= None # (string)

        #metadata
        self.rdf_graph = Graph() # (rdflib Graph)
        self.e57_xml_node = ET.ElementTree # (xml.etree.ElementTree)
        self.e57_index = 0 # (int) index of scan in e57 file
        self.images2D = ET.ElementTree() # (list of string xml.etree.ElementTree) 

        #data
        self.o3d_images = [] # (o3d.geometry.RGBDImage) # Open3D list of images
        self.o3d_pointcloud = None # (o3d.geometry.PointCloud) # Open3D point cloud
        self.e57_pointcloud=None # E57 raw data file
        self.features3d= None #o3d.registration.Feature() # http://www.open3d.org/docs/0.9.0/python_api/open3d.registration.Feature.html

    def get_pcd_path(self):
        all_session_file_paths=ld.getListOfFiles(self.session_path) 
        pcd_file_paths=[] 

        for file_path in all_session_file_paths:        
            if file_path.endswith(".pcd"): 
                pcd_file_paths.append(file_path)
        for file_path in pcd_file_paths:
            if self.name in file_path:
                self.pcd_path=file_path
                return True
        return False        

    def set_from_e57_xml_node(self, e57_xml_node : ET.Element): 
        self.e57_xml_node=e57_xml_node
        self.guid=e57_xml_node.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}guid').text
        self.name=e57_xml_node.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}name').text.replace(' ','_')
        posenode=e57_xml_node.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}pose')

        cartesianBoundsnode=e57_xml_node.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}cartesianBounds') 
        if cartesianBoundsnode is not None:
            self.CartesianBounds=CartesianBounds()
            self.CartesianBounds.xMinimum=xml_to_float(cartesianBoundsnode[0].text)
            self.CartesianBounds.xMaximum=xml_to_float(cartesianBoundsnode[1].text)
            self.CartesianBounds.yMinimum=xml_to_float(cartesianBoundsnode[2].text)
            self.CartesianBounds.yMaximum=xml_to_float(cartesianBoundsnode[3].text)
            self.CartesianBounds.zMinimum=xml_to_float(cartesianBoundsnode[4].text)
            self.CartesianBounds.zMaximum=xml_to_float(cartesianBoundsnode[5].text)
        if posenode is not None:
            self.Pose=Pose()    
            rotationnode=posenode.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}rotation')
            if rotationnode is not None:  
                self.Pose.Quaternion.qw=xml_to_float(rotationnode[0].text)
                self.Pose.Quaternion.qx=xml_to_float(rotationnode[1].text)
                self.Pose.Quaternion.qy=xml_to_float(rotationnode[2].text)
                self.Pose.Quaternion.qz=xml_to_float(rotationnode[3].text)
            translationnode=posenode.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}translation')
            if translationnode is not None: 
                self.Pose.Translation.tx=xml_to_float(translationnode[0].text)
                self.Pose.Translation.ty=xml_to_float(translationnode[1].text)
                self.Pose.Translation.tz=xml_to_float(translationnode[2].text)
        elif cartesianBoundsnode is not None:
            self.Pose=Pose() 
            self.set_pose_from_cartesianbounds()
            # else: 
            #     self.Pose.Translation=Translation()
            #     self.Pose.Quaternion=Quaternion()
        pointsnode=e57_xml_node.find('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}points')
        if not pointsnode is None:
            self.point_count=pointsnode.attrib['recordCount']

    def set_pose_from_cartesianbounds(self):
        if self.CartesianBounds is not None:
            self.Pose.Translation.tx=(self.CartesianBounds.xMaximum-self.CartesianBounds.xMinimum)/2
            self.Pose.Translation.ty=(self.CartesianBounds.yMaximum-self.CartesianBounds.yMinimum)/2
            self.Pose.Translation.tz=(self.CartesianBounds.zMaximum-self.CartesianBounds.zMinimum)/2
            self.Pose.Quaternion=Quaternion()
        else:
            return None

    def set_from_o3d_PointCloud(self,pointcloud : o3d.geometry.PointCloud): # this is not finished
        if Type(pointcloud) is o3d.geometry.PointCloud:
            return 
        if Type(pointcloud) is o3d.geometry.PointCloud:
            return

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
        if self.GlobalPose is not None:
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

        # Create an RDF URI node to use as the subject for multiple triples
        subject = URIRef('http://'+ self.name.replace(' ','_')) 

        # Add triples using store's add() method.
        g.add((subject, RDF.type, Literal(v4d3D.PointCloudNode)))
        g.add((subject, RDFS.label, Literal(self.guid)))
        g.add((subject, openlabel.timestamp, Literal(self.timestamp)))
        #g.add((subject, openlabel.frame, Literal(self.session_name)))

        #coordinate system information
        g.add((subject, gom.hasCoordinateSystem, Literal(self.coordinate_system)))
        g.add((subject, openlabel.cartesian_transform, Literal(self.cartesian_transform))) # these are offsets
        g.add((subject, openlabel.geospatial_transform, Literal(self.geospatial_transform)))# these are offsets

        #geometries
        g.add((subject, e57.cartesianBounds, Literal(str(self.get_cuboid_bounds()))))
        g.add((subject, openlabel.cuboid, Literal(str(self.get_cuboid()))))
        g.add((subject, openlabel.quaternion, Literal(str(self.get_rotation_quaternion()))))
        g.add((subject, openlabel.pose, Literal(str(self.get_pose()))))
        g.add((subject, openlabel.global_pose, Literal(str(self.get_global_pose()))))
        g.add((subject, e57.recordCount, Literal(str(self.point_count))))
        g.add((subject, v4d.accuracy, Literal(str(self.accuracy))))

        #paths
        g.add((subject, v4d.session_path, Literal(self.session_path)))
        g.add((subject, v4d.e57_xml_path, Literal(self.e57_xml_path)))
        g.add((subject, v4d.e57_path, Literal(self.e57_path)))
        g.add((subject, v4d.pcd_path, Literal(self.pcd_path)))
        g.add((subject, v4d.images2d_path, Literal(self.images2d_path)))
        g.add((subject, v4d.features3d_path, Literal(self.features3d_path)))

        self.rdf_graph=g

        #BERger2021
        #geo:hasGeometry
        #aiics:dataset_type
        #aiics:created_by
        #askcore_pointclouds: http://askco.re/pointclouds#
        #askcore_types:point_cloud

        #arp:hasCamera
        # arp:hasFocalLength 20.49543 ;# initial focal length of camera in mm
        # 	arp:hasFrameHeigthInPixel 8192 ;
        # 	arp:hasFrameWidthInPixel 4096 ;
        # 	arp:hasPixelHeight_mm 0.0078 ;
        # 	arp:hasPixelWidth_mm 0.007800432180851064 .
        #   arp:BoundingBox
        # arp:hasXMax 6.52180814743042 ;
        # arp:hasXMin 9.2151564 ;
        # arp:hasYMax -23.2442359924316 ;
        # arp:hasYMin -19.454564 ;
        # arp:hasZMax -11.9359664916992 ;
        # arp:hasZMin -5.454654654 .

        # arp:RotationMatrix => m00->m22
        # arp:IPoint3D
        # xsd:dateTime

        #PDAL Point Data Abstraction Library
        # GeoJSON Geography JavaScript Object Notation
        # GeoSPARQL Geographic Query Language for RDF Data
        return True

    def create_o3d_pointcloud(self):
        """Convert e57 point cloud to o3d.geometry.PointCloud"""        
        if self.e57_pointcloud is not None:
            x_ndarray=self.e57_pointcloud.get('cartesianX')
            y_ndarray=self.e57_pointcloud.get('cartesianY')
            z_ndarray=self.e57_pointcloud.get('cartesianZ')

            array= np.vstack(( x_ndarray,y_ndarray,z_ndarray)).T
            points = o3d.utility.Vector3dVector(array)
            self.o3d_pointcloud = o3d.geometry.PointCloud(points)
            return True
        else: 
            print('No e57_pointcloud present in the pointcloudnode')
            return False    

    def write_rdf_graph(self, path : str = None ):
        if path is None:
            self.rdf_graph_path = self.session_path+"\\PCD\\"+self.name+".ttl"
        else :
            self.rdf_graph_path=path      
        # try: # validate filename
        #     validate_filename(str(self.pcd_path))
        # except ValidationError as e:
            # print(f"{e}\n", file=sys.stderr)        
        try:
            self.rdf_graph.serialize(destination=self.rdf_graph_path, format='ttl')
        except:
            print("Export of .pcd failed. Check path")
            return False
        return True

    def write_o3d_pointcloud(self, path : str = None):
        """Write o3d.geometry.PointCloud to pcd_path. If no path is given, the session_path\\PCD\\pcdnode_name is used if available.
             """
        # =str(self.session_path)+'\\PCD\\'+ str(self.name)
        if path is None:
            self.pcd_path = self.session_path+"\\PCD\\"+self.name+".pcd"
        else :
            self.pcd_path=path
        pcd_path=Path(self.pcd_path)
        if pcd_path.exists():
            print('file is already on drive.')
            return False
        # try: # validate filename
        #     validate_filename(str(self.pcd_path))
        # except ValidationError as e:
            # print(f"{e}\n", file=sys.stderr)        
        try:
            o3d.io.write_point_cloud(self.pcd_path, self.o3d_pointcloud)
        except:
            print("Export of .pcd failed. Perhaps point cloud creation failed or Check path is off")
            return False
        return True

    def get_e57_pointcloud(self):
        """Import e57 point cloud given the e57_xml file"""
        try:         
            e57 = pye57.E57(self.e57_path)            
            self.e57_pointcloud = e57.read_scan_raw(self.e57_index)
        except:
            print("File not found or import with PYE57 failed")
            return None

        return self.e57_pointcloud

    def get_rdf_graph (self):
        """Import o3d.geometry.PointCloud given the self.pcd_path (resource intensive)"""
        if self.rdf_graph_path is not None:
            try:                
                self.rdf_graph=Graph().parse(str(self.rdf_graph_path))
            except:
                print('Failed to import RDF graph')
                return False
            return True
        return False

    def get_pcd_file (self):
        """Import o3d.geometry.PointCloud given the self.pcd_path (resource intensive)"""
        if self.pcd_path is not None:
            try:
                self.o3d_pointcloud=o3d.io.read_point_cloud(self.pcd_path)
            except:
                print('Failed to import pcd')
                return False
            return True
        return False

    def compute_features3d(self):
        self.features3d=o3d.registration.compute_fpfh_feature(self.o3d_pointcloud, 0) # 0= KNNSearch, 1=RadiusSearch, 2=HybridSearch
        # find a way to also store points => maybe it's inside features?
        return True

    def get_images2D (self):
        pass

    def write_images2D (self):
        pass

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

def xml_to_float(xml):
    if xml is None:
        return None
    else:
        return float(xml)

def create_pointcloudnode_from_rdf(session_graph : Graph, s : URIRef):
    # create new node
    pcdnode=PointCloudNode    
    # bind additional ontologies that aren't in rdflib
    exif = rdflib.Namespace('http://www.w3.org/2003/12/exif/ns')
    session_graph.bind('exif', exif)
    geo=rdflib.Namespace('http://www.opengis.net/ont/geosparql#') #coordinate system information
    session_graph.bind('geo', geo)
    gom=rdflib.Namespace('https://w3id.org/gom#') # geometry representations => this is from mathias
    session_graph.bind('gom', gom)
    omg=rdflib.Namespace('https://w3id.org/omg#') # geometry relations
    session_graph.bind('omg', omg)
    fog=rdflib.Namespace('https://w3id.org/fog#')
    session_graph.bind('fog', fog)
    v4d=rdflib.Namespace('https://w3id.org/v4d/core#')
    session_graph.bind('v4d3D', v4d)
    v4d3D=rdflib.Namespace('https://w3id.org/v4d/3D#')
    session_graph.bind('v4d3D', v4d3D)
    openlabel=rdflib.Namespace('https://www.asam.net/index.php?eID=dumpFile&t=f&f=3876&token=413e8c85031ae64cc35cf42d0768627514868b2f')
    session_graph.bind('openlabel', openlabel)
    e57=rdflib.Namespace('http://libe57.org/')
    session_graph.bind('e57', e57)

    #instance attributes
    pcdnode.name = str(s).replace('http://','') # (string) PointCloudNode name => these are instance attributes
    pcdnode.guid = literal_to_string(session_graph.value(subject=s,predicate=RDFS.label))  # (string) PointCloudNode guid
    pcdnode.session_name = ld.get_filename(literal_to_string(session_graph.value(subject=s,predicate=v4d.session_path)))   # (string) session subject name
    pcdnode.timestamp = literal_to_string(session_graph.value(subject=s,predicate=openlabel.timestamp))  # (string) e.g. 2020-04-11 12:00:01

    #Geometry                 
    pcdnode.Pose = literal_to_pose(session_graph.value(subject=s,predicate=openlabel.pose))  # (structure) Translation(tx,ty,tz), Quaternion(qw,qx,qy,qz)
    pcdnode.GlobalPose=literal_to_global_pose(session_graph.value(subject=s,predicate=openlabel.global_pose)) # (structure) lat,long,alt, Quaternion(qw,qx,qy,qz)
    pcdnode.CartesianBounds=literal_to_cartesian_bounds(session_graph.value(subject=s,predicate=e57.cartesianBounds)) 
    pcdnode.point_count = literal_to_int(session_graph.value(subject=s,predicate=e57.recordCount)) # (int) number of points
    pcdnode.accuracy = literal_to_float(session_graph.value(subject=s,predicate=v4d.accuracy)) # (Float) metric data accuracy e.g. 0.05m
    
    #Coordinate system information
    pcdnode.cartesian_transform=literal_to_pose(session_graph.value(subject=s,predicate=openlabel.cartesian_transform)) # (offset)a 3D to 3D transform offering a change of origin, scale and rotation. Represented as a matrix and a quaternion.
    pcdnode.geospatial_transform=literal_to_global_pose(session_graph.value(subject=s,predicate=openlabel.geospatial_transform)) # (offset) a transform from a 3D Cartesian coordinate system into an ellipsoidal GNSS style coordinate system. E.g. from map-UTM to WGS84 latitude, longitude, altitude.
    pcdnode.coordinate_system = str(session_graph.value(subject=s,predicate=gom.hasCoordinateSystem))# (string) coordinate system i.e. Lambert72, Lambert2008, geospatial-wgs84, local

    #paths
    pcdnode.session_path = literal_to_string(session_graph.value(subject=s,predicate=v4d.session_path)) # (string)
    pcdnode.e57_xml_path = literal_to_string(session_graph.value(subject=s,predicate=v4d.e57_xml_path)) # (string)
    pcdnode.e57_path = literal_to_string(session_graph.value(subject=s,predicate=v4d.e57_path)) # (string)
    pcdnode.pcd_path = literal_to_string(session_graph.value(subject=s,predicate=v4d.pcd_path)) # (string)
    pcdnode.images2d_path = literal_to_string(session_graph.value(subject=s,predicate=v4d.images2d_path)) # (string)
    pcdnode.features3d_path= literal_to_string(session_graph.value(subject=s,predicate=v4d.features3d_path)) # (string)
    return pcdnode  
