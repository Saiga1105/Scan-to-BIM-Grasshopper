"""
linkeddatatools - a Python library for RDF graph structuring and exchange.
"""
#IMPORT PACKAGES
import numpy as np 
import cv2 
import open3d as o3d 
import json  
import os 
import matplotlib.pyplot as plt #conda install -c conda-forge matplotlib
#import torch #conda install -c pytorch pytorch
import pye57 #conda install xerces-c  =>  pip install pye57
import xml.etree.ElementTree as ET 
# from pathlib import Path
import math

import ifcopenshell.util
import ifcopenshell.geom as geom
from ifcopenshell.util.selector import Selector
from ifcopenshell.ifcopenshell_wrapper import file

# import APIs
import rdflib
from rdflib import Graph, plugin
from rdflib.serializer import Serializer #pip install rdflib-jsonld https://pypi.org/project/rdflib-jsonld/
from rdflib import Graph
from rdflib import URIRef, BNode, Literal
from rdflib.namespace import CSVW, DC, DCAT, DCTERMS, DOAP, FOAF, ODRL2, ORG, OWL, \
                           PROF, PROV, RDF, RDFS, SDO, SH, SKOS, SOSA, SSN, TIME, \
                           VOID, XMLNS, XSD


#IMPORT MODULES
from Classes.pointcloudnode import PointCloudNode
import Classes.pointcloudnode as pc

def read_e57_xml(e57_xml_path):
    """Parse xml files that are created with E57lib e57xmldump.exe

    Args:
        arg1: string of file path e.g. "D:\\Data\\2018-06 Werfopvolging Academiestraat Gent\\week 22\\PCD\\week 22 lidar_CC.xml"
            
    Returns:
        A pointcloud node class with the xml metadata 
    """
    #code
    try:
        #E57 XML file structure
        #e57Root
        #   >data3D
        #       >vectorChild
        #           >pose
        #               >rotation
        #               >translation
        #           >cartesianBounds
        #           >guid
        #           >name
        #           >points recordCount
        #   >images2D

        mytree = ET.parse(e57_xml_path)
        root = mytree.getroot()  
        pcdnodelist=[]   
        e57_path=e57_xml_path.replace('xml','e57')       

        for idx,scannode in enumerate(root.iter('{http://www.astm.org/COMMIT/E57/2010-e57-v1.0}vectorChild')):
            pcdnode=PointCloudNode() 
            pcdnode.set_from_e57_xml_node(scannode)
            pcdnode.e57_xml_path=e57_xml_path
            pcdnode.e57_index=idx
            pcdnode.e57_path=e57_path
            pcdnodelist.append(pcdnode)       

    except:
        print('graph_path was not recognized. Please run .\e57xmldump on target e57 files and store output xml files somewhere in session folder. If formatting error occurs, manually remove <?xml version="1.0" encoding="UTF-8"?> from xml file.')
        return None
    return pcdnodelist

def get_filename(file_path):
    """ Deconstruct path into filename"""
    import ntpath
    path=ntpath.basename(file_path)
    head, tail = ntpath.split(path)
    return tail

def get_timestamp(file_path):
    return os.path.getctime(file_path)

def lambert72_to_spherical_coordinates(x,y): 
    """"
       Belgian Lambert 1972---> Spherical coordinates
       Input parameters : X, Y = Belgian coordinates in meters
       Output : latitude and longitude in Belgium Datum!
       source: http://zoologie.umons.ac.be/tc/algorithms.aspx
    """
    LongRef = 0.076042943  #      '=4°21'24"983
    nLamb  = 0.7716421928 #
    aCarre = 6378388 ^ 2 #
    bLamb = 6378388 * (1 - (1 / 297)) #
    eCarre = (aCarre - bLamb ^ 2) / aCarre #
    KLamb = 11565915.812935 #
    
    eLamb = math.Sqrt(eCarre)
    eSur2 = eLamb / 2
    
    Tan1  = (x - 150000.01256) / (5400088.4378 - y)
    Lambda = LongRef + (1 / nLamb) * (0.000142043 + math.Atan(Tan1))
    RLamb = math.Sqrt((x - 150000.01256) ^ 2 + (5400088.4378 - y) ^ 2)
    
    TanZDemi = (RLamb / KLamb) ^ (1 / nLamb)
    Lati1 = 2 * math.Atan(TanZDemi)
    
    eSin=0.0
    Mult1=0.0
    Mult2=0.0
    Mult=0.0
    LatiN=0.0
    Diff=1     

    while math.Abs(Diff) > 0.0000000277777:
        eSin = eLamb * math.Sin(Lati1)
        Mult1 = 1 - eSin
        Mult2 = 1 + eSin
        Mult = (Mult1 / Mult2) ^ (eLamb / 2)
        LatiN = (math.PI / 2) - (2 * (math.Atan(TanZDemi * Mult)))
        Diff = LatiN - Lati1
        Lati1 = LatiN
    
    latBel = (LatiN * 180) / math.PI
    lngBel = (Lambda * 180) / math.PI
    return latBel,lngBel

