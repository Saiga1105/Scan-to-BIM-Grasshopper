import sys
import glob
import PIL
import os
import PIL.Image as PILimage
from PIL import ImageDraw, ImageFont, ImageEnhance
from PIL.ExifTags import TAGS, GPSTAGS
from pathlib import Path

import rdflib
from rdflib import Graph
from rdflib import URIRef, BNode, Literal
from rdflib.namespace import CSVW, DC, DCAT, DCTERMS, DOAP, FOAF, ODRL2, ORG, OWL, \
                           PROF, PROV, RDF, RDFS, SDO, SH, SKOS, SOSA, SSN, TIME, \
                           VOID, XMLNS, XSD

import ReadExifDatatest1
############################################         

class Worker(object):
    def __init__(self, img):
        self.img = img
        self.exif_data = self.get_exif_data()
        self.lat = self.get_lat() # this is to change lat info to dd
        self.lon = self.get_lon() # this is to change lat info to dd
        self.alt = self.get_alt() # no real need for this
        self.date = self.get_date_time() # no real need for this
        # self.graph = self.add_to_RDF_graph()
        super(Worker, self).__init__()

    @staticmethod
    def get_if_exist(data, key):
        if key in data:
            return data[key]
        return None

    # @staticmethod
    # def get_if_exist(self,key):
    #     if key in self.exif_data:
    #         return self.exif_data[key]
    #     return None

    # @staticmethod
    # def convert_to_degress(value):
    #     """Helper function to convert the GPS coordinates
    #     stored in the EXIF to degress in float format"""
    #     d0 = value[0][0]
    #     d1 = value[0][1]
    #     d = float(d0) / float(d1)
    #     m0 = value[1][0]
    #     m1 = value[1][1]
    #     m = float(m0) / float(m1)
    #     s0 = value[2][0]
    #     s1 = value[2][1]
    #     s = float(s0) / float(s1)
    #     return d + (m / 60.0) + (s / 3600.0)
    
    @staticmethod
    def dms2dd(value):
        """Convert the GPS coordinates
        stored in the EXIF from (degrees,minutes,seconds) to decimal degrees"""
        degrees=value[0]
        minutes=value[1]
        seconds=value[2]        
        dd = float(degrees) + float(minutes)/60 + float(seconds)/(60*60)
        if (degrees <0 or minutes<0 or seconds <0): # direction == 'S':
            dd *= -1
        return dd

    @staticmethod
    def dd2dms(dd): ## convert (decimal degrees) to (degrees, minutes, seconds)
        negative = dd < 0
        dd = abs(dd)
        minutes,seconds = divmod(dd*3600,60)
        degrees,minutes = divmod(minutes,60)
        if negative:
            if degrees > 0:
                degrees = -degrees
            elif minutes > 0:
                minutes = -minutes
            else:
                seconds = -seconds
        return (degrees,minutes,seconds)

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
        return exif_data

    def get_lat(self):
        """Returns the latitude, if available, from the 
        provided exif_data (obtained through get_exif_data above)"""
        # print(exif_data)
        if 'GPSInfo' in self.exif_data:
            gps_info = self.exif_data["GPSInfo"]
            gps_latitude = self.get_if_exist(gps_info, "GPSLatitude")
            gps_latitude_ref = self.get_if_exist(gps_info, 'GPSLatitudeRef')
            if gps_latitude and gps_latitude_ref:
                #lat = self.convert_to_degress(gps_latitude)
                lat = self.dms2dd(gps_latitude)
                if gps_latitude_ref != "N":
                    lat = 0 - lat
                lat = str(f"{lat:.{5}f}")
                return lat
        else:
            return None

    def get_lon(self):
        """Returns the longitude, if available, from the 
        provided exif_data (obtained through get_exif_data above)"""
        # print(exif_data)
        if 'GPSInfo' in self.exif_data:
            gps_info = self.exif_data["GPSInfo"]
            gps_longitude = self.get_if_exist(gps_info, 'GPSLongitude')
            gps_longitude_ref = self.get_if_exist(gps_info, 'GPSLongitudeRef')
            if gps_longitude and gps_longitude_ref:
                #lon = self.convert_to_degress(gps_longitude)
                lon = self.dms2dd(gps_longitude)
                if gps_longitude_ref != "E":
                    lon = 0 - lon
                lon = str(f"{lon:.{5}f}")
                return lon
        else:
            return None

    def get_alt(self):
        """Returns the altitude, if available, from the 
        provided exif_data (obtained through get_exif_data above)"""
        #print(exif_data)
        if 'GPSInfo' in self.exif_data:            
            gps_info = self.exif_data["GPSInfo"]
            gps_altitude = self.get_if_exist(gps_info, 'GPSAltitude')
            gps_altitude_ref = self.get_if_exist(gps_info, 'GPSAltitudeRef')
            if gps_altitude :
                altitude = float(gps_altitude)
                altitude = str(f"{altitude:.{5}f}")
                return altitude 
        else:
            return None

    def get_date_time(self):
        if 'DateTime' in self.exif_data:
            date_and_time = self.exif_data['DateTime']
            return date_and_time 

    # def write2json(self):
    #     """Write the obtained exif data to a json file"""
    #     #print(exif_data)
    #     jsonSubList1 = {}
    #     json_object ={
    #         exif:IFD rdf:ID="Primary_Image"
    #         "name" : "sathiyajith",
    #         "rollno" : 56,
    #         "cgpa" : 8.6,
    #         "phonenumber" : "9976770500",
    #         "GPS": [
    #             {"Latitude": self.lat, "mpg": 27.5}, #this can just be a tuple
    #             {"Longitude": "Ford Edge", "mpg": 24.1}
    #         ]
    #     }
        
    def add_to_RDF_graph(self,g):
        """Write the obtained exif data to a json file"""
        
        #bind the various ontologies that you want to use in your schema
        g.bind("rdf", RDF) 
        g.bind("rdfs", RDFS)
        g.bind("foaf", FOAF) 
        g.bind("owl", OWL) 

        # bind additional ontologies that aren't in rdflib
        exif = rdflib.Namespace('http://www.w3.org/2003/12/exif/ns')
        g.bind('exif', exif)

        # add entry per imagename
        temp = imagename.split('\\')
        imageName=temp[-1]
        imageRDF = URIRef(imageName) 
        g.add((imageRDF,exif.imageUniqueID, Literal(BNode()))) # a GUID is generated
        #test= self.get_if_exist(self.exif_data, "DateTime")


        g.add((imageRDF,exif.dateTime, Literal(self.get_if_exist(self.exif_data, "DateTime"))))
        
        #imageWidth=self.get_if_exist(self.exif_data,"ExifImageWidth")
        g.add((imageRDF,exif.imageWidth, Literal(self.get_if_exist(self.exif_data,"ExifImageWidth"))))
        g.add((imageRDF,exif.imageLength, Literal(self.get_if_exist(self.exif_data,"ExifImageHeight"))))
        g.add((imageRDF,exif.xResolution, Literal(self.get_if_exist(self.exif_data,"XResolution"))))
        g.add((imageRDF,exif.yResolution, Literal(self.get_if_exist(self.exif_data,"YResolution"))))
        g.add((imageRDF,exif.resolutionUnit, Literal(self.get_if_exist(self.exif_data,"ResolutionUnit"))))
        # 'exif:imageWidth'
        # 'exif:imageLength'
        # 'exif:orientation'
        # 'exif:xResolution'
        # 'exif:yResolution'        
        # 'exif:fNumber'
        # 'exif:exifVersion'
        # 'exif:apertureValue'
        # 'exif:focalLength'
        # 'exif:imageUniqueID'
        if 'GPSInfo' in self.exif_data:
            gps_info = self.exif_data["GPSInfo"]
            g.add((imageRDF,exif.gpsLatitude, Literal(self.get_if_exist(gps_info, "GPSLatitude"))))
            g.add((imageRDF,exif.gpsLatitudeRef, Literal(self.get_if_exist(gps_info, "GPSLatitudeRef"))))
            g.add((imageRDF,exif.gpsLongitude, Literal(self.get_if_exist(gps_info, "GPSLongitude"))))
            g.add((imageRDF,exif.gpsLongitudeRef, Literal(self.get_if_exist(gps_info, "GPSLongitudeRef"))))
            g.add((imageRDF,exif.gpsAltitude, Literal(self.get_if_exist(gps_info, "GPSAltitude"))))
            g.add((imageRDF,exif.gpsAltitudeRef, Literal(self.get_if_exist(gps_info, "GPSAltitudeRef"))))
        # 'exif:gpsVersionID'
        # 'exif:gpsLatitudeRef'
        # 'exif:gpsLatitude'
        # 'exif:gpsTimeStamp'
        # 'exif:gpsDOP'
        # 'exif:gpsMapDatum'
        # 'exif:gpsDifferential'
        # 'exif:model'
        # 'exif:resolutionUnit'
        # 'exif:exif_IFD_Pointer' #everything about camera
        # 'exif:IFD'
        return g



