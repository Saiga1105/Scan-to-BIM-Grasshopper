from rdflib import Namespace
from rdflib import Graph
from rdflib import URIRef, BNode, Literal
from rdflib import term
from rdflib.namespace import RDFS, RDF, NamespaceManager
import os
import sys
import codecs
import os
import string
from rdflib.namespace import XSD

g = Graph()

projectmap= r"C:\Users\Sam\OneDrive\SCHOOL\masterproef\data_thesis\TestDataMasterproef\helsinki-cathedral-miniworld"  # hierin bevind zich een map Geometry met een map per layer met obj objecten

#ttl file maken in de projectmap
ttl = str(input("Geef een naam voor het turtel file: "))
ttlfile = ttl + ".ttl"
f = open( os.path.join(projectmap, ttlfile),"wb")   

#namespaces aanmaken
n = Namespace("http://SamDeGeyter.org/HelsinkiCathedral/")
OMG= Namespace("https://w3id.org/omg#")
FOG = Namespace("https://w3id.org/fog#")
BOT = Namespace("https://w3id.org/bot#")
RDF = Namespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#")
RDFS = Namespace("http://www.w3.org/2000/01/rdf-schema#")
BEO = Namespace("http://pi.pauwel.be/voc/buildingelement#")
KULElement = URIRef("http://SamDeGeyter.org/HelsinkiCathedral/Element/")
KULGeometry = URIRef("http://SamDeGeyter.org/HelsinkiCathedral/Geometry/")
KULMaterial = URIRef("http://SamDeGeyter.org/HelsinkiCathedral/Material/")


g.namespace_manager.bind("bot", BOT, override=False)
g.namespace_manager.bind("inst", n, override=False)
g.namespace_manager.bind("omg", OMG, override=False)
g.namespace_manager.bind("fog", FOG, override=False)
g.namespace_manager.bind("beo", BEO, override=False)
g.namespace_manager.bind("kulE", KULElement, override=False)
g.namespace_manager.bind("kulM", KULMaterial, override=False)
g.namespace_manager.bind("kulG", KULGeometry, override=False)

p1 = URIRef(FOG + "asObj_v3.0-obj") #bevat tekens die niet in g.add kunnen staan
p2 = URIRef(FOG +"asObj_v3.0-mtl") #bevat tekens die niet in g.add kunnen staan


geometrymap = projectmap + chr(92) + "Geometry"     #naar map met geometry navigeren
for directory in os.listdir(geometrymap):           #Binnen de map geometry alle mapen(1 per layer) aflopen
    directory1 = geometrymap + chr(92) + directory  #path van elke map
    print(directory[:-1])
    
    #elementen clasificeren met BEO
    if directory != "Clutter":                      #Clutter layer uitsluiten (geen BEO classe)
        elementclass = URIRef(BEO + directory[:-1]) #laatste letter van de layer is een 's' verder is het nodig dat de mappen (en dus layers) een naam hebben die voorkomt in een BEO classe
   


    for filename in os.listdir(directory1):          #elk file in de opgegeven directory doorlopen
        name = filename.split(".")
        material = URIRef(KULMaterial+name[0])
        geometry = URIRef(KULGeometry+name[0])
        element = URIRef(KULElement+name[0])
        g.add((element , RDF.type, BOT.element))
        if directory != "Clutter":
            g.add((element, RDF.type, elementclass))

        if filename.endswith(".obj"):               #functie voor de obj files
            geo= open(os.path.join(directory1, filename),"r").read().splitlines()
            seperator = r"\n"
            geom= seperator.join(geo)
            geom.encode("utf-8",'xmlcharrefreplace')      #RDF litterals mogen elk XML karakter bevatten
            
            g.add((element, OMG.hasGeometry, geometry))
            g.add((geometry, RDF.type, OMG.Geometry))
            g.add((geometry, p1, Literal(geom, datatype = XSD.string)))
            g.add((geometry, RDFS.label, Literal(filename, datatype = XSD.string)))
            g.add((geometry, FOG.hasReferencedContent, material))

        if filename.endswith(".mtl"):
            geo= open(os.path.join(directory1, filename),"r").read().splitlines()
            seperator = r"\n"
            geom= seperator.join(geo)
            geom.encode("utf-8",'xmlcharrefreplace')      #RDF litterals mogen elk XML karakter bevatten
    
            g.add((material, RDF.type , FOG.ReferencedContent))
            g.add((material, p2, Literal(geom, datatype = XSD.string)))
            g.add((material, RDFS.label , Literal(filename, datatype = XSD.string)))

        


f.write(g.serialize(format='turtle'))