def spherical_coordinates_to_lambert72(latBel,lngBel):
    """
    Conversion from spherical coordinates to Lambert 72
    Input parameters : lat, lng (spherical coordinates)
    Spherical coordinates are in decimal degrees converted to Belgium datum!
    source: http://zoologie.umons.ac.be/tc/algorithms.aspx
    """
 
    LongRef  = 0.076042943  #      '=4°21'24"983
    bLamb  = 6378388 * (1 - (1 / 297))
    aCarre  = 6378388 ^ 2
    eCarre = (aCarre - bLamb ^ 2) / aCarre
    KLamb = 11565915.812935
    nLamb = 0.7716421928
    
    eLamb  = math.Sqrt(eCarre)
    eSur2  = eLamb / 2
    
    #conversion to radians
    lat = (math.PI / 180) * latBel
    lng = (math.PI / 180) * lngBel
    
    eSinLatitude = eLamb * math.Sin(lat)
    TanZDemi = (math.Tan((math.PI / 4) - (lat / 2))) *  (((1 + (eSinLatitude)) / (1 - (eSinLatitude))) ^ (eSur2))
    
    RLamb= KLamb * ((TanZDemi) ^ nLamb)
    
    Teta  = nLamb * (lng - LongRef)
        
    x = 150000 + 0.01256 + RLamb * math.Sin(Teta - 0.000142043)
    y = 5400000 + 88.4378 - RLamb * math.Cos(Teta - 0.000142043)
    return x,y

def belgian_datum_to_wgs84(latBel,lngBel):
    """
    Input parameters : Lat, Lng : latitude / longitude in decimal degrees and in Belgian 1972 datum
    Output parameters : LatWGS84, LngWGS84 : latitude / longitude in decimal degrees and in WGS84 datum
    source: http://zoologie.umons.ac.be/tc/algorithms.aspx
    """
    
    Haut = 0.0   #   'Altitude
    # Dim LatWGS84, LngWGS84 As Double
    # Dim DLat, DLng As Double
    # Dim Dh As Double
    # Dim dy, dx, dz As Double
    # Dim da, df As Double
    # Dim LWa, Rm, Rn, LWb As Double
    # Dim LWf, LWe2 As Double
    # Dim SinLat, SinLng As Double
    # Dim CoSinLat As Double
    # Dim CoSinLng As Double
    
    # Dim Adb As Double
    
    #conversion to radians
    Lat = (math.PI / 180) * latBel
    Lng = (math.PI / 180) * lngBel
    
    SinLat = math.Sin(Lat)
    SinLng = math.Sin(Lng)
    CoSinLat = math.Cos(Lat)
    CoSinLng = math.Cos(Lng)
    
    dx = -125.8
    dy = 79.9
    dz = -100.5
    da = -251.0
    df = -0.000014192702
    
    LWf = 1 / 297
    LWa = 6378388
    LWb = (1 - LWf) * LWa
    LWe2 = (2 * LWf) - (LWf * LWf)
    Adb = 1 / (1 - LWf)
    
    Rn = LWa / math.Sqrt(1 - LWe2 * SinLat * SinLat)
    Rm = LWa * (1 - LWe2) / (1 - LWe2 * Lat * Lat) ^ 1.5
    
    DLat = -dx * SinLat * CoSinLng - dy * SinLat * SinLng + dz * CoSinLat
    DLat = DLat + da * (Rn * LWe2 * SinLat * CoSinLat) / LWa
    DLat = DLat + df * (Rm * Adb + Rn / Adb) * SinLat * CoSinLat
    DLat = DLat / (Rm + Haut)
    
    DLng = (-dx * SinLng + dy * CoSinLng) / ((Rn + Haut) * CoSinLat)
    Dh = dx * CoSinLat * CoSinLng + dy * CoSinLat * SinLng + dz * SinLat
    Dh = Dh - da * LWa / Rn + df * Rn * Lat * Lat / Adb
    
    LatWGS84 = ((Lat + DLat) * 180) / math.PI
    LngWGS84 = ((Lng + DLng) * 180) / math.PI
    return LatWGS84,LngWGS84