########################################################################
if __name__ == '__main__':
    try:
        #enter a folder as 1st argument
        folder_path=Path(sys.argv[1])
        # folder_path=Path('K:\\Projects\\2021-03 Project FWO SB Heinder\\7.Data\\2021-09 Testcase code\\Raw Data\\100_0001\\photos\\')

        #enter an output name as second argument (default is imageGraph)
        # output_path=sys.argv[2]

        #example
        # python ReadExifDatatest1 K:\Projects\2021-03 Project FWO SB Heinder\7.Data\2021-09 Testcase code\Raw Data\100_0001\photos ImageGraph

        # 
        #retrieve all jpg files from the folder
        file_to_open = folder_path / '*.jpg'
        jpgFilenamesList = glob.glob(str(file_to_open)) #this works
        destinationfile = os.path.join(folder_path, "imageGraph.ttl")

        #initialize graph
        g = Graph()

        for imagename in jpgFilenamesList:
            # img = PILimage.open(sys.argv[1])
            # img = PILimage.open('100_0001_0001.JPG')
            img = PILimage.open(imagename)

            #Read & process exifdata to an RDF graph
            image = Worker(img)
            image.add_to_RDF_graph(g)

            #report succes
            # print(image.date, image.lat, image.lon, image.alt)

            #print graph
            #myGraph=image.graph
            #print(myGraph.serialize())

            #this printing is optional
            print (g.serialize())

        #Export graph
        #Format support can be extended with plugins, but “xml”, “n3”, “turtle”, “nt”, “pretty-xml”, “trix”, “trig” and “nquads” are built in.
        g.serialize(destination=destinationfile, format='ttl')

    except Exception as e:
        print(e)