def wgs84_to_belgian_datum(LatWGS84,LngWGS84):
    """
    Input parameters : Lat, Lng : latitude / longitude in decimal degrees and in WGS84 datum
    Output parameters : LatBel, LngBel : latitude / longitude in decimal degrees and in Belgian datum
    source: http://zoologie.umons.ac.be/tc/algorithms.aspx
    """
    Haut = 0    #  'Altitude
    # Dim LatBel, LngBel As Double
    # Dim DLat, DLng As Double
    # Dim Dh As Double
    # Dim dy, dx, dz As Double
    # Dim da, df As Double
    # Dim LWa, Rm, Rn, LWb As Double
    # Dim LWf, LWe2 As Double
    # Dim SinLat, SinLng As Double
    # Dim CoSinLat As Double
    # Dim CoSinLng As Double
    
    # Dim Adb As Double
    
    #conversion to radians
    Lat = (math.PI / 180) * LatWGS84
    Lng = (math.PI / 180) * LngWGS84
    
    SinLat = math.Sin(Lat)
    SinLng = math.Sin(Lng)
    CoSinLat = math.Cos(Lat)
    CoSinLng = math.Cos(Lng)
    
    dx = 125.8
    dy = -79.9
    dz = 100.5
    da = 251.0
    df = 0.000014192702
    
    LWf = 1 / 297
    LWa = 6378388
    LWb = (1 - LWf) * LWa
    LWe2 = (2 * LWf) - (LWf * LWf)
    Adb = 1 / (1 - LWf)
    
    Rn = LWa / math.Sqrt(1 - LWe2 * SinLat * SinLat)
    Rm = LWa * (1 - LWe2) / (1 - LWe2 * Lat * Lat) ^ 1.5
    
    DLat = -dx * SinLat * CoSinLng - dy * SinLat * SinLng + dz * CoSinLat
    DLat = DLat + da * (Rn * LWe2 * SinLat * CoSinLat) / LWa
    DLat = DLat + df * (Rm * Adb + Rn / Adb) * SinLat * CoSinLat
    DLat = DLat / (Rm + Haut)
    
    DLng = (-dx * SinLng + dy * CoSinLng) / ((Rn + Haut) * CoSinLat)
    Dh = dx * CoSinLat * CoSinLng + dy * CoSinLat * SinLng + dz * SinLat
    Dh = Dh - da * LWa / Rn + df * Rn * Lat * Lat / Adb
    
    LatBel = ((Lat + DLat) * 180) / math.PI
    LngBel = ((Lng + DLng) * 180) / math.PI
    return  LatBel,LngBel

def getListOfFiles(directory_path):
    """Get a list of all files in the directory and subdirectories

    Args:
        directory_path: directory path e.g. "D:\\Data\\2018-06 Werfopvolging Academiestraat Gent\\week 22\\"
            
    Returns:
        A pointcloud node class with the xml metadata 
    """
    # names in the given directory 
    listOfFile = os.listdir(directory_path)
    allFiles = list()
    # Iterate over all the entries
    for entry in listOfFile:
        # Create full path
        fullPath = os.path.join(directory_path, entry)
        # If entry is a directory then get the list of files in this directory 
        if os.path.isdir(fullPath):
            allFiles = allFiles + getListOfFiles(fullPath)
        else:
            allFiles.append(fullPath)                
    return allFiles