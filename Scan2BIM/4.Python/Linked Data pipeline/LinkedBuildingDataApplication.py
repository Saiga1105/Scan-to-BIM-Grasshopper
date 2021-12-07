#Import of the needed packages
from tkinter import *
from tkinter.ttk import *
from tkinter import filedialog
from tkinter import messagebox
from rdflib import *
import socket
from multiprocessing import *
from functools import partial
from urllib.parse import *
from datetime import datetime
from threading import *
import codecs
import sys
import os
import requests 

Version = 7
#get user information
host = socket.gethostname()
ip_address = socket.gethostbyname(host)

internalport= 8630
externalport = 2412
settingsDirectory = r"\ThesisSamDeGeyter-2020\LBD-GeoLinkApplication\Settings"
#List containing al supported geometry file formats
SupportedGeometryFormats = []

#Important global variables
ProjectName =""
ProjectDirectory = ""
ProjectCreator = ""
ProjectGeometryDirectory =""
Projectfile = ""
Language = "en"
ShowDescriptions = bool()

LinkedDataManager = ""
SaveToDefaultGraph = bool()
SaveToDefaultGraph = False
PreferedFormat = ""
DeleteGeometryFiles = bool()
ExportLevel = ""
Frames = []
DefaultCS = ""

#projectsettings
ZonesSettings = []
ElementsSettings = []
DamagesSettings = []
GeometriesSettings = []

##Variables for the communication with GraphDB
GraphDBportnumber = 7200 #Default value
GraphDBrepository ="ThesisSamDeGeyterTest" #To speed up testing
UrlGet =""
UrlPost =""
headersGet = {"Accept":"application/json"}
headersPost = {"content-type": 'application/sparql-update'}

#Variables to connect with Rhino
Rhinoport = 2412 #Default
RhinoStatus = bool()
RhinoStatus = True
STATUS = True
processes =[]
threads=[]
root = Tk()
update = IntVar()
#Functions concerning saving an reading from/to files
#function to save to file
def SAVEtoFile():
    tp = open(Projectfile, "w")
    tp.writelines("PN*"+ ProjectName + "*\n")
    tp.writelines("PD*" + ProjectDirectory + "*\n")
    tp.writelines("GD*" + ProjectGeometryDirectory + "*\n")
    tp.writelines("C*" + ProjectCreator + "*\n")
    tp.writelines("ZS*" + ZonesSettings[0] + "*" + ZonesSettings[1] + "*\n")
    tp.writelines("ES*" + ElementsSettings[0] + "*" + ElementsSettings[1] + "*\n")
    tp.writelines("GS*" + GeometriesSettings[0] + "*" + GeometriesSettings[1] + "*"+ GeometriesSettings[2] + "*" + GeometriesSettings[3] + "*\n")
    tp.writelines("DS*" + DamagesSettings[0] + "*" + DamagesSettings[1] + "*\n")
    if LinkedDataManager == 'GraphDB':
        tp.writelines("LINKEDDATAMANAGER*GraphDB*"+ str(GraphDBportnumber)+"*" + GraphDBrepository + "*\n")
    tp.writelines("SHOWDESCRIPTIONS*"+str(ShowDescriptions)+"*\n")
    tp.writelines("SAVETODEFAULT*"+str(SaveToDefaultGraph)+"*\n")
    tp.writelines("DELETEGEOMETRYFILES"+"*"+str(DeleteGeometryFiles)+"*\n")
    tp.writelines("PREFEREDFORMAT*" + PreferedFormat[0] + "*\n")
    tp.writelines("ExportLevel*" + ExportLevel + "*\n")
    tp.writelines("END")

def LOADfromFile():
    f = open(Projectfile, "r")
    for line in f:
            var = line.split("*")
            if var[0] == "PN":
                global ProjectName
                ProjectName = var[1]
            if var[0] == "PD":
                global ProjectDirectory
                ProjectDirectory = var[1]
            if var[0] == "ZS":
                global ZonesSettings
                ZonesSettings = [var[1],var[2]]
            if var[0] == "ES":
                global ElementsSettings
                ElementsSettings = [var[1],var[2]]
            if var[0] == "GS":
                global GeometriesSettings
                GeometriesSettings = [var[1],var[2],var[3], var[4]]
            if var[0] == "DS":
                global DamagesSettings
                DamagesSettings = [var[1],var[2]]
            
            if var[0] == "LINKEDDATAMANAGER":
                global LinkedDataManager
                LinkedDataManager = var[1]
                if LinkedDataManager == 'GraphDB':
                    global GraphDBportnumber
                    GraphDBportnumber = var[2]
                    global GraphDBrepository
                    GraphDBrepository = var[3]
            if var[0] == 'END':
                global UrlGet
                UrlGet = "http://%s:%s/repositories/%s" % (ip_address, GraphDBportnumber,GraphDBrepository)
                global UrlPost
                UrlPost = "http://%s:%s/repositories/%s/statements" % (ip_address, GraphDBportnumber,GraphDBrepository)
            if var[0] == 'SHOWDESCRIPTIONS':
                global ShowDescriptions
                print(ShowDescriptions)
                ShowDescriptions = var[1]
            if var[0] == 'SAVETODEFAULT':
                global SaveToDefaultGraph
                SaveToDefaultGraph = var[1]
            if var[0] == 'DELETEGEOMETRYFILES':
                global DeleteGeometryFiles
                DeleteGeometryFiles = var[1]
            if var[0] == 'PREFEREDFORMAT':
                for geometryformat in SupportedGeometryFormats:
                    if geometryformat[0] == var[1]:
                        global PreferedFormat
                        PreferedFormat = geometryformat
            if var[0] == "ExportLevel":
                global ExportLevel
                ExportLevel = var[1]
            if var[0] == "C":
                global ProjectCreator
                ProjectCreator = var[1]
            if var[0] == "GD":
                global ProjectGeometryDirectory
                ProjectGeometryDirectory = var[1]

#Functions to read the content of a certain file
def READfile(Location):
    print("Reading the file")
    for geometryformat in SupportedGeometryFormats:
        if Location.endswith(geometryformat[2]):
        
            if geometryformat[7] == '0':
                filecontent = open(Location,'r').read().splitlines()
                seperator = r"\n"
                content = seperator.join(filecontent)
                content.encode(geometryformat[5], 'xmlcharrefreplace')
                return content
            if geometryformat[7] == '1':
                try:
                    filecontent = open(Location,'r').read().splitlines()
                    seperator = r"\n"
                    content = seperator.join(filecontent)
                    content.encode('utf-8', 'xmlcharrefreplace')
                except:
                    filecontent = open(Location, 'rb').read()
                    content = codecs.encode(filecontent,geometryformat[5])
                    content = content.decode()
                return content

def DetermineFormat(Location,SupportedGeometryFormats):
    for geometryformat in SupportedGeometryFormats:
        if Location.endswith(geometryformat[2]):
            if geometryformat[0] == 'PLY':
                try:
                    filecontent = open(Location,'r').read()
                    FOGproperty = "fog:asPly_v1.0-ascii"
                except:
                    filecontent = open(Location, 'rb').read()
                    if "binary_little_endian".encode() in filecontent:
                        FOGproperty = "fog:asPly_v1.0-binaryLE"
                    if "binary_big_endian".encode() in filecontent:
                        FOGproperty = "fog:asPly_v1.0-binaryBE"
                return FOGproperty
            else:
                FOGproperty = geometryformat[1]
                return FOGproperty

############################################################################################
# ONTOLOGIES###
############################################################################################
#Function that will search all ontologies that are present in the ontologies folder of the Application
def ListNeededOntologies():
    Ontologies = []
    dir = os.path.dirname(__file__)
    ontologiespath = os.path.join(dir, 'Ontologies')
    for file in os.listdir(ontologiespath):
        FileLocation = os.path.join(ontologiespath, file)
        if file.endswith(".ttl"):
            g = Graph()
            g.parse(FileLocation,format ='turtle')
            OntologieNameQueryResult = g.query(
                """SELECT DISTINCT ?label
                        WHERE {
                            ?a rdf:type owl:Ontology .
                            ?a <http://purl.org/dc/terms/title> ?label.
                            FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))
                        }""" % Language,
                        initNs=dict(
                            rdf=Namespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
                            owl=Namespace("http://www.w3.org/2002/07/owl#")))
            for row in OntologieNameQueryResult:
                OntologyName = row
            Ontologies.append(OntologyName)
    return Ontologies
#Function that will search all the ontologies that are loaded in the graph
def SearchOntologies(): 
        ListOntologies = []
        queryOntologies= """
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?s ?label ?g ?version where { GRAPH ?g{
                ?s rdf:type owl:Ontology .
                ?s <http://purl.org/dc/terms/title> ?label.
                ?s owl:versionInfo ?version.
                FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))
        }}
        """ % (Language)
        PARAMS = {'query':queryOntologies, 'infer' : "false"} 
        r = requests.get(url = UrlGet, headers = headersGet, params= PARAMS)
        print("Search ontologies: %s " %r.status_code)
        data = r.json()
        index =0
        try:
            length = len(data['results']['bindings'])
            while index < length:
                try:
                    OntologyName = data['results']['bindings'][index]['s']['value']
                    try: 
                        Ontologylabel = data['results']['bindings'][index]['label']['value']
                    except:
                        Ontologylabel = OntologyName
                    try:
                        NamedOntologyGraph = data['results']['bindings'][index]['g']['value']
                        NamedOntologyGraph = "<" + NamedOntologyGraph + ">"
                    except:
                        NamedOntologyGraph = ""
                    try:
                        OntologyVersion = data['results']['bindings'][index]['version']['value']
                    except:
                        OntologyVersion = ""
                except:
                    print("something went wrong with this ontology")
                
                index = index + 1      
                ListOntologies.append((Ontologylabel, OntologyName , NamedOntologyGraph, OntologyVersion))
        except:
            print("No Ontologies found")
        return ListOntologies
#function that will delete an ontology
def DeleteOntology(NamedGraphExisting):
    ClearQuery = """
        CLEAR GRAPH %s
    """% NamedGraphExisting
    r = requests.post(url = UrlPost, headers = headersPost, data = ClearQuery)
    print("remove ontology: %s "%r.status_code)
#function to loaded an ontology into a graph
def LoadOntology(Ontologyfile):
    def INSERTOntology():
        f = open(Ontologyfile,"r", encoding = 'utf-8')
        Listprefixes = []
        ListBases = []
        Content =""
        for line in f:
            if line.startswith("@prefix"):
                Listprefixes.append(line)
            else:
                if line.startswith("@base"):
                    ListBases.append(line)
                else:
                    Content = Content + line
        InsertOntologyQuery =""" """
        for prefix in Listprefixes:
            prefix = prefix.replace('@prefix', 'PREFIX')
            prefix = prefix.replace('> .', '>')
            prefix = prefix.replace('>.','>')
            InsertOntologyQuery += prefix
        for base in ListBases:
            base = base.replace('@base', 'BASE')
            base = base.replace('> .', '>')
            base = base.replace('>.','>')
            InsertOntologyQuery += base
        #Building the insert data query
        InsertOntologyQuery += """
            INSERT DATA{
                GRAPH %s{
                            %s
                }
            }
            """ % (NamedGraph, Content)
        r = requests.post(url = UrlPost, headers = headersPost, data = InsertOntologyQuery.encode('utf-8'))
        print("Add ontology: %s , %s"%(OntologyName,r.status_code))

    if Ontologyfile.endswith(".ttl"):
        g = Graph()
        g.parse(Ontologyfile,format ='turtle')
        OntologieNameQueryResult = g.query(
                """SELECT DISTINCT ?ontology ?prefix ?version ?label
                        WHERE {
                            ?ontology rdf:type owl:Ontology .
                            ?ontology dcterms:title ?label.
                            ?ontology <http://purl.org/vocab/vann/preferredNamespacePrefix> ?prefix.
                            ?ontology owl:versionInfo ?version.
                        }""",
                        initNs=dict(
                            rdf=Namespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
                            owl=Namespace("http://www.w3.org/2002/07/owl#"),
                            dcterms=Namespace("http://purl.org/dc/terms/")))

        for row in OntologieNameQueryResult:
            OntologyName = row[0]
            NamedGraph = "<http://LBDA_Ontology.edu/%s>" % (row[1])
            OntologyVersion = row[2]
            OntologyLabel = row[3]
        #Look if ontology is already present in the given repository
        ListOntologies = SearchOntologies()
        
        #Does the ontology already exists?
        OntologyExists = False
        NamedGraphExisting = ""
        FoundOntology =""
        FoundOntologyVersion = ""
        for Ontology in ListOntologies:
            if str(OntologyName) == Ontology[1]:
                NamedGraphExisting = Ontology[2]
                FoundOntology = Ontology[0]
                FoundOntologyVersion = Ontology[3]
                OntologyExists = True
        if OntologyExists:
            if messagebox.askyesno("Loading Ontology", """
            Loading Ontology: %s 
            Version: %s
            The given repository already contains 
            Ontology: %s 
            Version: %s
            Do you want to replace?"""% (OntologyLabel,OntologyVersion,FoundOntology, FoundOntologyVersion)):
                #clear the named graph with the Ontology in
                DeleteOntology(NamedGraphExisting)
                INSERTOntology()
        else: 
            INSERTOntology()
#function that will load all the requiered ontologies into the graph                            
def LoadNeededOntologies():
    for file in os.listdir("Ontologies"):
        FileLocation = "Ontologies\\"+file
        LoadOntology(FileLocation)

def ListBOTZoneClasses():
    for Ontology in SearchOntologies():
        ListBOTzoneClasses = []
        BOTURI = "https://w3id.org/bot#"
        if Ontology[1] == BOTURI:
            QueryZoneClasses = """
                PREFIX owl: <http://www.w3.org/2002/07/owl#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                PREFIX bot: <https://w3id.org/bot#>

                SELECT ?class ?label ?comment
                    WHERE{
                        ?class rdf:type owl:Class.
                        ?class rdfs:subClassOf bot:Zone.
                        ?class rdfs:label ?label.
                        ?class rdfs:comment ?comment.
                            FILTER(LANG(?label)=""|| LANGMATCHES(LANG(?label), "%s"))
                            FILTER(LANG(?comment) ="" || LANGMATCHES(LANG(?comment), "%s"))
                    }
                """ % (Language, Language)
            PARAMS = {'query':QueryZoneClasses, 'infer' : "false"}
            r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
            print("bot:Zone classes: %s " %r.status_code)
            data = r.json()
            index =0
            length = len(data['results']['bindings'])
            while index < length:
                subject = data['results']['bindings'][index]['class']['value']
                try: 
                    label = data['results']['bindings'][index]['label']['value']
                except:
                    label = subject
                try:
                    comment = data['results']['bindings'][index]['comment']['value']
                except: 
                    comment = ""
                index +=1
                ListBOTzoneClasses.append((label, subject,comment))
            return ListBOTzoneClasses

def SearchAllClassesWithoutSubclasses(graph,Classes):
    for Ontology in SearchOntologies():
        ListClasses = []
        if Ontology[2] == graph:
            QueryClasses = """
                PREFIX owl: <http://www.w3.org/2002/07/owl#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                
                SELECT ?class ?label ?comment
                    WHERE{
                        GRAPH %s{
                            ?class rdf:type owl:Class.
                            ?class rdfs:label ?label.
                            ?class rdfs:comment ?comment.
                                MINUS{?subclass rdfs:subClassOf ?class.}
                                """ % (graph)
            for Class in Classes:
                QueryClasses = QueryClasses + """
                    MINUS{?class owl:disjointWith <%s>.}"""% Class
            QueryClasses = QueryClasses + """
                    FILTER(LANG(?label)=""|| LANGMATCHES(LANG(?label), "%s"))
                    FILTER(LANG(?comment) ="" || LANGMATCHES(LANG(?comment), "%s"))}}"""%(Language, Language)
            PARAMS = {'query':QueryClasses, 'infer' : "true"}
            r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
            print("Searsch ontologies %s" %r.status_code)
            data = r.json()
            index =0
            length = len(data['results']['bindings'])
            while index < length:
                subject = data['results']['bindings'][index]['class']['value']
                try: 
                    label = data['results']['bindings'][index]['label']['value']
                except:
                    label = subject
                try:
                    comment = data['results']['bindings'][index]['comment']['value']
                except: 
                    comment = ""
                index +=1
                ListClasses.append((label, subject,comment))
            return ListClasses

############################################################################################
# Browse functions###
############################################################################################
def BrowseDirectory(Entry):
    Directory = filedialog.askdirectory(initialdir= ProjectDirectory)
    Entry.delete(0,END)
    Entry.insert(0,Directory)

def BrowseProjectfile(Entry):
    my_filetypes = [('TEXT', '.txt')]
    Entry.delete(0,END)
    projectfile = filedialog.askopenfilename(title="Please select a projectfile:",filetypes=my_filetypes)
    Entry.insert(0, projectfile)

def BrowseOntology(Entry):
    my_filetypes = [('TURTLE', '.ttl')]
    Entry.delete(0,END)
    Ontologyfile = filedialog.askopenfilenames(title="Please select a file containing a ontology:",filetypes=my_filetypes)
    Entry.insert(0,Ontologyfile)

def BrowseGeometryfile(Entry,Message,ReferencedContentVariable,Description,xsdtype):
    my_filetypes = []
    for Geometryformat in SupportedGeometryFormats:
        if int(Geometryformat[3]) <= 1:
            my_filetypes.append((Geometryformat[0],Geometryformat[2]))
    Entry.delete(0,END)
    Geometryfile = filedialog.askopenfilenames(title="Please select a geometry file:", filetypes = my_filetypes)
    Entry.insert(0,Geometryfile[0])
    for Geometryformat in SupportedGeometryFormats:
        if Geometryfile[0].endswith(Geometryformat[2]):
            Message.set(DetermineFormat(Geometryfile[0],SupportedGeometryFormats))
            xsdtype.set(Geometryformat[6])
            if int(Geometryformat[3]) == 1:
                ReferencedContentVariable.set(1)
    Description.set(READfile(Geometryfile[0]))

def BrowseReferencedContent(Entry,Message,Description,xsdtype):
    my_filetypes = []
    for Geometryformat in SupportedGeometryFormats:
        if int(Geometryformat[3]) == 2:
            my_filetypes.append((Geometryformat[0],Geometryformat[2]))
    Entry.delete(0,END)
    Referencefile = filedialog.askopenfilenames(title = "Please select the referenced content:", filetypes = my_filetypes)
    Entry.insert(0,Referencefile[0])
    for Geometryformat in SupportedGeometryFormats:
        if Referencefile[0].endswith(Geometryformat[2]):
            Message.set(DetermineFormat(Referencefile[0],SupportedGeometryFormats))
            xsdtype.set(Geometryformat[6])
    Description.set(READfile(Referencefile[0]))
############################################################################################
# Functions for support###
############################################################################################
def CloseFrame():
    for f in Frames:
        f.destroy()

def CheckIfObjectExists(object):
    CheckObjectquery = """
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        select ?s
            where{
                    %s ?p ?o.
                    } 
            """ % (object)
    PARAMS = {'query':CheckObjectquery, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Object exists: %s" %r.status_code)
    data = r.json()
    lengte = len(data['results']['bindings'])
    if lengte > 0:         
        return FALSE
    else:
        return TRUE 

def InternalMessage(text):
    InternalSender = socket.socket()
    InternalSender.connect((host, internalport))
    InternalSender.send(text.encode())
    InternalSender.close()

def ExternalMessage(text):
    ExternalSender = socket.socket()
    ExternalSender.connect((host, externalport))
    ExternalSender.send(text.encode())
    ExternalSender.close()

def on_closing():
    if messagebox.askokcancel("Quit", "Do you want to quit?"):
        root.destroy()
        global STATUS
        STATUS = NONE
        for p in processes:
            p.terminate()
            p.join()
        InternalMessage('END')
        ExternalMessage('END')
        for t in threads:
            t.join()

        sys.exit()

############################################################################################
# Search in graph###
############################################################################################
def SearchCoordinates():
    ListCoordinates = []
    query = """
        PREFIX gom: <https://w3id.org/gom#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> 
            select ?s ?label
                where{
                        ?s rdf:type gom:CoordinateSystem.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Coordinate systems: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListCoordinates.append((subject,label))
    except:
        print('No coordinate systems found')
    return ListCoordinates

def SearchGeometryContexts():
    ListGeometryContexts = []
    query = """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            select ?s ?label
                where{
                        ?s rdf:type omg:GeometryContext.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometry contexts: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListGeometryContexts.append((subject,label))
    except:
        print('No zones found')
    return ListGeometryContexts

def SearchGeometryTypes():
    ListGeometryTypes = []
    query = """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> 
            select ?s ?label
                where{
                        ?s rdfs:subClassOf omg:Geometry.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometry types: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListGeometryTypes.append((subject,label))
    except:
        print('No zones found')
    return ListGeometryTypes

def SearchReferenceProperties():
    ListReferenceProperties = []
    query = """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> 
            select ?s
                where{
                        ?s rdfs:subPropertyOf omg:hasReferencedGeometryId.
                        MINUS{ ?x rdfs:subPropertyOf ?s.}
                    }
    """
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Reference properties: %s "%r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            if not subject == "":
                ListReferenceProperties.append(subject)
            index = index +1
    except:
        print('No ReferenceProperties found')
    return ListReferenceProperties

def SearchGeometryApplications():
    ListGeometryApplications = []
    query = """
        PREFIX gom: <https://w3id.org/gom#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> 
            select ?s ?label
                where{
                        ?s a gom:GeometryModellingApplication.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometry applications: %s "% r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListGeometryApplications.append((subject,label))
    except:
        print('No zones found')
    return ListGeometryApplications

def SearchZones():
    ListZones = []
    query = """
        PREFIX bot: <https://w3id.org/bot#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?s ?label
                where{
                        ?s rdf:type bot:Zone.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Zones: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = "subject"
            index = index + 1
            if not subject == "":
                ListZones.append((subject, label))
    except:
        print('No zones found')
    return ListZones

def SearchElements():
    ListElements = []
    query = """
        PREFIX bot: <https://w3id.org/bot#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?s ?label
                where{
                        ?s rdf:type bot:Element.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Elements %s" %r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListElements.append((subject, label))
    except:
        print('No Elements found')
    return ListElements

def SearchGeometryNodes():
    ListGeometries = []
    queryGeometries= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?geometry ?label
                where{
                        ?geometry rdf:type omg:Geometry.
                            OPTIONAL{ ?geometry rdfs:label ?label}
                                FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))
                    }""" % (Language)
    PARAMS = {'query':queryGeometries, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometry nodes: %s"%r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            subject = data['results']['bindings'][index]['geometry']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except: 
                label = ""
            index = index + 1
            ListGeometries.append((subject, label))
    except:
        print("No Geometries found")
    return ListGeometries

def SearchGeometryStateNodes(geometry):
    ListGeometryStates = []
    QueryGeometryStates= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?geometrystate ?label
                where{
                    <%s> omg:hasGeometryState ?geometrystate.                        
                    ?geometrystate rdfs:label ?label.
                            OPTIONAL{ ?geometry rdfs:label ?label}
                                FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))
                    }""" % (geometry, Language)
    PARAMS = {'query':QueryGeometryStates, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometry states: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            subject = data['results']['bindings'][index]['geometrystate']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except: 
                label = ""
            index = index + 1
            ListGeometryStates.append((subject, label))
    except:
        print("No Geometries found")
    return ListGeometryStates

def SearchLinkedObject(geometry):
    queryGeometries= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        
            select ?s ?label
                where{
                        ?s omg:hasGeometry <%s>.
                        OPTIONAL{
                            ?s rdfs:label ?label.
                            FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (geometry, Language)
    PARAMS = {'query':queryGeometries, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Linked objects: %s " %r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                Linkedobject = (subject, label)
    except:
        print("No Links found")
    return Linkedobject

def SearchDamages():
    ListDamages = []
    query = """
        PREFIX dot: <https://w3id.org/dot#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
            select ?s ?label
                where{
                        ?s rdf:type dot:Damage.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                    }
    """ % (Language)
    PARAMS = {'query':query, 'infer' : "TRUE"}
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Damages: %s "%r.status_code)
    data = r.json()
    index =0
    try:
        lengte = len(data['results']['bindings'])
        while index < lengte:
            subject = data['results']['bindings'][index]['s']['value']
            try:
                label = data['results']['bindings'][index]['label']['value']
            except:
                label = ""
            index = index + 1
            if not subject == "":
                ListDamages.append((subject,label))
    except:
        print('No zones found')
    return ListDamages

#Because of the complexety of the geometries with their possible multiple geometrystates and referenced contents, they are directly added to the treeview
def SearchGeometries(GeometriesTreeview):
    
    #Query for all Geometry descriptions saved as OMG1.
    queryOMG1= """
        
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX fog: <https://w3id.org/fog#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?s ?label
                where{
                        ?s omg:hasSimpleGeometryDescription ?o.
                        OPTIONAL{
                        ?s rdfs:label ?label.
                        FILTER(LANG(?label) = "" || LANGMATCHES(LANG(?label), "%s"))}
                        MINUS{ ?s a omg:Geometry.}
                        MINUS{?s a omg:GeometryState.}
                        MINUS{?s a fog:ReferencedContent.}
                    }""" % (Language)
    PARAMS = {'query':queryOMG1, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometries OMG 1:  %s " %r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            try:
                name = data['results']['bindings'][index]['s']['value']
                try:
                    label = data['results']['bindings'][index]['label']['value']
                except:
                    print("No label")
            except:
                name = ""
            if not name == "":
                GeometriesTreeview.insert("",END,name, text = name, values = (label, "OMG-L1"))
                
            index = index + 1
    except:
        print("Something went wrong adding the geometries OMG 1 to the treeview")
    #Query for all Geometry descriptions saved as OMG2.
    queryOMG2= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX fog: <https://w3id.org/fog#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?geometry ?geometrylabel ?referencedcontent ?referencedcontentlabel
                where{
                        ?object omg:hasGeometry ?geometry.
                        OPTIONAL{
                        ?geometry rdfs:label ?geometrylabel.
                            FILTER(LANG(?geometrylabel) = "" || LANGMATCHES(LANG(?geometrylabel), "%s"))}
                        OPTIONAL{
                            ?geometry fog:hasReferencedContent ?referencedcontent.
                            ?referencedcontent rdfs:label ?referencedcontentlabel.
                            FILTER(LANG(?referencedcontentlabel) = "" || LANGMATCHES(LANG(?referencedcontentlabel), "%s"))}
                        
                        MINUS{?geometry omg:hasGeometryState ?GeometryState.}
                    }""" % (Language,Language)
    PARAMS = {'query':queryOMG2, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometries OMG 3: %s"%r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            name = data['results']['bindings'][index]['geometry']['value']
            try:
                GeometryLabel = data['results']['bindings'][index]['geometrylabel']['value']
            except:
                GeometryLabel = ""
            if not name == "":
                GeometriesTreeview.insert("",END, name, text = name, values = (GeometryLabel, "OMG-L2"))

                ##referenced content and add to the Treeview
                try:
                    ReferencedContent = data['results']['bindings'][index]['referencedcontent']['value']
                    try:
                        ReferencedContentLabel = data['results']['bindings'][index]['referencedcontentlabel']['value']
                    except:
                        ReferencedContentLabel = ""
                    if not ReferencedContent == "":
                        GeometriesTreeview.insert(name, END,ReferencedContent, text =ReferencedContent, values = (ReferencedContentLabel, "OMG-L2"))
                except:
                    print('No referenced content')
            index = index + 1
    except:
        print("Something went wrong adding the geometries OMG 2 to the treeview")
    
    #Query for all Geometry descriptions saved as OMG3
    queryOMG3= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX fog: <https://w3id.org/fog#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?geometry ?geometrylabel ?geometrystate ?geometrystatelabel ?referencedcontent ?referencedcontentlabel
                where{
                        ?objecten omg:hasGeometry ?geometry.
                        OPTIONAL{
                        ?geometry rdfs:label ?geometrylabel.
                        FILTER(LANG(?geometrylabel) = "" || LANGMATCHES(LANG(?geometrylabel),"%s"))
                            }
                        ?geometry omg:hasGeometryState ?geometrystate.
                        OPTIONAL{
                        ?geometrystate rdfs:label ?geometrystatelabel.
                        FILTER(LANG(?geometrystatelabel) = "" || LANGMATCHES(LANG(?geometrystatelabel),"%s"))
                            }

                        OPTIONAL{
    	                    ?geometrystate fog:hasReferencedContent ?referencedcontent.
                            ?referencedcontent rdfs:label ?referencedcontentlabel.
                        FILTER(LANG(?referencedcontentlabel) = "" || LANGMATCHES(LANG(?referencedcontentlabel),"%s"))
                            }
                        
                        
                    }""" % (Language,Language, Language)
    PARAMS = {'query':queryOMG3, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometries OMG 3: %s" %r.status_code)
    data = r.json()
    index =0
    try:
        GeometriesList = []
        GeometryStatesList =[]
        lengte = len(data['results']['bindings'])
        while index < lengte:
            geometry = data['results']['bindings'][index]['geometry']['value']
            geometryalreadyknown = bool()
            geometryalreadyknown = False
            if not len(GeometriesList) == 0:
                for g in GeometriesList:
                    if g == geometry:
                        geometryalreadyknown = True            

            if not geometryalreadyknown:
                GeometriesList.append(geometry)
            try:
                GeometryLabel = data['results']['bindings'][index]['geometrylabel']['value']
            except:
                GeometryLabel = ""
            if not geometry == "":
                if not geometryalreadyknown:
                    GeometriesTreeview.insert("",END, geometry, text = geometry, values = (GeometryLabel,"OMG-L3"))
                #Geometry states toevoegen
                geometrystate = data['results']['bindings'][index]['geometrystate']['value']
                geometrystatealreadyknown = bool()
                geometrystatealreadyknown = False
                if not len(GeometryStatesList) ==0:
                    for gs in GeometryStatesList:
                        if gs == geometrystate:
                            geometrystatealreadyknown = True
                            
                if not geometrystatealreadyknown:
                    GeometryStatesList.append(geometrystate)
                try:
                    GeometryStateLabel = data['results']['bindings'][index]['geometrystatelabel']['value']
                except:
                    GeometryStateLabel = ""
                if not geometrystate =="":
                    if not geometrystatealreadyknown:
                        GeometriesTreeview.insert(geometry,END,geometrystate, text = geometrystate, values = (GeometryStateLabel,"OMG-L3"))
                    ##referenced content and add to the Treeview
                    try:
                        ReferencedContent = data['results']['bindings'][index]['referencedcontent']['value']
                        try:
                            ReferencedContentLabel = data['results']['bindings'][index]['referencedcontentlabel']['value']
                        except:
                            ReferencedContentLabel = ""
                        if not ReferencedContent == "":
                            GeometriesTreeview.insert(geometrystate, END,ReferencedContent,text =ReferencedContent, values = (ReferencedContentLabel, "OMG-L3"))
                    except: 
                        print("No referenced content")
            index = index + 1
    except:
        print("Something went wrong adding the geometries OMG 3 to the treeview")
        
def SearchGeometries2(GeometriesTreeview):

    queryOMG2= """
        PREFIX omg: <https://w3id.org/omg#>
        PREFIX fog: <https://w3id.org/fog#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            select ?geometry ?geometrylabel
                where{
                        ?object omg:hasGeometry ?geometry.
                        ?geometry rdf:type omg:Geometry.
                        OPTIONAL{
                        ?geometry rdfs:label ?geometrylabel.
                            FILTER(LANG(?geometrylabel) = "" || LANGMATCHES(LANG(?geometrylabel), "%s"))}
                    }""" % (Language)
    PARAMS = {'query':queryOMG2, 'infer' : "TRUE"} 
    r = requests.get(url = UrlGet, headers = headersGet, params = PARAMS)
    print("Geometries 2: %s "%r.status_code)
    data = r.json()
    index =0
    try:
        length = len(data['results']['bindings'])
        while index < length:
            name = data['results']['bindings'][index]['geometry']['value']
            try:
                GeometryLabel = data['results']['bindings'][index]['geometrylabel']['value']
            except:
                GeometryLabel = ""
            if not name == "":
                GeometriesTreeview.insert("",END, name, text = name, values = (GeometryLabel, "OMG-L2"))
                
            index = index + 1
    except:
        print("Something went wrong adding the geometries to the treeview")
        
############################################################################################
# Save to graph###
############################################################################################
def CoordinateSystemToGraph(SaveToDefaultGraph, UrlPost, headersPost, NamedGraph, coordinatesystem, coordinatesystemURI, creator, description, unit, Language):
    query = """
            PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            PREFIX prov: <http://www.w3.org/ns/prov#>
            PREFIX dcterms: <http://purl.org/dc/terms/>
            PREFIX gom: <https://w3id.org/gom#>
                
            INSERT DATA{"""
    if not SaveToDefaultGraph:
        query = query+ """GRAPH <%s> {"""%NamedGraph
    
    query = query + """
        <%s> rdf:type gom:CartesianCoordinateSystem;
        rdfs:label "%s"@%s;
        prov:generatedAtTime "%s"^^xsd:date.
    """%(coordinatesystemURI, coordinatesystem, Language, datetime.now().strftime('%Y-%m-%d'))
    if not creator == 'Unknown':
        query = query +"""
            <%s> dcterms:creator "%s".
        """%(coordinatesystemURI, creator)
    if description:
        query = query + """
            <%s> rdfs:comment \"""%s\"""@%s.
        """%(coordinatesystemURI, description, Language)
    if unit:
        query = query +"""
            <%s> gom:hasLengthUnit <%s>.
        """%(coordinatesystemURI, unit)

    
    if not SaveToDefaultGraph:
        query = query + """} """

    query = query + """} """ 
    r = requests.post(url = UrlPost, headers = headersPost, data = query)
    print("Coordinate system to graph: %s " %r.status_code)
    InternalMessage('New')

def GeometryContextToGraph(SaveToDefaultGraph, UrlPost, headerpost, NamedGraph, geometrycontext, geometrycontextURI, creator, description, geometries, Language):
    query = """
            PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            PREFIX fog: <https://w3id.org/fog#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            PREFIX prov: <http://www.w3.org/ns/prov#>
            PREFIX omg: <https://w3id.org/omg#>
            PREFIX dcterms: <http://purl.org/dc/terms/>
                            
            INSERT DATA{"""
    if not SaveToDefaultGraph:
        query = query+ """GRAPH <%s> {"""%NamedGraph
    
    query = query + """
        <%s> rdf:type omg:GeometryContext;
        rdfs:label "%s"@%s;
        prov:generatedAtTime "%s"^^xsd:date.
    """%(geometrycontextURI, geometrycontext, Language, datetime.now().strftime('%Y-%m-%d'))
    if not creator == 'Unknown':
        query = query +"""
            <%s> dcterms:creator "%s".
        """%(geometrycontextURI, creator)
    if description:
        query = query + """
            <%s> rdfs:comment \"""%s\"""@%s.
        """%(geometrycontextURI, description, Language)
    if geometries:
        for geometry in geometries:
            query = query + """
                <%s> omg:hasGeometryContext <%s>.
            """%(geometry[0], geometrycontextURI)
    
    if not SaveToDefaultGraph:
        query = query + """} """

    query = query + """} """ 
    r = requests.post(url = UrlPost, headers = headersPost, data = query)
    print("Geometry context to graph: %s" %r.status_code)
    InternalMessage('New')

def OMG1ToGraph(SaveToDefaultGraph, UrlPost, headerPost,NamedGraph,geometryFormat,subject, geometryLiteral,xsdtype):
    query = """
            PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            PREFIX fog: <https://w3id.org/fog#>
                            
            INSERT DATA{"""
    if not SaveToDefaultGraph:
        query = query+ """GRAPH <%s> {"""%NamedGraph
    if geometryFormat == 'fog:asPly_v1.0-ascii':
        xsdtype = "string"
    if not subject ==None:
        query = query + """
            <%s> %s \"""%s\"""^^xsd:%s.
        """%(subject, geometryFormat, geometryLiteral,xsdtype)
    
    if not SaveToDefaultGraph:
        query = query + """} """

    query = query + """} """ 
    r = requests.post(url = UrlPost, headers = headersPost, data = query)
    print("OMG 1 to graph: %s " %r.status_code)
    InternalMessage('New')   

def OMG3ToGraph(SaveToDefaultGraph, UrlPost, headerPost,graph,subject, geometry,geometryURI, geometrystate, geometrystateURI, geometryformat,geometryLiteral, geometryliteraltype, PartOf,referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, creator, geometrytype, createdin, coordinatesystem, geometrycontext,DerivedFromGeometry,Language):
    if geometryformat == 'fog:asPly_v1.0-ascii':
        geometryliteraltype = "string"
    query = """
            PREFIX omg: <https://w3id.org/omg#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            PREFIX fog: <https://w3id.org/fog#>
            PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            PREFIX dcterms: <http://purl.org/dc/terms/>
            PREFIX prov: <http://www.w3.org/ns/prov#>
            PREFIX gom: <https://w3id.org/gom#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            INSERT DATA{"""
    if not SaveToDefaultGraph:
        query = query+ """GRAPH <%s> {""" %graph
    
    if subject:
        query = query + """
            <%s> omg:hasGeometry <%s>."""%(subject, geometryURI)
    if geometrytype :
        query = query +"""<%s> rdf:type <%s>;
                rdfs:label "%s"@%s.
        """%(geometryURI,geometrytype, geometry, Language)
    else:
        query = query + """<%s> rdf:type omg:Geometry;
                rdfs:label "%s"@%s.
        """%(geometryURI,geometry, Language)
    if PartOf:
        query = query + """
            <%s> omg:hasGeometryState <%s>.
            <%s> rdf:type omg:GeometryState;
                %s \"""%s\"""^^xsd:%s;
                omg:isPartOfGeometry <%s>;
                rdfs:label "%s"@%s.
        """ % (geometryURI,geometrystateURI, geometrystateURI, geometryformat, geometryLiteral,geometryliteraltype, PartOf,geometrystate,Language)
    else:    
        query = query + """
            <%s> omg:hasGeometryState <%s>.
            <%s> rdf:type omg:GeometryState;
                %s \"""%s\"""^^xsd:%s;
                rdfs:label "%s"@%s.
        """ % (geometryURI,geometrystateURI, geometrystateURI, geometryformat, geometryLiteral,geometryliteraltype, geometrystate,Language)    
    

    if referencedcontent:
        query = query + """
        <%s>    fog:hasReferencedContent    <%s>.
        <%s>    rdf:type                    fog:ReferencedContent;
                %s                          \"""%s\"""^^xsd:%s;
                rdfs:label                  "%s"@%s.

        """%(geometrystateURI, referencedcontentURI, referencedcontentURI,referencedcontentformat,referencedcontentLiteral, referencedcontentliteraltype, referencedcontent,Language)
    if creator:
        query = query +"""
        <%s> dcterms:creator "%s".
        """%(geometrystateURI, creator)
    if createdin:
        query = query +"""
        <%s> gom:createdIn <%s>.
        """%(geometrystateURI, createdin)
    if coordinatesystem:
        query = query +"""
        <%s> gom:hasCoordinateSystem <%s>.
        """%(geometrystateURI, coordinatesystem)
    
    if geometrycontext:
        for gc in geometrycontext:
            query = query +"""
            <%s> omg:hasGeometryContext <%s>.
            """%(geometrystateURI, gc)
    
    if DerivedFromGeometry:
        for derivedgeometry in DerivedFromGeometry:
            query = query +"""
            <%s> omg:isDerivedFromGeometry <%s>.
            """%(geometrystateURI, derivedgeometry)

    query = query +"""
        <%s> prov:generatedAtTime "%s"^^xsd:dateTime.
        """%(geometrystateURI, datetime.now().strftime('%Y-%m-%dT%H:%M:%S'))


    if not SaveToDefaultGraph:
        query = query + """} """

    query = query + """} """ 
    r = requests.post(url = UrlPost, headers = headersPost, data = query)
    print("OMG 3 to graph: %s" %r.status_code)
    InternalMessage('New')

def OMG2ToGraph(SaveToDefaultGraph, UrlPost, headerPost,graph,subject, geometry,geometryURI, geometryformat,geometryLiteral, geometryliteraltype, PartOf,referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, creator, geometrytype, createdin, coordinatesystem, geometrycontext,DerivedFromGeometry,Language):
    if geometryformat == 'fog:asPly_v1.0-ascii':
        geometryliteraltype = "string"
    query = """
            PREFIX omg: <https://w3id.org/omg#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            PREFIX fog: <https://w3id.org/fog#>
            PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            PREFIX dcterms: <http://purl.org/dc/terms/>
            PREFIX prov: <http://www.w3.org/ns/prov#>
            PREFIX gom: <https://w3id.org/gom#>
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            INSERT DATA{"""
    if not SaveToDefaultGraph:
        query = query+ """GRAPH <%s> {""" %graph

    query = query + """
            <%s> omg:hasGeometry <%s>."""%(subject, geometryURI)
    if geometrytype :
        query = query +"""<%s> rdf:type <%s>;
                rdfs:label "%s"@%s;
        """%(geometryURI,geometrytype, geometry, Language)
    else:
        query = query + """<%s> rdf:type omg:Geometry;
                rdfs:label "%s"@%s;
        """%(geometryURI,geometry, Language)
    if PartOf:
        query = query + """
                %s \"""%s\"""^^xsd:%s;
                omg:isPartOfGeometry <%s>.
        """ % ( geometryformat, geometryLiteral,geometryliteraltype, PartOf)
    else:    
        query = query + """
                %s \"""%s\"""^^xsd:%s.
        """ % ( geometryformat, geometryLiteral,geometryliteraltype)

    if referencedcontent:
        query = query + """
        <%s>    fog:hasReferencedContent    <%s>.
        <%s>    rdf:type                    fog:ReferencedContent;
                %s                          \"""%s\"""^^xsd:%s;
                rdfs:label                  "%s"@%s.

        """%(geometryURI, referencedcontentURI, referencedcontentURI,referencedcontentformat,referencedcontentLiteral, referencedcontentliteraltype, referencedcontent,Language)
    if creator:
        query = query +"""
        <%s> dcterms:creator "%s".
        """%(geometryURI, creator)
    if createdin:
        query = query +"""
        <%s> gom:createdIn <%s>.
        """%(geometryURI, createdin)
    if coordinatesystem:
        query = query +"""
        <%s> gom:hasCoordinateSystem <%s>.
        """%(geometryURI, coordinatesystem)
    
    if geometrycontext:
        for gc in geometrycontext:
            query = query +"""
            <%s> omg:hasGeometryContext <%s>.
            """%(geometryURI, gc)
    if DerivedFromGeometry:
        for derivedgeometry in DerivedFromGeometry:
            query = query +"""
            <%s> omg:isDerivedFromGeometry <%s>.
            """%(geometryURI, derivedgeometry)

    query = query +"""
        <%s> prov:generatedAtTime "%s"^^xsd:dateTime.
        """%(geometryURI, datetime.now().strftime('%Y-%m-%dT%H:%M:%S'))


    if not SaveToDefaultGraph:
        query = query + """} """

    query = query + """} """ 
    r = requests.post(url = UrlPost, headers = headersPost, data = query)
    print("OMG 2 to graph: %s " %r.status_code)
    InternalMessage('New')

def ElementToGraph(SaveToDefaultGraph,UrlPost, headerPost, graph, element, elementuri, ListClasses):
    
    #Start building the insert query
    INSERTelement = """
        PREFIX bot: <https://w3id.org/bot#>
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        INSERT DATA{"""
    if not SaveToDefaultGraph:
        INSERTelement = INSERTelement + """GRAPH <%s>{""" % graph
        
    for ClassURI in ListClasses:
        INSERTelement = INSERTelement + """ <%s> rdf:type <%s>. """ %(elementuri,ClassURI)
        
    INSERTelement = INSERTelement + """<%s> rdfs:label "%s"@%s.}""" %(elementuri, element, Language)
    if not SaveToDefaultGraph:
        INSERTelement = INSERTelement + """}"""      
    r = requests.post(url = UrlPost, headers = headersPost, data = INSERTelement)
    print("Element to graph: %s" %r.status_code)
    InternalMessage('New')

def ZoneToGraph(SaveToDefaultGraph,UrlPost, headersPost, graph, zone, zoneuri, ListClasses):
    #Start building the insert query
    INSERTzone = """
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        INSERT DATA{"""
    if not SaveToDefaultGraph:
        INSERTzone = INSERTzone + """GRAPH <%s>{""" % graph
        
    for ClassURI in ListClasses:
        INSERTzone = INSERTzone + """ <%s> rdf:type <%s>. """ %(zoneuri,ClassURI)
        
    INSERTzone = INSERTzone + """<%s> rdfs:label "%s"@%s.}""" %(zoneuri, zone, Language)
    if not SaveToDefaultGraph:
        INSERTzone = INSERTzone + """}"""   
    r = requests.post(url = UrlPost, headers = headersPost, data = INSERTzone)
    print("Zone to graph: %s "%r.status_code)
    InternalMessage('New')

def ObjectToGraph(SaveToDefaultGraph, UrlPost, headerPost, name,layer,namespace, Namedgraph,objectIRI):
    if layer.startswith('http'):
        layer = "<"+layer+">"

    #Start building the insert query
    INSERTobject = """
        PREFIX bot: <https://w3id.org/bot#>
        PREFIX beo: <https://pi.pauwel.be/voc/buildingelement#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        INSERT DATA{"""
    if not SaveToDefaultGraph:
        INSERTobject = INSERTobject + """    GRAPH <%s>{"""%Namedgraph
    INSERTobject = INSERTobject + """
                <%s> a %s; 
                rdfs:label "%s"@%s."""%(objectIRI,layer,name, Language)
    if layer.startswith('beo'):
        INSERTobject = INSERTobject + """<%s> a %s."""%(objectIRI, 'bot:Element')
    INSERTobject = INSERTobject + """}""" 
    if not SaveToDefaultGraph:
        INSERTobject = INSERTobject + """}"""    
    r = requests.post(url = UrlPost, headers = headersPost, data = INSERTobject)
    print("object to graph: %s "%r.status_code)
    InternalMessage('New')

def ObjectToGraphwithRelations(SaveToDefaultGraph,UrlPost, headerPost, name,layer,namespace, Namedgraph,objectIRI,groups,origin):
    Namedgraph = Namedgraph
    if layer.startswith('http'):
        layer = "<"+layer+">"
    
    #Start building the insert query
    INSERTobject = """
        PREFIX bot: <https://w3id.org/bot#>
        PREFIX beo: <https://pi.pauwel.be/voc/buildingelement#>
        PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#>
        INSERT DATA{"""
    if not SaveToDefaultGraph:
        INSERTobject = INSERTobject + """    GRAPH <%s>{"""%Namedgraph
    INSERTobject = INSERTobject + """
                <%s> a %s; 
                rdfs:label "%s"@%s."""%(objectIRI,layer,name, Language)
                
    for group in groups:
        groupIRI = namespace + quote(group)
        INSERTobject = INSERTobject+ """
                <%s> bot:hasSubElement <%s>.
        """% (groupIRI, objectIRI)
    INSERTobject = INSERTobject + """}"""
    if not SaveToDefaultGraph:
        INSERTobject = INSERTobject + """    GRAPH <%s>{"""%Namedgraph     
    r = requests.post(url = UrlPost, headers = headersPost, data = INSERTobject)
    print("object with relations to graph: %s"%r.status_code)
    InternalMessage('New')

def RecievedPartOfModel(SaveToDefaultGraph,UrlPost, headersPost,ProjectName,layer,name,location,GeometriesSettings,PreferedFormat,SupportedGeometryFormat,origin,DefaultCS,**kwargs):
    #Object aanmaken
    namespace = "http://LBDA_%s.edu/Model/" % quote(ProjectName)
    subject = namespace + quote(name+ datetime.now().strftime('%H%M%d%m%Y')) #Make sure there are no illigal caracters in the URI 
    graph = "http://LBDA_%s.edu/LBDmodel" %quote(ProjectName)
    
    ObjectToGraph(SaveToDefaultGraph,UrlPost, headersPost, name,layer, namespace, graph, subject)
    global SupportedGeometryFormats
    SupportedGeometryFormats = SupportedGeometryFormat
    geometryformat = DetermineFormat(location,SupportedGeometryFormats)

    if GeometriesSettings[3] == 'Embeded':
        geometryLiteral = READfile(location)
        geometryliteraltype = PreferedFormat[6]      
    if GeometriesSettings[3] == 'Local file':
        geometryLiteral = location
        geometryLiteral = geometryLiteral.replace("\\","\\\\")
        geometryLiteral = str(geometryLiteral)
        geometryliteraltype = "anyURI"
    if GeometriesSettings[3] == 'Online file':
        geometryLiteral = location
        geometryLiteral = geometryLiteral.replace("\\","\\\\")
        geometryLiteral = str(geometryLiteral)
        geometryliteraltype = "anyURI"
    
    
    if GeometriesSettings[2] == "OMG L1":   
        OMG1ToGraph(SaveToDefaultGraph,UrlPost, headersPost, graph,geometryformat, subject, geometryLiteral, geometryliteraltype)
    else:
        geometry = "Geometry_" + name 
        geometryURI = namespace + quote(geometry + datetime.now().strftime('%H%M%d%m%Y'))
        referencedcontentfilelocation = ""
        if PreferedFormat[3]== '1':
            referencedcontentformat1 = PreferedFormat[4]
            for geometryformat1 in SupportedGeometryFormats:
                if referencedcontentformat1 == geometryformat1[0]:
                    referencedcontentformat = geometryformat1[1]
                    referencedcontentliteraltype = geometryformat1[3]
                    referencedcontent = geometry + "_ReferencedContent"
                    referencedcontentURI = namespace + quote(referencedcontent + datetime.now().strftime('%H%M%d%m%Y'))
                    referencedcontentfilelocation = location.replace(PreferedFormat[2], geometryformat1[2])
                    if os.path.isfile(referencedcontentfilelocation):
                        if GeometriesSettings[3] == 'Embeded':
                            referencedcontentLiteral = READfile(referencedcontentfilelocation)
                            referencedcontentliteraltype = PreferedFormat[6]        
                        if GeometriesSettings[3] == 'Local file':
                            referencedcontentLiteral = referencedcontentfilelocation
                            referencedcontentLiteral = referencedcontentLiteral.replace("\\","\\\\")
                            referencedcontentLiteral = str(referencedcontentLiteral)
                            referencedcontentliteraltype = "anyURI"
                        if GeometriesSettings[3] == 'Online file':
                            referencedcontentLiteral = referencedcontentfilelocation
                            referencedcontentLiteral = referencedcontentLiteral.replace("\\","\\\\")
                            referencedcontentLiteral = str(referencedcontentLiteral)
                            referencedcontentliteraltype = "anyURI"
                    else:
                        referencedcontent = ""
                        referencedcontentURI =""
                        referencedcontentformat = ""
                        referencedcontentliteraltype = ""
                        referencedcontentfilelocation = ""
                        referencedcontentLiteral = ""

        else:
            referencedcontent = ""
            referencedcontentURI =""
            referencedcontentformat = ""
            referencedcontentliteraltype = ""
            referencedcontentfilelocation = ""
            referencedcontentLiteral = ""
            referencedcontentliteraltype = ""
        if GeometriesSettings[2] == "OMG L2":   
            OMG2ToGraph(SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometryformat,geometryLiteral, geometryliteraltype,None, referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, None, None, origin,DefaultCS, None,None,Language)
          
        if GeometriesSettings[2] == "OMG L3":
            geometrystate = "Geometry_State_"+ name
            geometrystateURI= namespace + quote(geometrystate + datetime.now().strftime('%H%M%d%m%Y'))
            OMG3ToGraph(SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometrystate, geometrystateURI, geometryformat,geometryLiteral, geometryliteraltype, None  ,referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, None,None,origin,DefaultCS,None,None, Language)
            
def RecievedPartOfModelrelations(SaveToDefaultGraph, UrlPost, headersPost,ProjectName,layer,name,location,GeometriesSettings,PreferedFormat,SupportedGeometryFormat,groups, origin,DefaultCS):
    #Object aanmaken
    namespace = "http://LBDA_%s.edu/Model/" % quote(ProjectName)
    subject = namespace + quote(name+ datetime.now().strftime('%H%M%d%m%Y')) #Make sure there are no illigal caracters in the URI 
    graph = "http://LBDA_%s.edu/LBDmodel" %quote(ProjectName)
    ObjectToGraphwithRelations(SaveToDefaultGraph,UrlPost, headersPost, name,layer, namespace, graph, subject,groups,origin)
    global SupportedGeometryFormats
    SupportedGeometryFormats = SupportedGeometryFormat
    geometryformat = DetermineFormat(location,SupportedGeometryFormats)

    if GeometriesSettings[3] == 'Embeded':
        geometryLiteral = READfile(location)
        geometryliteraltype = PreferedFormat[6]      
    if GeometriesSettings[3] == 'Local file':
        geometryLiteral = location
        geometryLiteral = geometryLiteral.replace("\\","\\\\")
        geometryLiteral = str(geometryLiteral)
        geometryliteraltype = "anyURI"
    if GeometriesSettings[3] == 'Online file':
        geometryLiteral = location
        geometryLiteral = geometryLiteral.replace("\\","\\\\")
        geometryLiteral = str(geometryLiteral)
        geometryliteraltype = "anyURI"
    
    if GeometriesSettings[2] == "OMG L1":   
        OMG1ToGraph(False,UrlPost, headersPost, graph,geometryformat, subject, geometryLiteral, geometryliteraltype)
    else:
        geometry = "Geometry_" + name
        geometryURI = namespace + quote(geometry + datetime.now().strftime('%H%M%d%m%Y')) 
        referencedcontentfilelocation = ""
        if PreferedFormat[3]== '1':
            referencedcontentformat1 = PreferedFormat[4]
            for geometryformat1 in SupportedGeometryFormats:
                if referencedcontentformat1 == geometryformat1[0]:
                    referencedcontentformat = geometryformat1[1]
                    referencedcontentliteraltype = geometryformat1[3]
                    referencedcontent = geometry + "_ReferencedContent"
                    referencedcontentURI = namespace + quote(referencedcontent + datetime.now().strftime('%H%M%d%m%Y'))
                    referencedcontentfilelocation = location.replace(PreferedFormat[2], geometryformat1[2])
                    if os.path.isfile(referencedcontentfilelocation):
                        if GeometriesSettings[3] == 'Embeded':
                            referencedcontentLiteral = READfile(referencedcontentfilelocation)
                            referencedcontentliteraltype = PreferedFormat[6]        
                        if GeometriesSettings[3] == 'Local file':
                            referencedcontentLiteral = referencedcontentfilelocation
                            referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                            referencedcontentLiteral = str(referencedcontentLiteral)
                            referencedcontentliteraltype = "anyURI"
                        if GeometriesSettings[3] == 'Online file':
                            referencedcontentLiteral = referencedcontentfilelocation
                            referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                            referencedcontentLiteral = str(referencedcontentLiteral)
                            referencedcontentliteraltype = "anyURI"
                    else:
                        referencedcontent =""
                        referencedcontentURI =""
                        referencedcontentformat = ""
                        referencedcontentliteraltype = ""
                        referencedcontentfilelocation = ""
                        referencedcontentLiteral = ""
                        referencedcontentliteraltype = ""
        else:
            referencedcontent =""
            referencedcontentURI =""
            referencedcontentformat = ""
            referencedcontentliteraltype = ""
            referencedcontentfilelocation = ""
            referencedcontentLiteral = ""
        if GeometriesSettings[2] == "OMG L2":   
            OMG2ToGraph(SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometryformat,geometryLiteral, geometryliteraltype, None,referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, None, None, origin, DefaultCS, None,None,Language)
          
        if GeometriesSettings[2] == "OMG L3":
            geometrystate = "Geometry_State_"+ name
            geometrystateURI = namespace + quote(geometrystate + datetime.now().strftime('%H%M%d%m%Y'))
            OMG3ToGraph(SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometrystate, geometrystateURI, geometryformat,geometryLiteral, geometryliteraltype, None,referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, None, None, origin, DefaultCS, None,None,Language)

############################################################################################
# USER INTERFACE###
############################################################################################
#Project information
def ProjectInformation(frame):
    
    CloseFrame()

    projectinformationframe = Frame(frame)
    projectinformationframe.pack()
    Frames.append(projectinformationframe)
    
    #Projectname

    def SETprojectname(*args):
        global ProjectName
        ProjectName = ProjectnameVariable.get()
    
    ProjectnameVariable = StringVar()
    ProjectnameVariable.set(ProjectName)

    projectnamelabel = Label(projectinformationframe, text = "Project:")
    projectnamelabel.grid(column = 0, row = 0, sticky = NW)
    
    ProjectnameEntry = Entry(projectinformationframe,textvariable = ProjectnameVariable,  width = 50)
    ProjectnameEntry.grid(column = 1, row =0, sticky = NW)

    ProjectnameVariable.trace_add("write", SETprojectname)

    #Project directory

    def SETprojectdirectory(*args):
        global ProjectDirectory
        ProjectDirectory = ProjectDirectoryVariable.get()
    
    ProjectDirectoryVariable = StringVar()
    ProjectDirectoryVariable.set(ProjectDirectory)

    ProjectDirectoryLabel = Label(projectinformationframe, text = 'Project directory:')
    ProjectDirectoryLabel.grid(column = 0, row = 1, sticky = NW)

    ProjectDirectoryEntry = Entry(projectinformationframe,textvariable = ProjectDirectoryVariable, width = 75)
    ProjectDirectoryEntry.grid(column = 1, row =1, sticky=NW)

    ProjectDirectoryInformationLabel = Label(projectinformationframe, text = "Will be used to store the projectfile")
    ProjectDirectoryInformationLabel.grid(column = 1, row = 2, sticky = NW)

    BrowseProjectDirectoryButton = Button(projectinformationframe, text = 'Browse', command = partial(BrowseDirectory,ProjectDirectoryEntry))
    BrowseProjectDirectoryButton.grid(column = 2, row = 1, sticky = NW)

    ProjectDirectoryVariable.trace_add("write",SETprojectdirectory)

    #Geometry directory

    def SETgeometrydirectory(*args):
        global ProjectGeometryDirectory
        ProjectGeometryDirectory = ProjectGeometryDirectoryVariable.get()

    ProjectGeometryDirectoryVariable = StringVar()
    ProjectGeometryDirectoryVariable.set(ProjectGeometryDirectory)

    ProjectGeometryDirectoryLabel = Label(projectinformationframe, text = "Geometry directory:")
    ProjectGeometryDirectoryLabel.grid(column = 0, row = 3, sticky = NW)

    ProjectGeometryDirectoryEntry = Entry(projectinformationframe, textvariable = ProjectGeometryDirectoryVariable, width = 75)
    ProjectGeometryDirectoryEntry.grid(column = 1, row = 3, sticky = NW)

    ProjectGeometryDirectoryButton = Button(projectinformationframe, text = 'Browse', command = partial(BrowseDirectory, ProjectGeometryDirectoryEntry))
    ProjectGeometryDirectoryButton.grid(column = 2, row =3, sticky = NW)

    ProjectGeometryDirectoryInformationLabel = Label(projectinformationframe, text = "This directory will be used to store geometries that are exported by connected applications")
    ProjectGeometryDirectoryInformationLabel.grid(column = 1, row = 4, sticky = NW)

    ProjectGeometryDirectoryVariable.trace_add("write", SETgeometrydirectory)

    #Checkbox to delete files from the directory ones they are loaded into the Linked Data Graph
    
    def SETdeletefiles(*args):
        global DeleteGeometryFiles
        if DeleteFilesVariable.get() == 1:
            DeleteGeometryFiles = True
        else:
            DeleteGeometryFiles = False
    
    DeleteFilesVariable = IntVar()
    if DeleteGeometryFiles == True:
        DeleteFilesVariable.set(1)
    else:
        DeleteFilesVariable.set(0)
    
    DeleteFilesCheckbutton = Checkbutton(projectinformationframe, text = 'Delete geometry files when description is loaded into the graph', variable = DeleteFilesVariable, onvalue =1, offvalue = 0)
    DeleteFilesCheckbutton.grid(column =1, row = 5, columnspan = 2, sticky = NW)

    DeleteFilesVariable.trace("w", SETdeletefiles)    

    #Creator

    def SETprojectcreator(*args):
        global ProjectCreator
        ProjectCreator = ProjectCreatorVariable.get()

    ProjectCreatorVariable = StringVar()
    ProjectCreatorVariable.set(ProjectCreator)

    ProjectCreatorLabel = Label(projectinformationframe, text = "Creator:")
    ProjectCreatorLabel.grid(column = 0, row = 6, sticky = NW)
    
    ProjectCreatorEntry = Entry(projectinformationframe, textvariable = ProjectCreatorVariable, width = 50)
    ProjectCreatorEntry.grid(column = 1, row = 6, sticky = NW)

    ProjectCreatorVariable.trace_add("write", SETprojectcreator)

    #GeometryContexts
    GeometryContextsLabel = Label(projectinformationframe, text = "Geometry contexts:")
    GeometryContextsLabel.grid(column =0, row = 7, sticky = NW)

    GeometryContextsTreeview = Treeview(projectinformationframe)
    GeometryContextsTreeview.grid(column = 0, row = 8, sticky = NW, columnspan = 3)

    GeometryContextsTreeview["selectmode"] = BROWSE
    GeometryContextsTreeview["columns"] = ("one")
    GeometryContextsTreeview.column("#0", width = 400)
    GeometryContextsTreeview.heading("#0", text = "URI")
    GeometryContextsTreeview.column("one", width = 200)
    GeometryContextsTreeview.heading("one", text = "Name")

    GeometryContextsButton = Button(projectinformationframe, text = "Add", command = partial(NewGeometryContext,frame))
    GeometryContextsButton.grid(column = 0, row = 9)

    ListGeometryContexts = SearchGeometryContexts()
    for geometrycontext in ListGeometryContexts:
        GeometryContextsTreeview.insert("",END, geometrycontext[0], text = geometrycontext[0], values = (geometrycontext[1],))

def NewGeometryContext(frame):

    NewGeometryFrameToplevel = Toplevel(root)
    NewGeometryFrameToplevel.title("New Geometry Context")

    newgeometrycontextframe = Frame(NewGeometryFrameToplevel)
    newgeometrycontextframe.pack()

    NewGeometryContextNameVariable = StringVar()
    NewGeometryContextURIVariable = StringVar()
    NewGeometryContextCreatorVariable = StringVar()
    NewGeometryContextCreatorVariable.set(ProjectCreator)

    GeometryContextNameLabel = Label(newgeometrycontextframe, text = 'Geometry context:')
    GeometryContextNameLabel.grid(column =0, row =0, sticky = NW)

    GeometryContextNameEntry = Entry(newgeometrycontextframe, textvariable = NewGeometryContextNameVariable, width = 50)
    GeometryContextNameEntry.grid(column = 1, row =0, sticky = NW)
    def GeometryContextName(*args):
        GeometryURI = GeometriesSettings[1] + quote(NewGeometryContextNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
        NewGeometryContextURIVariable.set(GeometryURI)

    NewGeometryContextNameVariable.trace_add("write", GeometryContextName)

    GeometryContextURILabel = Label(newgeometrycontextframe, text = 'URI')
    GeometryContextURILabel.grid(column =0, row =1, sticky = NW)

    GeometryContextURIEntry = Entry(newgeometrycontextframe, textvariable = NewGeometryContextURIVariable, width = 75)
    GeometryContextURIEntry.grid(column = 1, row = 1, sticky = NW)

    GeometryContextCreatorLabel = Label(newgeometrycontextframe, text = 'Creator:')
    GeometryContextCreatorLabel.grid(column = 0, row =2, sticky = NW)

    GeometryContextCreatorEntry = Entry(newgeometrycontextframe, textvariable= NewGeometryContextCreatorVariable, width = 50)
    GeometryContextCreatorEntry.grid(column =1, row = 2, sticky = NW)

    GeometryContextDescriptionLabel = Label(newgeometrycontextframe, text = 'Description')
    GeometryContextDescriptionLabel.grid(column = 0, row = 3, sticky = NW)

    GeometryContextDescriptionText = Text(newgeometrycontextframe, width = 75)
    GeometryContextDescriptionText.grid(column = 1, row = 3, sticky = NW)

    AddGeometriesToContextLabel = Label(newgeometrycontextframe, text = "Add geometries to the new geometry context:")
    AddGeometriesToContextLabel.grid(column = 0, row = 4, columnspan = 2, sticky = NW)

    GeometriesTreeview = Treeview(newgeometrycontextframe)
    GeometriesTreeview.grid(column = 0, row = 5, columnspan =2, sticky = NW)
    GeometriesTreeview["selectmode"] = EXTENDED
    GeometriesTreeview["columns"] = ("one","two")
    GeometriesTreeview.column("#0", width = 500, stretch = NO)
    GeometriesTreeview.heading("#0", text = "URI")
    GeometriesTreeview.column("one", width = 250)
    GeometriesTreeview.heading("one", text = "Name")
    GeometriesTreeview.column("two", width = 75)
    GeometriesTreeview.heading("two", text = "OMG level")

    SearchGeometries(GeometriesTreeview)
    def SAVEgeometrycontext():
        NamedGraph = GeometriesSettings[0]
        geometrycontext = NewGeometryContextNameVariable.get()
        geometrycontextURI = NewGeometryContextURIVariable.get()
        creator = NewGeometryContextCreatorVariable.get()
        
        if GeometryContextDescriptionText.compare("end-1c", "==", "1.0"):
            print("the widget is empty")
            description= None
        else: 
            description = GeometryContextDescriptionText.get("1.0",END)
        geometries = GeometriesTreeview.selection()
        GeometryContextToGraph(SaveToDefaultGraph, UrlPost, headersPost, NamedGraph, geometrycontext, geometrycontextURI, creator, description, geometries, Language)
        ProjectInformation(frame)
        NewGeometryFrameToplevel.destroy()

    SaveNewGeometryContextButton = Button(newgeometrycontextframe, text = 'Save', command = partial(SAVEgeometrycontext))
    SaveNewGeometryContextButton.grid(column =1, row = 6, sticky = SE)

    NewGeometryFrameToplevel.mainloop()

def Coordinates(frame):
    CloseFrame()

    coordinatesinformationframe = Frame(frame)
    coordinatesinformationframe.pack()
    Frames.append(coordinatesinformationframe)

    #coordinates
    CoordinatesLabel = Label(coordinatesinformationframe, text = "Coordinate systems:")
    CoordinatesLabel.grid(column =0, row = 0, sticky = NW)

    CoordinatesTreeview = Treeview(coordinatesinformationframe)
    CoordinatesTreeview.grid(column = 0, row = 1, sticky = NW, columnspan = 3)

    CoordinatesTreeview["selectmode"] = BROWSE
    CoordinatesTreeview["columns"] = ("one")
    CoordinatesTreeview.column("#0", width = 400)
    CoordinatesTreeview.heading("#0", text = "URI")
    CoordinatesTreeview.column("one", width = 300)
    CoordinatesTreeview.heading("one", text = "Name")

    AddCoordinateSystemButton = Button(coordinatesinformationframe, text = "Add", command =partial(NewCoordinateSystem, frame))
    AddCoordinateSystemButton.grid(column = 0, row = 2)

    ListCoordinates = SearchCoordinates()
    print(ListCoordinates)
    for CS in ListCoordinates:
        CoordinatesTreeview.insert("",END,CS[0],text = CS[0], values = (CS[1],))
    
def NewCoordinateSystem(frame):
    
    NewCoordinateSystemToplevel = Toplevel(root)
    NewCoordinateSystemToplevel.title("New Coordinate system")

    newcoordinatesystemframe = Frame(NewCoordinateSystemToplevel)
    newcoordinatesystemframe.pack()

    NewCoordinateSystemNameVariable = StringVar()
    NewCoordinateSystemURIVariable = StringVar()
    NewCoordinateSystemCreatorVariable = StringVar()
    NewCoordinateSystemCreatorVariable.set(ProjectCreator)
    NewCoordinateSystemLengthUnitVariable = StringVar()

    CoordinateSystemNameLabel = Label(newcoordinatesystemframe, text = 'Coordinate system:')
    CoordinateSystemNameLabel.grid(column =0, row =0, sticky = NW)

    CoordinateSystemNameEntry = Entry(newcoordinatesystemframe, textvariable = NewCoordinateSystemNameVariable, width = 50)
    CoordinateSystemNameEntry.grid(column = 1, row =0, sticky = NW)
    def CoordinateSystemName(*args):
        NewCoordinateSystemURI = "http://LBDA_%s.edu/CoordinateSystem/" % quote(ProjectName) + quote(NewCoordinateSystemNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
        NewCoordinateSystemURIVariable.set(NewCoordinateSystemURI)

    NewCoordinateSystemNameVariable.trace_add("write", CoordinateSystemName)

    CoordinateSystemURILabel = Label(newcoordinatesystemframe, text = 'URI')
    CoordinateSystemURILabel.grid(column =0, row =1, sticky = NW)

    CoordinateSystemURIEntry = Entry(newcoordinatesystemframe, textvariable = NewCoordinateSystemURIVariable, width = 75)
    CoordinateSystemURIEntry.grid(column = 1, row = 1, sticky = NW)

    CoordinateSystemCreatorLabel = Label(newcoordinatesystemframe, text = 'Creator:')
    CoordinateSystemCreatorLabel.grid(column = 0, row =2, sticky = NW)

    CoordinateSystemCreatorEntry = Entry(newcoordinatesystemframe, textvariable= NewCoordinateSystemCreatorVariable, width = 50)
    CoordinateSystemCreatorEntry.grid(column =1, row = 2, sticky = NW)

    CoordinateSystemDescriptionLabel = Label(newcoordinatesystemframe, text = 'Description')
    CoordinateSystemDescriptionLabel.grid(column = 0, row = 3, sticky = NW)

    CoordinateSystemDescriptionText = Text(newcoordinatesystemframe, width = 75)
    CoordinateSystemDescriptionText.grid(column = 1, row = 3, sticky = NW)

    ListUnits = []
    CoordinateSystemLengthUnits = []
    dir = os.path.dirname(__file__)
    filename = os.path.join(dir, 'Settings','Units.txt')
    unitsfile = open(filename,"r")
    for line in unitsfile:
        line = line.split(",")
        ListUnits.append((line[0], line[1]))
        CoordinateSystemLengthUnits.append(line[0])

    CoordinateSystemLengthUnitsLabel = Label(newcoordinatesystemframe, text = 'Length unit:')
    CoordinateSystemLengthUnitsLabel.grid(column = 0, row = 4, sticky = NW)

    CoordinateSystemLengthUnitsCombobox = Combobox(newcoordinatesystemframe, width = 50, values = CoordinateSystemLengthUnits)
    CoordinateSystemLengthUnitsCombobox.grid(column =1, row = 4, sticky = NW)

    def LengthUnitSelected(*args):
        selectedunit = CoordinateSystemLengthUnitsCombobox.get()
        for unit in ListUnits:
            if selectedunit == unit[0]:
                NewCoordinateSystemLengthUnitVariable.set(unit[1])

    CoordinateSystemLengthUnitsCombobox.bind('<<ComboboxSelected>>', LengthUnitSelected)
        
    def SAVEcoordinatesystem(frame):
        
        NamedGraph = "http://LBDA_%s.edu/CoordinateSystems" %quote(ProjectName)
        coordinatesystem = NewCoordinateSystemNameVariable.get()
        coordinatesystemURI = NewCoordinateSystemURIVariable.get()
        creator = NewCoordinateSystemCreatorVariable.get()
        
        if CoordinateSystemDescriptionText.compare("end-1c", "==", "1.0"):
            print("the widget is empty")
            description= None
        else: 
            description = CoordinateSystemDescriptionText.get("1.0",END)
        
        unit = NewCoordinateSystemLengthUnitVariable.get()
        CoordinateSystemToGraph(SaveToDefaultGraph, UrlPost, headersPost, NamedGraph, coordinatesystem, coordinatesystemURI, creator, description, unit, Language)
        Coordinates(frame)
        NewCoordinateSystemToplevel.destroy()

    SaveNewGeometryContextButton = Button(newcoordinatesystemframe, text = 'Save', command = partial(SAVEcoordinatesystem, frame))
    SaveNewGeometryContextButton.grid(column =1, row = 6, sticky = SE)

    NewCoordinateSystemToplevel.mainloop()

def Ontologies(frame):
    CloseFrame()

    ontologiesinformationframe = Frame(frame)
    ontologiesinformationframe.pack()
    Frames.append(ontologiesinformationframe)

    #Search all ontologies present in the graph
    OntologiesList = SearchOntologies()

    #Ontologies
    OntologiesLabel = Label(ontologiesinformationframe, text = "Ontologies")
    OntologiesLabel.grid(column = 0, row =0, sticky = NW)

    OntologiesTreeview = Treeview(ontologiesinformationframe)
    OntologiesTreeview.grid(column = 0, row = 1, sticky = NW)

    OntologiesTreeview["selectmode"] = BROWSE
    OntologiesTreeview["columns"] = ("one","two","three")
    OntologiesTreeview.column("#0", width = 250)
    OntologiesTreeview.heading("#0", text = "URI")
    OntologiesTreeview.column("one", width = 250)
    OntologiesTreeview.heading("one", text = "Name")
    OntologiesTreeview.column("two", width = 250)
    OntologiesTreeview.heading("two", text = "Named graph")
    OntologiesTreeview.column("three", width = 100)
    OntologiesTreeview.heading("three", text = "Version")

    for ontology in OntologiesList:
        try:
            OntologiesTreeview.insert("",END,ontology[1],text = ontology[1], values = (ontology[0],ontology[2],ontology[3]))
        except:
            print("Ontology already in list.")
    

    OntologiesButtonsFrame = Frame(ontologiesinformationframe)
    OntologiesButtonsFrame.grid(column =0, row =2,sticky = SE)

    def DeleteSelectedOntologies():
        for ontology in OntologiesTreeview.selection():
            print(ontology)
            DeleteOntology(OntologiesTreeview.item(ontology)['values'][1])
            
    DeleteOntologyButton = Button(OntologiesButtonsFrame, text = 'Delete', command = DeleteSelectedOntologies)
    DeleteOntologyButton.grid(column = 0, row = 0, sticky = NW)
    
    def AddOntology(frame):
        def LoadNewOntology(frame):
            Ontologyfile = NewOntologyFileLocationEntry.get()
            LoadOntology(Ontologyfile)
            Ontologies(frame)
            AddOntologyWindow.destroy()


        AddOntologyWindow = Toplevel(root)
        AddOntologyWindow.title("New Ontology")
        AddOntologyWindow.attributes("-topmost", True)

        AddNewOntologyInfoLabel = Label(AddOntologyWindow, text = 'Create an add a new ontology to the project')
        AddNewOntologyInfoLabel.grid(column=0, row = 0, columnspan = 3, sticky = NW)
        NewOntologyFileLocationLabel = Label(AddOntologyWindow, text = 'file location')
        NewOntologyFileLocationLabel.grid(column = 0, row = 2, sticky = NW)
        NewOntologyFileLocationEntry = Entry(AddOntologyWindow, width = 50)
        NewOntologyFileLocationEntry.grid(column =1, row =2, sticky = NW)
        BrowseNewOntologyFileButton = Button(AddOntologyWindow, text = 'Browse', command = partial(BrowseOntology,NewOntologyFileLocationEntry))
        BrowseNewOntologyFileButton.grid(column = 2, row = 2, sticky = NW)
        LoadOntologyButton = Button(AddOntologyWindow, text = 'Load', command = partial(LoadNewOntology,frame))
        LoadOntologyButton.grid(column =2, row =4, sticky = SE)

        AddOntologyWindow.mainloop()

    AddOntologyButton = Button(OntologiesButtonsFrame, text = 'Add', command = partial(AddOntology,frame))
    AddOntologyButton.grid(column = 1, row = 0, sticky = NW)

#Settings
def LinkedData(frame):
    
    CloseFrame()

    linkeddatasettingsframe = Frame(frame)
    linkeddatasettingsframe.pack()
    Frames.append(linkeddatasettingsframe)

    #Linked Data Manager

    def LinkeddataCombobox(event):
        global LinkedDataManager
        LinkedDataManager = LinkeddataOptionsCombobox.get()
    
    LinkeddataOptions = ['GraphDB', 'In memory']

    LinkedDataManagerLabel = Label(linkeddatasettingsframe, text = "Linked Data manager")
    LinkedDataManagerLabel.grid(column = 0, row = 0, sticky = NW)
    
    LinkeddataOptionsCombobox = Combobox(linkeddatasettingsframe, values = LinkeddataOptions)
    LinkeddataOptionsCombobox.grid(column = 1, row =0)
    LinkeddataOptionsCombobox.current(0)
    LinkeddataOptionsCombobox.bind('<<ComboboxSelected>>',LinkeddataCombobox)

    ##GraphDB settings
    GraphDBSettingsLabelFrame = LabelFrame(linkeddatasettingsframe, text = "GraphDB")
    GraphDBSettingsLabelFrame.grid(column = 1, row =2, columnspan = 1, sticky = NW)

    ###GraphDBportnumber

    def SETgraphdbportnumber():
        global GraphDBportnumber
        GraphDBportnumber = GraphDBportVariable.get()
        global UrlGet
        UrlGet = "http://%s:%s/repositories/%s" % (ip_address, GraphDBportnumber,GraphDBrepository)
        global UrlPost
        UrlPost = "http://%s:%s/repositories/%s/statements" % (ip_address, GraphDBportnumber,GraphDBrepository)

    PortLabel = Label(GraphDBSettingsLabelFrame, text = 'Port:')
    PortLabel.grid(column = 0, row =0, sticky = NW)

    GraphDBportVariable = StringVar()
    GraphDBportVariable.set(GraphDBportnumber)
    PortEntry = Entry(GraphDBSettingsLabelFrame, textvariable = GraphDBportVariable, width = 20)
    PortEntry.grid(column =1, row =0, sticky = NW)
    GraphDBportVariable.trace_add("write", SETgraphdbportnumber)

    ###GraphDB repository
    def SETgraphdbrepository():
        global GraphDBrepository
        GraphDBrepository = GraphDBRepositoryVariable.get()
        global UrlGet
        UrlGet = "http://%s:%s/repositories/%s" % (ip_address, GraphDBportnumber,GraphDBrepository)
        global UrlPost
        UrlPost = "http://%s:%s/repositories/%s/statements" % (ip_address, GraphDBportnumber,GraphDBrepository)

    RepositoryLabel = Label(GraphDBSettingsLabelFrame, text = 'Repository:')
    RepositoryLabel.grid(column = 0, row =1, sticky = NW)
    
    GraphDBRepositoryVariable = StringVar()
    GraphDBRepositoryVariable.set(GraphDBrepository)
    GraphDBRepositoryEntry = Entry(GraphDBSettingsLabelFrame, textvariable = GraphDBRepositoryVariable, width = 50)
    GraphDBRepositoryEntry.grid(column =1, row = 1, sticky = NW)
    GraphDBRepositoryVariable.trace_add("write", SETgraphdbrepository)

    #Named Graphs
    GraphSettingsLabelFrame = LabelFrame(linkeddatasettingsframe, text = 'Linked Data Graph')
    GraphSettingsLabelFrame.grid(column = 0, row = 3, columnspan = 2, sticky = NW)
    ##Default graph
    def SETdefaultgraph(*args):
        global SaveToDefaultGraph
        if DefaultGraphVariable.get() == 1:
            ZonesGraphEntry['state'] = 'disabled'
            ElementsGraphEntry['state'] = 'disabled'
            DamagesGraphEntry['state'] = 'disabled'
            GeometriesGraphEntry['state'] = 'disabled'
            SaveToDefaultGraph = True
        else:
            ZonesGraphEntry['state'] = 'enabled'
            ElementsGraphEntry['state'] = 'enabled'
            DamagesGraphEntry['state'] = 'enabled'
            GeometriesGraphEntry['state'] = 'ensabled'
            SaveToDefaultGraph = False

    DefaultGraphVariable = IntVar()
    if SaveToDefaultGraph == True:
        DefaultGraphVariable.set(1)
    else:
        DefaultGraphVariable.set(0)
    
    DefaultGraphCheckbutton = Checkbutton(GraphSettingsLabelFrame, text = 'Save all to default graph', variable = DefaultGraphVariable, onvalue = 1, offvalue = 0)
    DefaultGraphCheckbutton.grid(column = 0, row = 0, sticky = NW)
    DefaultGraphVariable.trace('w', SETdefaultgraph)
    
    ##ZonesGraph
    def SETzonesgraph(*args):
        ZonesSettings[0] == ZonesGraphVariable.get()
    ZonesGraphLabel = Label(GraphSettingsLabelFrame, text = 'Named graph where the zones will be saved:')
    ZonesGraphLabel.grid(column =0, columnspan = 2, row = 1, sticky = NW)

    ZonesGraphVariable = StringVar()
    ZonesGraphVariable.set(ZonesSettings[0])
    
    ZonesGraphEntry = Entry(GraphSettingsLabelFrame,textvariable = ZonesGraphVariable, width = 50)
    ZonesGraphEntry.grid(column = 1, row = 2, sticky = NW)
    
    ZonesGraphVariable.trace_add("write",SETzonesgraph)

    ##Elements graph
    def SETelementsgraph(*args):
        ElementsSettings[0] == ElementsGraphVariable.get()

    ElementsGraphLabel = Label(GraphSettingsLabelFrame, text = 'Named graph where the elements will be saved:')
    ElementsGraphLabel.grid(column = 0, columnspan = 2, row = 3, sticky = NW)

    ElementsGraphVariable = StringVar()
    ElementsGraphVariable.set(ElementsSettings[0])
    
    ElementsGraphEntry = Entry(GraphSettingsLabelFrame, textvariable = ElementsGraphVariable, width = 50)
    ElementsGraphEntry.grid(column = 1, row = 4, sticky = NW)
    
    ElementsGraphVariable.trace_add("write", SETelementsgraph)

    ##Damages graph
    def SETdamagesgraph(*args):
        DamagesSettings[0] = DamagesGraphVariable.get()

    DamagesGraphLabel = Label(GraphSettingsLabelFrame, text = 'Named graph where the Damages will be saved:')
    DamagesGraphLabel.grid(column = 0, columnspan = 2, row = 5, sticky = NW)

    DamagesGraphVariable = StringVar()
    DamagesGraphVariable.set(DamagesSettings[0])
    
    DamagesGraphEntry = Entry(GraphSettingsLabelFrame,textvariable = DamagesGraphVariable, width = 50)
    DamagesGraphEntry.grid(column = 1, row = 6, sticky = NW)
    
    DamagesGraphVariable.trace_add("write", SETdamagesgraph)

    ##Geometry graph
    def SETgeometrygraph(*args):
        GeometriesSettings[0] = GeometryGraphVariable.get()

    GeometriesGraphLabel = Label(GraphSettingsLabelFrame, text = "Named graph where te geomtries will be saved:")
    GeometriesGraphLabel.grid(column = 0, columnspan = 2, row = 7, sticky = NW)

    GeometryGraphVariable = StringVar()
    GeometryGraphVariable.set(GeometriesSettings[0])
    
    GeometriesGraphEntry = Entry(GraphSettingsLabelFrame, textvariable = GeometryGraphVariable, width = 50)
    GeometriesGraphEntry.grid(column = 1, row = 8, sticky = NW)
    
    GeometryGraphVariable.trace_add("write", SETgeometrygraph)
    

    if DefaultGraphVariable.get() == 1:
        ZonesGraphEntry['state'] = 'disabled'
        ElementsGraphEntry['state'] = 'disabled'
        DamagesGraphEntry['state'] = 'disabled'
        GeometriesGraphEntry['state'] = 'disabled'
    else:
        ZonesGraphEntry['state'] = 'enabled'
        ElementsGraphEntry['state'] = 'enabled'
        DamagesGraphEntry['state'] = 'enabled'
        GeometriesGraphEntry['state'] = 'enabled'

    #Namespaces
    NamespacesLabelFrame = LabelFrame(linkeddatasettingsframe, text = "Namespaces")
    NamespacesLabelFrame.grid(column = 0, row =4, columnspan = 2, sticky = NW)

    ##ZonesNamespace
    def SETzonesNamespace(*args):
        ZonesSettings[1] == ZonesNamespaceVariable.get()
    ZonesNamespaceLabel = Label(NamespacesLabelFrame, text = 'Namespace where the zones will be saved:')
    ZonesNamespaceLabel.grid(column =0, columnspan = 2, row = 1, sticky = NW)

    ZonesNamespaceVariable = StringVar()
    ZonesNamespaceVariable.set(ZonesSettings[1])
    
    ZonesNamespaceEntry = Entry(NamespacesLabelFrame,textvariable = ZonesNamespaceVariable, width = 50)
    ZonesNamespaceEntry.grid(column = 1, row = 2, sticky = NW)
    
    ZonesNamespaceVariable.trace_add("write",SETzonesNamespace)

    ##Elements Namespace
    def SETelementsNamespace(*args):
        ElementsSettings[1] == ElementsNamespaceVariable.get()

    ElementsNamespaceLabel = Label(NamespacesLabelFrame, text = 'Namespace where the elements will be saved:')
    ElementsNamespaceLabel.grid(column = 0, columnspan = 2, row = 3, sticky = NW)

    ElementsNamespaceVariable = StringVar()
    ElementsNamespaceVariable.set(ElementsSettings[1])
    
    ElementsNamespaceEntry = Entry(NamespacesLabelFrame, textvariable = ElementsNamespaceVariable, width = 50)
    ElementsNamespaceEntry.grid(column = 1, row = 4, sticky = NW)
    
    ElementsNamespaceVariable.trace_add("write", SETelementsNamespace)

    ##Damages Namespace
    def SETdamagesNamespace(*args):
        DamagesSettings[1] = DamagesNamespaceVariable.get()

    DamagesNamespaceLabel = Label(NamespacesLabelFrame, text = 'Namespace where the Damages will be saved:')
    DamagesNamespaceLabel.grid(column = 0, columnspan = 2, row = 5, sticky = NW)

    DamagesNamespaceVariable = StringVar()
    DamagesNamespaceVariable.set(DamagesSettings[1])
    
    DamagesNamespaceEntry = Entry(NamespacesLabelFrame,textvariable = DamagesNamespaceVariable, width = 50)
    DamagesNamespaceEntry.grid(column = 1, row = 6, sticky = NW)
    
    DamagesNamespaceVariable.trace_add("write", SETdamagesNamespace)

    ##Geometry Namespace
    def SETgeometryNamespace(*args):
        GeometriesSettings[1] = GeometryNamespaceVariable.get()

    GeometriesNamespaceLabel = Label(NamespacesLabelFrame, text = "Namespace where te geoemtries will be saved:")
    GeometriesNamespaceLabel.grid(column = 0, columnspan = 2, row = 7, sticky = NW)

    GeometryNamespaceVariable = StringVar()
    GeometryNamespaceVariable.set(GeometriesSettings[1])
    
    GeometriesNamespaceEntry = Entry(NamespacesLabelFrame, textvariable = GeometryNamespaceVariable, width = 50)
    GeometriesNamespaceEntry.grid(column = 1, row = 8, sticky = NW)
    
    GeometryNamespaceVariable.trace_add("write", SETgeometryNamespace)

def GeometrySettings(frame):
    CloseFrame()
    
    geometrysettingsframe = Frame(frame)
    geometrysettingsframe.pack()

    Frames.append(geometrysettingsframe)

    #OMG level
    def OMGLevelSelected(event):
        GeometriesSettings[2] = GeometriesOMGLevelCombobox.get()
        if GeometriesOMGLevelCombobox.get() == "OMG L1":
            OMGLevelinfo.set("Link the geometry directly to the object itself")
            SaveAsOptions = ["Embeded","Local file", "Online file"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions
        
        if GeometriesOMGLevelCombobox.get() == "OMG L2":
            OMGLevelinfo.set("Link the geometry to the object using one intermediate node. This methods allows to link relevant metadata.")
            SaveAsOptions = ["Embeded","Local file", "Online file","Part of"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions

        if GeometriesOMGLevelCombobox.get() == "OMG L3":
            OMGLevelinfo.set("Link the geometry to the object using two intermediate nodes. This methods allows to link relevant metadata and use version control.")
            SaveAsOptions = ["Embeded","Local file", "Online file","Part of"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions

    GeometriesOMGLevelLabel = Label(geometrysettingsframe, text = "Default OMG level:")
    GeometriesOMGLevelLabel.grid(column = 0, columnspan = 2, row = 1, sticky = NW)

    OMGLevels = ["OMG L3", "OMG L2", "OMG L1"]

    GeometriesOMGLevelCombobox = Combobox(geometrysettingsframe, values = OMGLevels)
    GeometriesOMGLevelCombobox.set(GeometriesSettings[2])
    GeometriesOMGLevelCombobox.grid(column = 1, row = 2, sticky = NW)
    GeometriesOMGLevelCombobox.bind("<<ComboboxSelected>>", OMGLevelSelected)
    
    OMGLevelinfo = StringVar()
    GeometriesOMGLevelMessage = Message(geometrysettingsframe, textvariable = OMGLevelinfo, width = 300)
    GeometriesOMGLevelMessage.grid(column = 1, row = 3, sticky = NW)
    
    if GeometriesOMGLevelCombobox.get() == "OMG L1":
        OMGLevelinfo.set("Link the geometry directly to the object itself")
        SaveAsOptions = ["Embeded","Local file", "Online file"]
    
    if GeometriesOMGLevelCombobox.get() == "OMG L2":
        OMGLevelinfo.set("Link the geometry to the object using one intermediate node. This methods allows to link relevant metadata.")
        SaveAsOptions = ["Embeded","Local file", "Online file","Part of"]
    
    if GeometriesOMGLevelCombobox.get() == "OMG L3":
        OMGLevelinfo.set("Link the geometry to the object using two intermediate nodes. This methods allows to link relevant metadata and use version control.")
        SaveAsOptions = ["Embeded","Local file", "Online file","Part of"]

    #Save Geometry method
    
    def SaveAsSelected(event):
        GeometriesSettings[3] = GeometriesSaveAsCombobox.get()
        
        if GeometriesSaveAsCombobox.get() == 'Embeded':
            SaveAsinfo.set("""The entire geometry description will be saved into a literal as
            "..."^^xsd:string or "..."^^xsd:base64""")
        
        if GeometriesSaveAsCombobox.get() == 'Local file':
            SaveAsinfo.set("""The entire geometry description will be saved into a literal as
            "..."^^xsd:anyURI""")
        
        if GeometriesSaveAsCombobox.get() == 'Online file':
            SaveAsinfo.set("""The entire geometry description will be saved into a literal as
            "..."^^xsd:anyURI""")
        if GeometriesSaveAsCombobox.get() == 'Part of':
            SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                "...\"""")
    GeometriesSaveAsLabel = Label(geometrysettingsframe, text = "Save as:")
    GeometriesSaveAsLabel.grid(column = 0, row = 4, sticky = NW)

    GeometriesSaveAsCombobox = Combobox(geometrysettingsframe, width = 50,values = SaveAsOptions)
    GeometriesSaveAsCombobox.grid(column = 1, row = 4, sticky = NW)
    GeometriesSaveAsCombobox.set(GeometriesSettings[3])
    GeometriesSaveAsCombobox.bind("<<ComboboxSelected>>", SaveAsSelected)
        
    SaveAsinfo = StringVar()
    GeometriesSaveAsMessage = Message(geometrysettingsframe, textvariable = SaveAsinfo, width = 300)
    GeometriesSaveAsMessage.grid(column = 1, row = 5, sticky = NW)
    
    if GeometriesSaveAsCombobox.get() == 'Description':
        SaveAsinfo.set("""The entire geometry description will be saved into a literal 
            "..."^^xsd:string""")
    
    if GeometriesSaveAsCombobox.get() == 'Local file':
        SaveAsinfo.set("""The entire geometry description will be saved into a literal as
            "..."^^xsd:anyURI""")
    
    if GeometriesSaveAsCombobox.get() == 'Online file':
        SaveAsinfo.set("""The entire geometry description will be saved into a literal as
            "..."^^xsd:anyURI""")
    if GeometriesSaveAsCombobox.get() == 'Part of':
            SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                "...\"""")
    
    #Format
    def SETpreferedformat(*args):
        PreferedGeometryFormat = GeometryformatCombobox.get()
        for geometryformat in SupportedGeometryFormats:
                if geometryformat[0] == PreferedGeometryFormat:
                    global PreferedFormat
                    PreferedFormat = geometryformat
    
    Preferedformatlabel = Label(geometrysettingsframe, text = "Preferd geometryformat")
    Preferedformatlabel.grid(column = 0, row = 6, sticky = NW)
    
    formatsList =[]
    for geometryformat in SupportedGeometryFormats:
        if geometryformat[3] == "0" or geometryformat[3] =="1" :
            formatsList.append(geometryformat[0])
        
    GeometryformatCombobox = Combobox(geometrysettingsframe, width = 50, values = formatsList)
    GeometryformatCombobox.grid(column=1, row = 6, sticky = NW)
    GeometryformatCombobox.set(PreferedFormat[0])
    GeometryformatCombobox.bind("<<ComboboxSelected>>", SETpreferedformat)

    #ShowDescription
    def SETshowdescription(*args):
        global ShowDescriptions
        if SeeDescriptionVariable.get() == 1:
            ShowDescriptions = True
        else:
            ShowDescriptions = False

    SeeDescriptionVariable = IntVar()
    SeeDescriptionVariable.trace("w", SETshowdescription)

    SeeDescriptionCheckbutton = Checkbutton(geometrysettingsframe, text = 'Show Geometry description', variable = SeeDescriptionVariable, onvalue = 1, offvalue =0)
    SeeDescriptionCheckbutton.grid(column = 0, row = 9, columnspan = 2, sticky = NW)

    if ShowDescriptions == True:
        SeeDescriptionVariable.set(1)
    else:
        SeeDescriptionVariable.set(0)

    #Level of export
    def SETlevelofexport(*args):
        global ExportLevel
        ExportLevel = LevelofExportCombobox.get()
        
    LevelofExportLabel = Label(geometrysettingsframe, text = 'level of exported elements:')
    LevelofExportLabel.grid(column = 0, row =7, columnspan = 2, sticky = NW)

    LevelofExportList = ['Element level', 'Subelement level']

    LevelofExportCombobox = Combobox(geometrysettingsframe, width = 75, values = LevelofExportList)
    LevelofExportCombobox.grid(column =1, row = 8, sticky = NW)
    LevelofExportCombobox.set(ExportLevel)
    LevelofExportCombobox.bind("<<ComboboxSelected>>",SETlevelofexport)

def CreateNewProjectFrame(frame):
    
    CloseFrame()
    
    #function that will set al the variables to the input given in this window
    def CreateNewProject():
        global ProjectName
        ProjectName =EntryProjectname.get()

        global ProjectDirectory
        ProjectDirectory = EntryDirectory.get()

        global ProjectGeometryDirectory
        ProjectGeometryDirectory = ProjectGeometryDirectory + "\Geometry" 
    
        ProjectFileName = ProjectName +".txt"    
        ProjectFilePath = ProjectDirectory+"/"+ProjectFileName
        global Projectfile
        Projectfile = ProjectFilePath

        global LinkedDataManager
        LinkedDataManager = LinkeddataOptionsCombobox.get()

        global ZonesSettings
        ZonesSettings = ["http://LBDA_%s.edu/LBD"%quote(ProjectName) , "http://LBDA_%s.edu/Zone/"%quote(ProjectName)]
        
        global ElementsSettings
        ElementsSettings= ["http://LBDA_%s.edu/LBD"%quote(ProjectName) , "http://LBDA_%s.edu/Element/"%quote(ProjectName)]
        global GeometriesSettings
        GeometriesSettings = ["http://LBDA_%s.edu/LBD"%quote(ProjectName) , "http://LBDA_%s.edu/Geometry/"%quote(ProjectName), "OMG L3", "Embeded"]
        
        global DamagesSettings
        DamagesSettings= ["http://LBDA_%s.edu/LBD"%quote(ProjectName) , "http://LBDA_%s.edu/Damage/"%quote(ProjectName)]

        if LinkeddataOptionsCombobox.get() == 'GraphDB':
            
            global GraphDBportnumber
            GraphDBportnumber = PortNumberVariable.get()
            
            global GraphDBrepository
            GraphDBrepository = RepositoryVariable.get()
            
            global UrlGet
            UrlGet = "http://%s:%s/repositories/%s" % (ip_address, GraphDBportnumber,GraphDBrepository)
            
            global UrlPost
            UrlPost = "http://%s:%s/repositories/%s/statements" % (ip_address, GraphDBportnumber,GraphDBrepository)

        #reading the settings
        dir = os.path.dirname(__file__)
        filename = os.path.join(dir, 'Settings','CoordinateReferenceSystems.txt')
        CRSfile = open(filename,"r")
        graph = "http://LBDA_%s.edu/CoordinateSystems" %quote(ProjectName)
        NewCoordinateSystem = "%s_ProjectCS" % (ProjectName)
        NewCoordinateSystemURI = "http://LBDA_%s.edu/CoordinateSystem/" % quote(ProjectName) + quote(NewCoordinateSystem)
        global DefaultCS
        DefaultCS = NewCoordinateSystemURI
        CoordinateSystemToGraph(SaveToDefaultGraph, UrlPost, headersPost, graph, NewCoordinateSystem, NewCoordinateSystemURI, 'Unknown', "Automatically generated by the LBDgeoLink App as a default CS", "unit:M", Language)
        for line in CRSfile:
            line = line.split(",")
            CoordinateSystemToGraph(SaveToDefaultGraph, UrlPost, headersPost, graph, line[1], line[0], 'Unknown', line[2], line[3], Language)
                 
        SAVEtoFile()
        LoadNeededOntologies()
        NewProjectFrame.destroy()
        ProjectInformation(frame)
        
    def BACK():
        StartupFrame(frame)
        
    NewProjectFrame = Frame(frame)
    NewProjectFrame.pack(fill = BOTH, expand = 1)
    Frames.append(NewProjectFrame)

    #Labelframe thet will contain the project information
    ProjectInfolblf = LabelFrame(NewProjectFrame, text = 'Project information: ')
    ProjectInfolblf.grid(column = 0, row = 0, sticky = W, columnspan = 3, padx = 10, pady =10)
    
    #Projectname
    projectlabel = Label(ProjectInfolblf, text='Projectname: ')
    projectlabel.grid(column = 0, row=0, sticky = W)
    
    EntryProjectname = Entry(ProjectInfolblf, width = 50)
    EntryProjectname.insert(0,'Untitled') #Default to Untitled
    EntryProjectname.grid(column = 2, row =0,sticky = W)
    
    #Projectdirectory
    projectmaplabel = Label(ProjectInfolblf, text='Project directory: ')
    projectmaplabel.grid(column = 0, row=1, columnspan = 2, sticky = W)
    
    EntryDirectory = Entry(ProjectInfolblf, width=75)
    EntryDirectory.insert(0, ProjectDirectory)
    EntryDirectory.grid(column = 2, row =1)
    
    ##Button to browse a Directory
    BtnBrowseProjectDirectory = Button(ProjectInfolblf, text = 'Change directory', command = partial(BrowseDirectory,EntryDirectory))
    BtnBrowseProjectDirectory.grid( column= 3, row = 1 )
        
    #Choose the way the linked data will be managed
    LinkeddataOptionslabel = Label(NewProjectFrame, text = 'Linked data management:')
    LinkeddataOptionslabel.grid(column =0, row =1)
    LinkeddataOptions = ['GraphDB', 'In memory']
    
    GraphDBSettingsFrame = Frame(NewProjectFrame)
    #Functions when the settings button is clicked, this will show the possible settings and will disable the combobx until the settings are closed again.
    def GraphDBSettings():
                       
            #print("Linked data manager settings opened")
            GraphDBSettingsFrame.grid(column = 0, row =2, columnspan = 2, sticky = NW, padx = 10, pady = 10)
            PortLabel = Label(GraphDBSettingsFrame, text = 'Port:')
            PortLabel.grid(column = 0, row =0, sticky = NW)
            
            PortEntry = Entry(GraphDBSettingsFrame, textvariable = PortNumberVariable, width = 20)
            PortEntry.grid(column =1, row =0, sticky = NW)
            PortEntry.delete(0, END)
            PortEntry.insert(0, GraphDBportnumber)
            
            RepositoryLabel = Label(GraphDBSettingsFrame, text = 'Repository:')
            RepositoryLabel.grid(column = 0, row =1, sticky = NW)
            
            RepositoryEntry = Entry(GraphDBSettingsFrame, textvariable = RepositoryVariable, width = 50)
            RepositoryEntry.grid(column =1, row = 1, sticky = NW)
            RepositoryEntry.delete(0,END)
            RepositoryEntry.insert(0,GraphDBrepository)
        
        #Function to assign the correct command to the settings button, depending on the selection f the combobox.
    def LinkeddataCombobox(event):
        if LinkeddataOptionsCombobox.get() == 'In memory':
            GraphDBSettingsFrame.grid_remove()
        if LinkeddataOptionsCombobox.get() == 'GraphDB':
            GraphDBSettings()
            
    LinkeddataOptionsCombobox = Combobox(NewProjectFrame, values = LinkeddataOptions)
    LinkeddataOptionsCombobox.current(0)
    LinkeddataOptionsCombobox.grid(column = 1, row =1)
    LinkeddataOptionsCombobox.bind('<<ComboboxSelected>>',LinkeddataCombobox)
    
    if LinkeddataOptionsCombobox.get() == 'In memory':
        print("TODO")
    
    if LinkeddataOptionsCombobox.get() == 'GraphDB':
        RepositoryVariable = StringVar()
        PortNumberVariable = StringVar()
        GraphDBSettingsFrame = Frame(NewProjectFrame)
        GraphDBSettings()

    #Labelframe for Linked data ontologies
    LinkedDataLabelFrame = LabelFrame(NewProjectFrame, text = 'Linked data:')
    LinkedDataLabelFrame.grid(column =0, row = 4, sticky = NW, padx = 10, pady =10, columnspan = 2)
    
    LinkeddataOntologiesLabel = Label(LinkedDataLabelFrame, text = 'The following ontologies will be loaded automatically into the project:')
    LinkeddataOntologiesLabel.grid(column = 0, row = 0,columnspan = 2, sticky = NW)
    i =0
    for Ontology in ListNeededOntologies():
        Ontologylabel = Label(LinkedDataLabelFrame, text = Ontology)
        position = 1 + i
        Ontologylabel.grid(column = 1, row = position, sticky = NW)     
        i +=1   

    CreateNewProjectButton = Button(NewProjectFrame, text = "Create", command = CreateNewProject)
    CreateNewProjectButton.grid(column = 2, row = 5, sticky = SE)

    BackButton = Button(NewProjectFrame, text = 'Back', command = BACK)
    BackButton.grid(column = 2, row = 6, sticky = SE)

def LoadExsistingProjectFrame(frame):
    CloseFrame()
    
    def LoadExistingProject():
        global Projectfile
        Projectfile = ProjectFileEntry.get()
        LOADfromFile()
        
        NewCoordinateSystem = "%s_ProjectCS" % (ProjectName)
        NewCoordinateSystemURI = "http://LBDA_%s.edu/CoordinateSystem/" % quote(ProjectName) + quote(NewCoordinateSystem)
        global DefaultCS
        DefaultCS = NewCoordinateSystemURI
        
        ExistingProjectFrame.destroy()
        ProjectInformation(frame)

    def BACK():
        ExistingProjectFrame.destroy()
        StartupFrame(frame)
        
    ExistingProjectFrame = Frame(frame)
    ExistingProjectFrame.pack(fill = BOTH, expand = 1)
    Frames.append(ExistingProjectFrame)

    ProjectFileLabel = Label(ExistingProjectFrame, text = 'Projectfile:')
    ProjectFileLabel.grid(column = 0, row =1, sticky = NW)
    
    ProjectFileEntry = Entry(ExistingProjectFrame, width = 50)
    ProjectFileEntry.grid(column = 1, row = 1, sticky = NW)
    
    BrowseProjectFileButton = Button(ExistingProjectFrame, text = 'Browse', command = partial(BrowseProjectfile,ProjectFileEntry))
    BrowseProjectFileButton.grid(column = 2, row = 1)

    LoadExistingProjectButton = Button(ExistingProjectFrame, text = 'Load', command = LoadExistingProject)
    LoadExistingProjectButton.grid(column = 2, row = 2, sticky = SE)

    BackButton = Button(ExistingProjectFrame, text = 'Back', command = BACK)
    BackButton.grid(column = 2, row = 3, sticky = SE)

def StartupFrame(frame):
    CloseFrame()
    
    StartupFrame = Frame(frame)
    StartupFrame.pack(side = TOP,fill = BOTH, expand =1)
    Frames.append(StartupFrame)
    
    WelcomeLabel1 = Label(StartupFrame, text = "Linked Building Data Application")
    WelcomeLabel1.pack(side = TOP)
    
    WelcomeLabel2 = Label(StartupFrame, text = "KU Leuven")
    WelcomeLabel2.pack(side = TOP)
        
    StartUpLabel = Label(StartupFrame, text = 'How do you want to start?')
    StartUpLabel.pack(side = TOP)
    
    BtnCreateNewProject = Button(StartupFrame, text = 'New Project', command = partial(CreateNewProjectFrame,frame))
    BtnCreateNewProject.pack(side = TOP)
    
    BtnLoadProject = Button(StartupFrame, text = 'Open Project' ,command = partial(LoadExsistingProjectFrame,frame))
    BtnLoadProject.pack(side = TOP)

#Objects
def Geometries(frame):
    CloseFrame()

    geometriesdetailframe = Frame(frame)
    geometriesdetailframe.pack()

    Frames.append(geometriesdetailframe)

    #Treeview containing all the geometries
    GeometriesTreeview = Treeview(geometriesdetailframe)
    GeometriesTreeview.grid(column = 0, row = 0, padx = 10, pady = 10, sticky = NW)
    GeometriesTreeview["selectmode"] = BROWSE
    GeometriesTreeview["columns"] = ("one","two")
    GeometriesTreeview.column("#0", width = 500, stretch = NO)
    GeometriesTreeview.heading("#0", text = "URI")
    GeometriesTreeview.column("one", width = 250)
    GeometriesTreeview.heading("one", text = "Name")
    GeometriesTreeview.column("two", width = 75)
    GeometriesTreeview.heading("two", text = "OMG level")

    SearchGeometries(GeometriesTreeview)
    GeometriesButtonFrame = Frame(geometriesdetailframe)
    GeometriesButtonFrame.grid(column = 0, row = 1,sticky = SE)
    AddGeometryButton = Button(GeometriesButtonFrame, text = 'Add', command = partial(NewGeometryUI,frame))
    AddGeometryButton.grid(column =0, row = 1)

def NewGeometryUI(frame, **kwargs): 
    CloseFrame()
    def GeometrySaving():
        Geometries(frame)

    def SaveAsSelected(event):
        if GeometriesSaveAsCombobox.get() == 'Embeded':
            SaveAsinfo.set("""The entire geometry description will be saved into a literal 
            "..."^^xsd:string""")
        
        if GeometriesSaveAsCombobox.get() == 'Local file':
            SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
            "..."^^xsd:anyURI""")
        if GeometriesSaveAsCombobox.get() == 'Online file':
            SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
            "..."^^xsd:anyURI""")
        if GeometriesSaveAsCombobox.get() == 'Part of':
            SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                "...\"""")
      
    def OMGLevelSelected(event):
        if OMGLevelCombobox.get() == "OMG L1":
            OMGinfo.set("Link the geometry directly to the object itself")
            OMG2LabelFrame.grid_remove()
            OMG3LabelFrame.grid_remove()
            OMGLevel1Frame()
            SaveAsOptions = ["Embeded","Local file", "Online file"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions

        if OMGLevelCombobox.get() == "OMG L2":
            OMGinfo.set("Link the geometry to the object using one intermediate node. This methods allows to link relevant metadata.")
            OMG1LabelFrame.grid_remove()
            OMG3LabelFrame.grid_remove()
            OMGLevel2Frame()
            SaveAsOptions = ["Embeded","Local file", "Online file", "Part of"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions

        if OMGLevelCombobox.get() == "OMG L3":
            OMGinfo.set("Link the geometry to the object using two intermediate nodes. This methods allows to link relevant metadata and use version control.")
            OMG1LabelFrame.grid_remove()
            OMG2LabelFrame.grid_remove()
            OMGLevel3Frame()
            SaveAsOptions = ["Embeded","Local file", "Online file", "Part of"]
            GeometriesSaveAsCombobox['values'] = SaveAsOptions

    def OMGLevel1Frame():
    
        def LinkObjects():
                                            
            def OMG1ElementSelected(event):
                def GetOMG1():
                    geometryNamedGraph = GeometriesSettings[0]
                    geometryFormat = GeometryFormat.get()
                    subject = ElementsTreeview.selection()[0]
                    label = ElementsTreeview.item(subject)['values'][0]
                            
                    if GeometriesSaveAsCombobox.get() == 'Embeded':
                        geometryLiteral = GeometryDescription.get()
                        geometryLiteraltype = GeometryLiteraltype.get()
                        if DeleteGeometryFiles == True:
                            os.remove(GeometryLocation.get())
                                
                    if GeometriesSaveAsCombobox.get() == 'Local file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                    if GeometriesSaveAsCombobox.get() == 'Online file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                                
                    SaveNewGeometry = Process(target = OMG1ToGraph, args = (SaveToDefaultGraph,UrlPost, headersPost,geometryNamedGraph,geometryFormat, subject, geometryLiteral, geometryLiteraltype))
                    processes.append(SaveNewGeometry)
                    SaveNewGeometry.start()
                    GeometrySaving()

                SaveButton['command'] = GetOMG1
                SaveButton['state'] = 'enabled'

            def OMG1ZoneSelected(event):
                def GetOMG1():
                    geometryNamedGraph = GeometriesSettings[0]
                    geometryFormat = GeometryFormat.get()
                    subject = ZonesTreeview.selection()[0]
                    label = ZonesTreeview.item(subject)['values'][0]
                            
                    if GeometriesSaveAsCombobox.get() == 'Embeded':
                        geometryLiteral = GeometryDescription.get()
                        geometryLiteraltype = GeometryLiteraltype.get()
                        if DeleteGeometryFiles == True:
                            os.remove(GeometryLocation.get())
                                
                    if GeometriesSaveAsCombobox.get() == 'Local file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                    if GeometriesSaveAsCombobox.get() == 'Online file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                            
        
                    SaveNewGeometry = Process(target = OMG1ToGraph, args = (SaveToDefaultGraph,UrlPost, headersPost,geometryNamedGraph,geometryFormat,subject, geometryLiteral, geometryLiteraltype))
                    processes.append(SaveNewGeometry)
                    SaveNewGeometry.start()
                    GeometrySaving()
                        
                SaveButton['command'] = GetOMG1
                SaveButton['state'] = 'enabled'

            def OMG1DamageSelected(event):
                def GetOMG1():
                    geometryNamedGraph = GeometriesSettings[0]
                    geometryFormat = GeometryFormat.get()
                    subject = DamagesTreeview.selection()[0]
                    label = DamagesTreeview.item(subject)['values'][0]
                            
                    if GeometriesSaveAsCombobox.get() == 'Embeded':
                        geometryLiteral = GeometryDescription.get()
                        geometryLiteraltype = GeometryLiteraltype.get()
                        if DeleteGeometryFiles == True:
                            os.remove(GeometryLocation.get())
                                
                    if GeometriesSaveAsCombobox.get() == 'Local file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                    if GeometriesSaveAsCombobox.get() == 'Online file':
                        geometryLiteral = OMG1GeometryfileEntry.get()
                        geometryLiteral = geometryLiteral.replace("\\","\\\\")
                        geometryLiteral = str(geometryLiteral)
                        geometryLiteraltype = "anyURI"
                            
        
                    SaveNewGeometry = Process(target = OMG1ToGraph, args = (SaveToDefaultGraph,UrlPost, headersPost,geometryNamedGraph,geometryFormat,subject, geometryLiteral, geometryLiteraltype))
                    processes.append(SaveNewGeometry)
                    SaveNewGeometry.start()
                    GeometrySaving()
                        
                SaveButton['command'] = GetOMG1
                SaveButton['state'] = 'enabled'

            for f in tempFrames:
                f.destroy()
            LinkToFrame2 = Frame(LinkToFrame)
            LinkToFrame2.grid(column = 0, row = 0, sticky = NW)
            tempFrames.append(LinkToFrame2)
                        
            #print("link possible objects")
                    
            ListZones = SearchZones()
                    
            LinkOMG1ZoneLabel = Label(LinkToFrame2, text = "Zones")
            LinkOMG1ZoneLabel.grid(column = 0, row = 0, sticky = NW)

            ZonesTreeview = Treeview(LinkToFrame2)
            ZonesTreeview.grid(column = 0, row = 1, sticky = NW)
            ZonesTreeview["selectmode"] = BROWSE
            ZonesTreeview["columns"] = ("one")
            ZonesTreeview.column("#0", width = 400)
            ZonesTreeview.heading("#0", text = "URI")
            ZonesTreeview.column("one", width = 200)
            ZonesTreeview.heading("one", text = "Name")

            for zone in ListZones:
                ZonesTreeview.insert("",END,zone[0],text = zone[0], values =(zone[1],))

            ZonesTreeview.bind('<<TreeviewSelect>>', OMG1ZoneSelected)
                    
            ListElements = SearchElements()

            LinkOMG1ElementLabel = Label(LinkToFrame2, text = "Elements")
            LinkOMG1ElementLabel.grid(column = 0, row = 2, sticky = NW)
                    
            ElementsTreeview = Treeview(LinkToFrame2)
            ElementsTreeview.grid(column = 0, row = 3, sticky = NW)
            ElementsTreeview["selectmode"] = BROWSE
            ElementsTreeview["columns"] = ("one")
            ElementsTreeview.column("#0", width = 400)
            ElementsTreeview.heading("#0", text = "URI")
            ElementsTreeview.column("one", width = 200)
            ElementsTreeview.heading("one", text = "Name")

            for element in ListElements:
                ElementsTreeview.insert("",END,element[0],text = element[0], values =(element[1],))

            ElementsTreeview.bind('<<TreeviewSelect>>', OMG1ElementSelected)
                
            ListDamages = SearchDamages()

            LinkOMG3DamageLabel = Label(LinkToFrame2, text = "Damages")
            LinkOMG3DamageLabel.grid(column = 0, row = 6, sticky = NW)
                    
            DamagesTreeview = Treeview(LinkToFrame2)
            DamagesTreeview.grid(column = 0, row = 7, sticky = NW)
            DamagesTreeview["selectmode"] = BROWSE
            DamagesTreeview["columns"] = ("one")
            DamagesTreeview.column("#0", width = 400)
            DamagesTreeview.heading("#0", text = "URI")
            DamagesTreeview.column("one", width = 200)
            DamagesTreeview.heading("one", text = "Name")

            for damage in ListDamages:
                DamagesTreeview.insert("",END,damage[0],text = damage[0], values =(damage[1],))

            DamagesTreeview.bind('<<TreeviewSelect>>', OMG1DamageSelected)

        ReferencedContentVariable = IntVar()
        GeometryFormat = StringVar()
        GeometryDescription = StringVar()
        GeometryLiteraltype = StringVar()
        GeometryLocation = StringVar()
                
        OMG1LabelFrame.grid(column = 0, row = 5, sticky = NW, columnspan = 2)

        OMG1GeometryfileLabel = Label(OMG1LabelFrame, text = "Geometryfile location:")
        OMG1GeometryfileLabel.grid(column =0, row =4, sticky = NW)
                
        OMG1GeometryfileEntry = Entry(OMG1LabelFrame,textvariable = GeometryLocation,width = 75)
        OMG1GeometryfileEntry.grid(column = 1, row = 4, sticky = NW)

        try:
            metadatalabelframe.grid_remove()
        except:
            print("FAILED")
                
        if filelocation:
            Geometryfile = filelocation
            GeometryLocation.set(Geometryfile)

            OMG1GeometryfileEntry['state'] = 'disabled'
            for Geometryformat in SupportedGeometryFormats:
                if Geometryfile.endswith(Geometryformat[2]):
                    GeometryFormat.set(Geometryformat[1])
                    GeometryLiteraltype.set(Geometryformat[6])
                GeometryDescription.set(READfile(GeometryLocation.get()))
        else:
            OMG1BrowseGeometryButton = Button(OMG1LabelFrame, text = "Browse", command = partial(BrowseGeometryfile,OMG1GeometryfileEntry,GeometryFormat,ReferencedContentVariable,GeometryDescription,GeometryLiteraltype))
            OMG1BrowseGeometryButton.grid(column = 2, row = 4, sticky = NE)
                               
        GeometryFormatMessage = Message(OMG1LabelFrame, textvariable = GeometryFormat, width = 300)
        GeometryFormatMessage.grid(column = 1, row = 5, sticky = NW, columnspan = 2)
        if ShowDescriptions == True:
            GeometryDescriptionMessage = Message(OMG1LabelFrame, textvariable = GeometryDescription, width = 300)
            GeometryDescriptionMessage.grid(column = 1, row = 6, sticky = NW, columnspan = 2)

        LinkObjects()

    def OMGLevel2Frame():
        def GetOMG2():
            graph = GeometriesSettings[0]
            subject = SubjectURIVariable.get()
            geometry = GeometryNameVariable.get()
            geometryURI = GeometryURIVariable.get()
            geometryformat = GeometryFormatPropertyVariable.get()
            PartOf = None

            if GeometriesSaveAsCombobox.get() == 'Embeded':
                geometryLiteral = GeometryDescription.get()
                geometryliteraltype = GeometryDatatypeVariable.get()
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedDescription.get()
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = ReferencedContentDatatypeVariable.get()
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None
                if DeleteGeometryFiles == True:
                    os.remove(GeometryLocation.get())
                    if ReferencedContentVariable.get() == 1:
                        os.remove(ReferencedContentLocationVariable.get())

            if GeometriesSaveAsCombobox.get() == 'Local file':
                geometryLiteral = GeometryLocation.get()
                geometryLiteral = geometryLiteral.replace("\\","\\\\")
                geometryLiteral = str(geometryLiteral)
                geometryliteraltype = "anyURI"
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedContentLocationVariable.get()
                    referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                    referencedcontentLiteral = str(referencedcontentLiteral)
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = "anyURI"
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None


            if GeometriesSaveAsCombobox.get() == 'Online file':
                geometryLiteral = GeometryLocation.get()
                geometryLiteral = geometryLiteral.replace("\\","\\\\")
                geometryLiteral = str(geometryLiteral)
                geometryliteraltype = "anyURI"
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedContentLocationVariable.get()
                    referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                    referencedcontentLiteral = str(referencedcontentLiteral)
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = "anyURI"
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None
            if GeometriesSaveAsCombobox.get() == 'Part of':
                geometryLiteral = PartofIDVariable.get()
                geometryliteraltype = "string"
                geometryformat = PartofPropertyVariable.get()
                PartOf = PartofGeometryTreeview.selection()[0]
                referencedcontent = None
                referencedcontentURI = None
                referencedcontentLiteral = None
                referencedcontentformat = None
                referencedcontentliteraltype = None
            else: 
                PartOf = None


            creator = CreatedByVariable.get()
            geometrytype = GeometryTypeVariable.get()
            createdin = CreatedInVariable.get()
            coordinatesystem = CoordinateSystemVariable.get()
            geometrycontext =[]
            for gc in GeometryContextsTreeview.selection():
                geometrycontext.append(gc)
            DerivedFromGeometry =[]
            for derivedgeometry in DerivedfromTreeview.selection():
                DerivedFromGeometry.append(derivedgeometry)

            SaveNewGeometry = Process(target = OMG2ToGraph, args = (SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometryformat,geometryLiteral, geometryliteraltype, PartOf, referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, creator, geometrytype, createdin, coordinatesystem, geometrycontext,DerivedFromGeometry,Language))
            processes.append(SaveNewGeometry)
            SaveNewGeometry.start()
            GeometrySaving()    
        OMG2LabelFrame.grid(column = 0, row = 5, sticky = NW, columnspan = 2)
                
        GeometryNameVariable = StringVar()
        GeometryURIVariable = StringVar()
        GeometryLocation = StringVar()
        GeometryDescription = StringVar()
        GeometryDatatypeVariable = StringVar()
        GeometryFormatPropertyVariable = StringVar()
        PartofGeometryVariable = StringVar()
        PartofIDVariable = StringVar()
        PartofPropertyVariable = StringVar()

        ReferencedContentVariable = IntVar()
        ReferencedContentNameVariable = StringVar()
        ReferencedContentURIVariable = StringVar()
        ReferencedContentLocationVariable = StringVar()
        ReferencedContentFormatPropertyVariable = StringVar()
        ReferencedDescription= StringVar()
        ReferencedContentDatatypeVariable = StringVar()
        SubjectURIVariable = StringVar()

        #GeometryName
        def GeometryName(*args):
            GeometryURI = GeometriesSettings[1] + quote(GeometryNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
            GeometryURIVariable.set(GeometryURI)

            ReferencedContentName = GeometryNameVariable.get() + "_ReferencedContent"
            ReferencedContentNameVariable.set(ReferencedContentName)

            ReferencedContentURI = GeometriesSettings[1] + quote(ReferencedContentName + datetime.now().strftime('%H%M%d%m%Y'))
            ReferencedContentURIVariable.set(ReferencedContentURI)
                   
        GeometryNameLabel = Label(OMG2LabelFrame, text = "Geometry:")
        GeometryNameLabel.grid(column = 0, row =0, sticky = NW)

        GeometryNameEntry = Entry(OMG2LabelFrame, textvariable = GeometryNameVariable, width = 50)
        GeometryNameEntry.grid(column =1, row = 0, sticky = NW)

        GeometryNameVariable.trace_add("write", GeometryName)

        GeometryURILabel = Label(OMG2LabelFrame, text = "Geometry URI:")
        GeometryURILabel.grid(column=0, row = 1, sticky = NW)

        GeometryURIEntry = Entry(OMG2LabelFrame, textvariable = GeometryURIVariable, width = 75)
        GeometryURIEntry.grid(column = 1, row = 1, sticky = NW)

        PartofFrame = Frame(OMG2LabelFrame)
        FileFrame = Frame(OMG2LabelFrame)
        def SaveAsSelected2(event):
            if GeometriesSaveAsCombobox.get() == 'Embeded':
                SaveAsinfo.set("""The entire geometry description will be saved into a literal 
                "..."^^xsd:string""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)
                
            if GeometriesSaveAsCombobox.get() == 'Local file':
                SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

            if GeometriesSaveAsCombobox.get() == 'Online file':
                SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

            if GeometriesSaveAsCombobox.get() == 'Part of':
                SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                    "...\"""")
                try:
                    FileFrame.grid_remove()
                except:
                    print("FAILED")
                PartofFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

        GeometriesSaveAsCombobox.bind("<<ComboboxSelected>>", SaveAsSelected2)

        PartofGeometryLabel = Label(PartofFrame, text = "Part of geometry:")
        PartofGeometryLabel.grid(column =0, row = 4, sticky = NW)

        PartofGeometryTreeview = Treeview(PartofFrame)
        PartofGeometryTreeview.grid(column =0, row = 5, sticky = NW, columnspan = 3)

        PartofGeometryTreeview["selectmode"] = BROWSE
        PartofGeometryTreeview["columns"] = ("one")
        PartofGeometryTreeview.column("#0", width = 400)
        PartofGeometryTreeview.heading("#0", text = "URI")
        PartofGeometryTreeview.column("one", width = 200)
        PartofGeometryTreeview.heading("one", text = "Name")

        SearchGeometries2(PartofGeometryTreeview)

        def SETPartofGeometryVariable(*args):
            PartofGeometryVariable.set(PartofGeometryTreeview.selection()[0])
                
        PartofGeometryTreeview.bind('<<TreeviewSelect>>', SETPartofGeometryVariable)

        IDLabel = Label(PartofFrame, text = "ID:")
        IDLabel.grid(column =0, row = 6, sticky = NW)

        IDEntry = Entry(PartofFrame, textvariable = PartofIDVariable, widt = 75)
        IDEntry.grid(column = 1, row = 6, sticky = NW)

        PartofPropertyLabel = Label(PartofFrame, text = "FOG property:")
        PartofPropertyLabel.grid(column =0, row = 7, sticky = NW)

        FOGproperties =[]

        for prop in SearchReferenceProperties():
            prop = prop.replace('https://w3id.org/fog#','fog:')
            FOGproperties.append(prop)

        PartofPropertyCombobox = Combobox(PartofFrame, width = 50, values = FOGproperties)
        PartofPropertyCombobox.grid(column = 1, row = 7, sticky = NW)

        def SETPartofPropertyVariable(event):
            PartofPropertyVariable.set(PartofPropertyCombobox.get())
            
        PartofPropertyCombobox.bind("<<ComboboxSelected>>", SETPartofPropertyVariable)

        FileFrame = Frame(OMG2LabelFrame)
        
        OMG3GeometryfileLabel = Label(FileFrame, text = "Geometryfile location:")
        OMG3GeometryfileLabel.grid(column =0, row =4, sticky = NW)
                    
        OMG3GeometryfileEntry = Entry(FileFrame,textvariable = GeometryLocation, width = 100)
        OMG3GeometryfileEntry.grid(column = 1, row = 4, sticky = NW)
                    
        OMG3BrowseGeometryButton = Button(FileFrame, text = "Browse", command = partial(BrowseGeometryfile,OMG3GeometryfileEntry,GeometryFormatPropertyVariable,ReferencedContentVariable,GeometryDescription,GeometryDatatypeVariable))
        OMG3BrowseGeometryButton.grid(column = 2, row = 4, sticky = NE)

        GeometryFormatMessage = Message(FileFrame, textvariable = GeometryFormatPropertyVariable, width = 300)
        GeometryFormatMessage.grid(column = 1, row = 5, sticky = NW, columnspan = 2)
        if ShowDescriptions == True:
            GeometryDescriptionMessage = Message(FileFrame, textvariable = GeometryDescription, width = 300)
            GeometryDescriptionMessage.grid(column = 1, row = 6, sticky = NW, columnspan = 2)
                
        def ReferencedContentChanged(*args):
            if ReferencedContentVariable.get() == 1:
                ReferencedContentNameEntry['state'] = 'enabled'
                ReferencedContentURIEntry['state'] = 'enabled'
                ReferencedContentLocationEntry['state'] = 'enabled'
                BrowseReferencedContentButton['state'] = 'enabled'
            if ReferencedContentVariable.get() == 0:
                ReferencedContentNameEntry['state'] = 'disabled'
                ReferencedContentURIEntry['state'] = 'disabled'
                ReferencedContentLocationEntry['state'] = 'disabled'
                BrowseReferencedContentButton['state'] = 'disabled'
                    
        ReferencedContentCheckbutton = Checkbutton(FileFrame, text = "Referenced Content", variable = ReferencedContentVariable, onvalue = 1, offvalue =0)
        ReferencedContentCheckbutton.grid(column = 0, row = 7, sticky = NW)

        ReferencedContentVariable.trace("w",ReferencedContentChanged)
                    
        referencedcontentframe = Frame(FileFrame)
        referencedcontentframe.grid(column =0, row = 8, columnspan = 3)

        ReferencedContentNameLabel = Label(referencedcontentframe, text = 'Referenced content:')
        ReferencedContentNameLabel.grid(column = 0, row = 0, sticky = NW)

        ReferencedContentNameEntry = Entry(referencedcontentframe, textvariable = ReferencedContentNameVariable, width = 50)
        ReferencedContentNameEntry.grid(column =1, row = 0, sticky = NW)

        ReferencedContentURILabel = Label(referencedcontentframe, text = 'Referenced content URI:')
        ReferencedContentURILabel.grid(column = 0, row =1, sticky = NW)

        ReferencedContentURIEntry = Entry(referencedcontentframe, textvariable = ReferencedContentURIVariable, width = 75)
        ReferencedContentURIEntry.grid(column =1, row = 1, sticky = NW)

        ReferencedContentLocationLabel = Label(referencedcontentframe, text = 'Location:')
        ReferencedContentLocationLabel.grid( column = 0, row = 2, sticky = NW)

        ReferencedContentLocationEntry = Entry(referencedcontentframe, textvariable = ReferencedContentLocationVariable, width = 100)
        ReferencedContentLocationEntry.grid(column =1, row = 2, sticky = NW)

        BrowseReferencedContentButton = Button(referencedcontentframe, text = 'Browse', command = partial(BrowseReferencedContent,ReferencedContentLocationEntry,ReferencedContentFormatPropertyVariable,ReferencedDescription,ReferencedContentDatatypeVariable), state = 'disabled')
        BrowseReferencedContentButton.grid(column = 2, row = 2, sticky = NW)
                    
        ReferencedContentFormatMessage = Message(referencedcontentframe, textvariable = ReferencedContentFormatPropertyVariable, width = 300)
        ReferencedContentFormatMessage.grid(column = 1, row = 3, sticky = NW)
                    
        if ShowDescriptions==True:
            ReferencedContentDescriptionMessage = Message(referencedcontentframe, textvariable = ReferencedDescription, width = 300)
            ReferencedContentDescriptionMessage.grid(column = 1, row = 4, sticky = NW, columnspan = 2)
                    
        if ReferencedContentVariable.get() == 0:
            ReferencedContentNameEntry['state'] = 'disabled'
            ReferencedContentURIEntry['state'] = 'disabled'
            ReferencedContentLocationEntry['state'] = 'disabled'
            BrowseReferencedContentButton['state'] = 'disabled'
        if ReferencedContentVariable.get() == 1:
            ReferencedContentNameEntry['state'] = 'enabled'
            ReferencedContentURIEntry['state'] = 'enabled'
            ReferencedContentLocationEntry['state'] = 'enabled'
            BrowseReferencedContentButton['state'] = 'enabled'
                    
        if filelocation:
            Geometryfile = filelocation
            OMG3GeometryfileEntry.insert(0, Geometryfile)
            OMG3GeometryfileEntry['state'] = 'disabled'
            OMG3BrowseGeometryButton['state'] = 'disabled'
            for Geometryformat in SupportedGeometryFormats:
                if Geometryfile.endswith(Geometryformat[2]):
                    GeometryFormatPropertyVariable.set(DetermineFormat(Geometryfile,SupportedGeometryFormats))
                    GeometryDatatypeVariable.set(Geometryformat[6])
                    if int(Geometryformat[3]) == 1:
                        ReferencedContentVariable.set(1)
                        for refcontentformat in SupportedGeometryFormats:
                            if Geometryformat[4] == refcontentformat[0]:
                                Refcontentfile = Geometryfile.replace(Geometryformat[2], refcontentformat[2])
                                if os.path.exists(Refcontentfile):
                                    ReferencedContentLocationVariable.set(Refcontentfile)
                                    ReferencedContentFormatPropertyVariable.set(DetermineFormat(Refcontentfile,SupportedGeometryFormats))
                                    ReferencedDescription.set(READfile(Refcontentfile))
                                    ReferencedContentDatatypeVariable.set(refcontentformat[6])
                    GeometryDescription.set(READfile(Geometryfile))
        if objectid:
            GeometryNameVariable.set(objectid)
            PartofIDVariable.set(objectid)
        
        if Origin:
            if Origin == 'gom:Rhinoceros_v6':
                PartofPropertyVariable.set('fog:hasRhinoId-object')
                PartofPropertyCombobox.set('fog:hasRhinoId-object')

        if GeometriesSaveAsCombobox.get() == 'Part of':
            PartofFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)
            
        else:
            FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)
        #Possible Links
        def LinkObjects():
            for f in tempFrames:
                f.destroy()
                   
            def OMG2ElementSelected(event):
                subject = ElementsTreeview.selection()[0]
                label = ElementsTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG2
                SaveButton['state'] = 'enabled'

            def OMG2ZoneSelected(event):
                subject = ZonesTreeview.selection()[0]
                label = ZonesTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG2
                SaveButton['state'] = 'enabled'

            def OMG2DamageSelected(event):
                subject = DamagesTreeview.selection()[0]
                label = DamagesTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG2
                SaveButton['state'] = 'enabled'
                
            #print("link possible  objects")
                    
            #Can be linked to 
           
            LinkToFrame2 = Frame(LinkToFrame)
            LinkToFrame2.grid(column = 0, row = 0, sticky = NW)
            tempFrames.append(LinkToFrame2)
            ##Zone
                    
            ListZones = SearchZones()
                    
            LinkOMG2ZoneLabel = Label(LinkToFrame2, text = "Zones")
            LinkOMG2ZoneLabel.grid(column = 0, row = 2, sticky = NW)

            ZonesTreeview = Treeview(LinkToFrame2)
            ZonesTreeview.grid(column = 0, row = 3, sticky = NW)
            ZonesTreeview["selectmode"] = BROWSE
            ZonesTreeview["columns"] = ("one")
            ZonesTreeview.column("#0", width = 400)
            ZonesTreeview.heading("#0", text = "URI")
            ZonesTreeview.column("one", width = 200)
            ZonesTreeview.heading("one", text = "Name")

            for zone in ListZones:
                ZonesTreeview.insert("",END,zone[0],text = zone[0], values =(zone[1],))

            ZonesTreeview.bind('<<TreeviewSelect>>', OMG2ZoneSelected)
                    
            #Element
                    
            ListElements = SearchElements()

            LinkOMG2ElementLabel = Label(LinkToFrame2, text = "Elements")
            LinkOMG2ElementLabel.grid(column = 0, row = 4, sticky = NW)
                    
            ElementsTreeview = Treeview(LinkToFrame2)
            ElementsTreeview.grid(column = 0, row = 5, sticky = NW)
            ElementsTreeview["selectmode"] = BROWSE
            ElementsTreeview["columns"] = ("one")
            ElementsTreeview.column("#0", width = 400)
            ElementsTreeview.heading("#0", text = "URI")
            ElementsTreeview.column("one", width = 200)
            ElementsTreeview.heading("one", text = "Name")

            for element in ListElements:
                ElementsTreeview.insert("",END,element[0],text = element[0], values =(element[1],))

            ElementsTreeview.bind('<<TreeviewSelect>>', OMG2ElementSelected)
            #Damage
            ListDamages = SearchDamages()

            LinkOMG2DamageLabel = Label(LinkToFrame2, text = "Damages")
            LinkOMG2DamageLabel.grid(column = 0, row = 6, sticky = NW)
                    
            DamagesTreeview = Treeview(LinkToFrame2)
            DamagesTreeview.grid(column = 0, row = 7, sticky = NW)
            DamagesTreeview["selectmode"] = BROWSE
            DamagesTreeview["columns"] = ("one")
            DamagesTreeview.column("#0", width = 400)
            DamagesTreeview.heading("#0", text = "URI")
            DamagesTreeview.column("one", width = 200)
            DamagesTreeview.heading("one", text = "Name")

            for damage in ListDamages:
                DamagesTreeview.insert("",END,damage[0],text = damage[0], values =(damage[1],))

            DamagesTreeview.bind('<<TreeviewSelect>>', OMG2DamageSelected)

        LinkObjects()

        #Metadata
        CreatedByVariable = StringVar()
        CreatedInVariable = StringVar()
        CoordinateSystemVariable = StringVar()
        CoordinateSystemVariable.set(DefaultCS)
        GeometryTypeVariable = StringVar()

        metadatalabelframe.grid(column = 0, row = 6, sticky = NW, columnspan = 2)

        CreatorLabel = Label(metadatalabelframe, text = 'Creator: ')
        CreatorLabel.grid(column =0, row =0, sticky = NW)

        CreatorEntry = Entry(metadatalabelframe, textvariable = CreatedByVariable, width = 50)
        CreatorEntry.grid(column =1, row = 0, sticky = NW)

        GeometryTypeLabel = Label(metadatalabelframe, text = 'Type of geometry:')
        GeometryTypeLabel.grid(column = 0, row = 1, sticky = NW)

        GeometryTypesTreeview = Treeview(metadatalabelframe)
        GeometryTypesTreeview.grid(column = 0, row = 2, sticky = NW, columnspan = 3)

        GeometryTypesTreeview["selectmode"] = BROWSE
        GeometryTypesTreeview["columns"] = ("one")
        GeometryTypesTreeview.column("#0", width = 400)
        GeometryTypesTreeview.heading("#0", text = "URI")
        GeometryTypesTreeview.column("one", width = 200)
        GeometryTypesTreeview.heading("one", text = "Name")

        ListGeometryTypes = SearchGeometryTypes()
        for geometrytype in ListGeometryTypes:
            GeometryTypesTreeview.insert("",END,geometrytype[0],text = geometrytype[0], values = (geometrytype[1],))
                
        def SETGeometryTypeVariable(*args):
            GeometryTypeVariable.set(GeometryTypesTreeview.selection()[0])
                
        GeometryTypesTreeview.bind('<<TreeviewSelect>>', SETGeometryTypeVariable)

        GeometryCreatedInLabel = Label(metadatalabelframe, text = 'Created in:')
        GeometryCreatedInLabel.grid(column = 0, row = 3, sticky = NW)

        GeometryCreatedInTreeview = Treeview(metadatalabelframe)
        GeometryCreatedInTreeview.grid(column = 0, row = 4, sticky = NW, columnspan = 3)

        GeometryCreatedInTreeview["selectmode"] = BROWSE
        GeometryCreatedInTreeview["columns"] = ("one")
        GeometryCreatedInTreeview.column("#0", width = 400)
        GeometryCreatedInTreeview.heading("#0", text = "URI")
        GeometryCreatedInTreeview.column("one", width = 200)
        GeometryCreatedInTreeview.heading("one", text = "Name")

        ListGeometryApplications = SearchGeometryApplications()
        for geometryapplication in ListGeometryApplications:
            GeometryCreatedInTreeview.insert("",END,geometryapplication[0],text = geometryapplication[0], values = (geometryapplication[1],))
        
        if Origin:
            createdIn = Origin
            createdIn = createdIn.replace('gom:', 'https://w3id.org/gom#')
            CreatedInVariable.set(createdIn)
            GeometryCreatedInTreeview.selection_set(CreatedInVariable.get())

                
        def SETGeometryCreatedInVariable(*args):
            CreatedInVariable.set(GeometryCreatedInTreeview.selection()[0])
                
        GeometryCreatedInTreeview.bind('<<TreeviewSelect>>', SETGeometryCreatedInVariable)

        CoordinatesLabel = Label(metadatalabelframe, text = "Coordinate systems:")
        CoordinatesLabel.grid(column =0, row = 6, sticky = NW)

        CoordinatesTreeview = Treeview(metadatalabelframe)
        CoordinatesTreeview.grid(column = 0, row = 7, sticky = NW, columnspan = 3)

        CoordinatesTreeview["selectmode"] = BROWSE
        CoordinatesTreeview["columns"] = ("one")
        CoordinatesTreeview.column("#0", width = 400)
        CoordinatesTreeview.heading("#0", text = "URI")
        CoordinatesTreeview.column("one", width = 200)
        CoordinatesTreeview.heading("one", text = "Name")

        ListCoordinates = SearchCoordinates()
        for CS in ListCoordinates:
            CoordinatesTreeview.insert("",END,CS[0],text = CS[0], values = (CS[1],))
                
        def SETCoordinateSystemVariable(*args):
            CoordinateSystemVariable.set(CoordinatesTreeview.selection()[0])
                
        CoordinatesTreeview.bind('<<TreeviewSelect>>', SETCoordinateSystemVariable)

        GeometryContextsLabel = Label(metadatalabelframe, text = "Geometry contexts:")
        GeometryContextsLabel.grid(column =0, row = 8, sticky = NW)

        GeometryContextsTreeview = Treeview(metadatalabelframe)
        GeometryContextsTreeview.grid(column = 0, row = 9, sticky = NW, columnspan = 3)

        GeometryContextsTreeview["selectmode"] = EXTENDED
        GeometryContextsTreeview["columns"] = ("one")
        GeometryContextsTreeview.column("#0", width = 400)
        GeometryContextsTreeview.heading("#0", text = "URI")
        GeometryContextsTreeview.column("one", width = 200)
        GeometryContextsTreeview.heading("one", text = "Name")

        ListGeometryContexts = SearchGeometryContexts()
        for geometrycontext in ListGeometryContexts:
            GeometryContextsTreeview.insert("",END, geometrycontext[0], text = geometrycontext[0], values = (geometrycontext[1],))
        
        derivedfromLabel = Label(metadatalabelframe, text = "Derived from:")
        derivedfromLabel.grid(column =0, row = 10, sticky = NW)

        DerivedfromTreeview = Treeview(metadatalabelframe)
        DerivedfromTreeview.grid(column =0, row = 11, sticky = NW, columnspan = 3)

        DerivedfromTreeview["selectmode"] = EXTENDED
        DerivedfromTreeview["columns"] = ("one")
        DerivedfromTreeview.column("#0", width = 400)
        DerivedfromTreeview.heading("#0", text = "URI")
        DerivedfromTreeview.column("one", width = 200)
        DerivedfromTreeview.heading("one", text = "Name")

        SearchGeometries2(DerivedfromTreeview)

    def OMGLevel3Frame():
        
        def GetOMG3():
            graph = GeometriesSettings[0]
            subject = SubjectURIVariable.get()
            geometry = GeometryNameVariable.get()
            geometryURI = GeometryURIVariable.get()
            geometrystate = GeometrystateNameVariable.get()
            geometrystateURI = GeometrystateURIVariable.get()
            geometryformat = GeometryFormatPropertyVariable.get()

            if GeometriesSaveAsCombobox.get() == 'Embeded':
                geometryLiteral = GeometryDescription.get()
                geometryliteraltype = GeometryDatatypeVariable.get()
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedDescription.get()
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = ReferencedContentDatatypeVariable.get()
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None

                if DeleteGeometryFiles == True:
                    os.remove(GeometryLocation.get())
                    if ReferencedContentVariable.get() == 1:
                        os.remove(ReferencedContentLocationVariable.get())

            if GeometriesSaveAsCombobox.get() == 'Local file':
                geometryLiteral = GeometryLocation.get()
                geometryLiteral = geometryLiteral.replace("\\","\\\\")
                geometryLiteral = str(geometryLiteral)
                geometryliteraltype = "anyURI"
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedContentLocationVariable.get()
                    referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                    referencedcontentLiteral = str(referencedcontentLiteral)
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = "anyURI"
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None


            if GeometriesSaveAsCombobox.get() == 'Online file':
                geometryLiteral = GeometryLocation.get()
                geometryLiteral = geometryLiteral.replace("\\","\\\\")
                geometryLiteral = str(geometryLiteral)
                geometryliteraltype = "anyURI"
                if ReferencedContentVariable.get() == 1:
                    referencedcontent = ReferencedContentNameVariable.get()
                    referencedcontentURI = ReferencedContentURIVariable.get()
                    referencedcontentLiteral = ReferencedContentLocationVariable.get()
                    referencedcontentLiteral =referencedcontentLiteral.replace("\\","\\\\")
                    referencedcontentLiteral = str(referencedcontentLiteral)
                    referencedcontentformat = ReferencedContentFormatPropertyVariable.get()
                    referencedcontentliteraltype = "anyURI"
                else:
                    referencedcontent = None
                    referencedcontentURI = None
                    referencedcontentLiteral = None
                    referencedcontentformat = None
                    referencedcontentliteraltype = None
            if GeometriesSaveAsCombobox.get() == 'Part of':
                geometryLiteral = PartofIDVariable.get()
                geometryliteraltype = "string"
                geometryformat = PartofPropertyVariable.get()
                PartOf = PartofGeometryTreeview.selection()[0]
                referencedcontent = None
                referencedcontentURI = None
                referencedcontentLiteral = None
                referencedcontentformat = None
                referencedcontentliteraltype = None
            else: 
                PartOf = None


            creator = CreatedByVariable.get()
            geometrytype = GeometryTypeVariable.get()
            createdin = CreatedInVariable.get()
            coordinatesystem = CoordinateSystemVariable.get()
            geometrycontext =[]
            for gc in GeometryContextsTreeview2.selection():
                geometrycontext.append(gc)
            DerivedFromGeometry =[]
            for derivedgeometry in DerivedfromTreeview.selection():
                DerivedFromGeometry.append(derivedgeometry[0])

            SaveNewGeometry = Process(target = OMG3ToGraph, args = (SaveToDefaultGraph, UrlPost, headersPost,graph,subject, geometry,geometryURI, geometrystate, geometrystateURI, geometryformat,geometryLiteral, geometryliteraltype, PartOf, referencedcontent, referencedcontentURI, referencedcontentformat, referencedcontentLiteral, referencedcontentliteraltype, creator, geometrytype, createdin, coordinatesystem, geometrycontext,DerivedFromGeometry,Language))
            processes.append(SaveNewGeometry)
            SaveNewGeometry.start()
            GeometrySaving() 
        OMG3LabelFrame.grid(column = 0, row = 5, sticky = NW, columnspan = 2)
                
        GeometryNameVariable = StringVar()
        GeometryURIVariable = StringVar()
        GeometrystateNameVariable = StringVar()
        GeometrystateURIVariable = StringVar()
        GeometryLocation = StringVar()
        GeometryDescription = StringVar()
        GeometryDatatypeVariable = StringVar()
        GeometryFormatPropertyVariable = StringVar()
        ReferencedContentVariable = IntVar()
        ReferencedContentNameVariable = StringVar()
        ReferencedContentURIVariable = StringVar()
        ReferencedContentLocationVariable = StringVar()
        ReferencedContentFormatPropertyVariable = StringVar()
        ReferencedDescription= StringVar()
        ReferencedContentDatatypeVariable = StringVar()
        SubjectURIVariable = StringVar()
        PartofGeometryVariable = StringVar()
        PartofIDVariable = StringVar()
        PartofPropertyVariable = StringVar()
        #GeometryName
        def GeometryName(*args):
            GeometryURI = GeometriesSettings[1] + quote(GeometryNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
            GeometryURIVariable.set(GeometryURI)

            GeometrystateName = GeometryNameVariable.get() + "_GeometryState"
            GeometrystateNameVariable.set(GeometrystateName)

            GeometrystateURI = GeometriesSettings[1] + quote(GeometrystateName + datetime.now().strftime('%H%M%d%m%Y'))
            GeometrystateURIVariable.set(GeometrystateURI)

            ReferencedContentName = GeometrystateNameVariable.get() + "_ReferencedContent"
            ReferencedContentNameVariable.set(ReferencedContentName)

            ReferencedContentURI = GeometriesSettings[1] + quote(ReferencedContentName + datetime.now().strftime('%H%M%d%m%Y'))
            ReferencedContentURIVariable.set(ReferencedContentURI)
                   
        GeometryNameLabel = Label(OMG3LabelFrame, text = "Geometry:")
        GeometryNameLabel.grid(column = 0, row =0, sticky = NW)

        GeometryNameEntry = Entry(OMG3LabelFrame, textvariable = GeometryNameVariable, width = 50)
        GeometryNameEntry.grid(column =1, row = 0, sticky = NW)

        GeometryNameVariable.trace_add("write", GeometryName)

        GeometryURILabel = Label(OMG3LabelFrame, text = "Geometry URI:")
        GeometryURILabel.grid(column=0, row = 1, sticky = NW)

        GeometryURIEntry = Entry(OMG3LabelFrame, textvariable = GeometryURIVariable, width = 75)
        GeometryURIEntry.grid(column = 1, row = 1, sticky = NW)

        GeometrystateNameLabel = Label(OMG3LabelFrame, text = "Geometrystate:")
        GeometrystateNameLabel.grid(column =0, row = 2, sticky = NW)

        GeometrystateNameEntry = Entry(OMG3LabelFrame, textvariable = GeometrystateNameVariable, width = 50)
        GeometrystateNameEntry.grid(column = 1, row = 2, sticky = NW)

        GeometrystateURILabel = Label(OMG3LabelFrame, text ='Geometrystate URI: ')
        GeometrystateURILabel.grid(column = 0, row = 3, sticky = NW)

        GeometrystateURIEntry = Entry(OMG3LabelFrame, textvariable = GeometrystateURIVariable, width = 75)
        GeometrystateURIEntry.grid(column = 1, row = 3, sticky = NW)

        PartofFrame = Frame(OMG3LabelFrame)
        FileFrame = Frame(OMG3LabelFrame)
        
        def SaveAsSelected2(event):
            if GeometriesSaveAsCombobox.get() == 'Embeded':
                SaveAsinfo.set("""The entire geometry description will be saved into a literal 
                "..."^^xsd:string""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)
                
            if GeometriesSaveAsCombobox.get() == 'Local file':
                SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

            if GeometriesSaveAsCombobox.get() == 'Online file':
                SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
                try:
                    PartofFrame.grid_remove()
                except:
                    print("FAILED")
                FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

            if GeometriesSaveAsCombobox.get() == 'Part of':
                SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                    "...\"""")
                try:
                    FileFrame.grid_remove()
                except:
                    print("FAILED")
                PartofFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

        GeometriesSaveAsCombobox.bind("<<ComboboxSelected>>", SaveAsSelected2)

        PartofGeometryLabel = Label(PartofFrame, text = "Part of geometry:")
        PartofGeometryLabel.grid(column =0, row = 4, sticky = NW)

        PartofGeometryTreeview = Treeview(PartofFrame)
        PartofGeometryTreeview.grid(column =0, row = 5, sticky = NW, columnspan = 3)

        PartofGeometryTreeview["selectmode"] = BROWSE
        PartofGeometryTreeview["columns"] = ("one")
        PartofGeometryTreeview.column("#0", width = 400)
        PartofGeometryTreeview.heading("#0", text = "URI")
        PartofGeometryTreeview.column("one", width = 200)
        PartofGeometryTreeview.heading("one", text = "Name")

        SearchGeometries2(PartofGeometryTreeview)

        def SETPartofGeometryVariable(*args):
            PartofGeometryVariable.set(PartofGeometryTreeview.selection()[0])
                
        PartofGeometryTreeview.bind('<<TreeviewSelect>>', SETPartofGeometryVariable)

        IDLabel = Label(PartofFrame, text = "ID:")
        IDLabel.grid(column =0, row = 6, sticky = NW)

        IDEntry = Entry(PartofFrame, textvariable = PartofIDVariable, widt = 75)
        IDEntry.grid(column = 1, row = 6, sticky = NW)

        PartofPropertyLabel = Label(PartofFrame, text = "FOG property:")
        PartofPropertyLabel.grid(column =0, row = 7, sticky = NW)

        FOGproperties =[]

        for prop in SearchReferenceProperties():
            print(prop)
            prop = prop.replace('https://w3id.org/fog#','fog:')
            print(prop)
            FOGproperties.append(prop)

        PartofPropertyCombobox = Combobox(PartofFrame, width = 50, values = FOGproperties)
        PartofPropertyCombobox.grid(column = 1, row = 7, sticky = NW)

        def SETPartofPropertyVariable(event):
            PartofPropertyVariable.set(PartofPropertyCombobox.get())
            
        PartofPropertyCombobox.bind("<<ComboboxSelected>>", SETPartofPropertyVariable)

        OMG3GeometryfileLabel = Label(FileFrame, text = "Geometryfile location:")
        OMG3GeometryfileLabel.grid(column =0, row =4, sticky = NW)
                    
        OMG3GeometryfileEntry = Entry(FileFrame,textvariable = GeometryLocation, width = 100)
        OMG3GeometryfileEntry.grid(column = 1, row = 4, sticky = NW)
                    
        OMG3BrowseGeometryButton = Button(FileFrame, text = "Browse", command = partial(BrowseGeometryfile,OMG3GeometryfileEntry,GeometryFormatPropertyVariable,ReferencedContentVariable,GeometryDescription,GeometryDatatypeVariable))
        OMG3BrowseGeometryButton.grid(column = 2, row = 4, sticky = NE)

        GeometryFormatMessage = Message(FileFrame, textvariable = GeometryFormatPropertyVariable, width = 300)
        GeometryFormatMessage.grid(column = 1, row = 5, sticky = NW, columnspan = 2)
        if ShowDescriptions==True:
            GeometryDescriptionMessage = Message(FileFrame, textvariable = GeometryDescription, width = 300)
            GeometryDescriptionMessage.grid(column = 1, row = 6, sticky = NW, columnspan = 2)
                
        def ReferencedContentChanged(*args):
            if ReferencedContentVariable.get() == 1:
                ReferencedContentNameEntry['state'] = 'enabled'
                ReferencedContentURIEntry['state'] = 'enabled'
                ReferencedContentLocationEntry['state'] = 'enabled'
                BrowseReferencedContentButton['state'] = 'enabled'
            if ReferencedContentVariable.get() == 0:
                ReferencedContentNameEntry['state'] = 'disabled'
                ReferencedContentURIEntry['state'] = 'disabled'
                ReferencedContentLocationEntry['state'] = 'disabled'
                BrowseReferencedContentButton['state'] = 'disabled'
                    
        ReferencedContentCheckbutton = Checkbutton(FileFrame, text = "Referenced Content", variable = ReferencedContentVariable, onvalue = 1, offvalue =0)
        ReferencedContentCheckbutton.grid(column = 0, row = 7, sticky = NW)

        ReferencedContentVariable.trace("w",ReferencedContentChanged)
                    
        referencedcontentframe = Frame(FileFrame)
        referencedcontentframe.grid(column =0, row = 8, columnspan = 3)

        ReferencedContentNameLabel = Label(referencedcontentframe, text = 'Referenced content:')
        ReferencedContentNameLabel.grid(column = 0, row = 0, sticky = NW)

        ReferencedContentNameEntry = Entry(referencedcontentframe, textvariable = ReferencedContentNameVariable, width = 50)
        ReferencedContentNameEntry.grid(column =1, row = 0, sticky = NW)

        ReferencedContentURILabel = Label(referencedcontentframe, text = 'Referenced content URI:')
        ReferencedContentURILabel.grid(column = 0, row =1, sticky = NW)

        ReferencedContentURIEntry = Entry(referencedcontentframe, textvariable = ReferencedContentURIVariable, width = 75)
        ReferencedContentURIEntry.grid(column =1, row = 1, sticky = NW)

        ReferencedContentLocationLabel = Label(referencedcontentframe, text = 'Location:')
        ReferencedContentLocationLabel.grid( column = 0, row = 2, sticky = NW)

        ReferencedContentLocationEntry = Entry(referencedcontentframe, textvariable = ReferencedContentLocationVariable, width = 100)
        ReferencedContentLocationEntry.grid(column =1, row = 2, sticky = NW)

        BrowseReferencedContentButton = Button(referencedcontentframe, text = 'Browse', command = partial(BrowseReferencedContent,ReferencedContentLocationEntry,ReferencedContentFormatPropertyVariable,ReferencedDescription,ReferencedContentDatatypeVariable), state = 'disabled')
        BrowseReferencedContentButton.grid(column = 2, row = 2, sticky = NW)
                    
        ReferencedContentFormatMessage = Message(referencedcontentframe, textvariable = ReferencedContentFormatPropertyVariable, width = 300)
        ReferencedContentFormatMessage.grid(column = 1, row = 3, sticky = NW)
                    
        if ShowDescriptions==True:
            ReferencedContentDescriptionMessage = Message(referencedcontentframe, textvariable = ReferencedDescription, width = 300)
            ReferencedContentDescriptionMessage.grid(column = 1, row = 4, sticky = NW, columnspan = 2)
                    
        if ReferencedContentVariable.get() == 0:
            ReferencedContentNameEntry['state'] = 'disabled'
            ReferencedContentURIEntry['state'] = 'disabled'
            ReferencedContentLocationEntry['state'] = 'disabled'
            BrowseReferencedContentButton['state'] = 'disabled'
        if ReferencedContentVariable.get() == 1:
            ReferencedContentNameEntry['state'] = 'enabled'
            ReferencedContentURIEntry['state'] = 'enabled'
            ReferencedContentLocationEntry['state'] = 'enabled'
            BrowseReferencedContentButton['state'] = 'enabled'
                  
        if filelocation:
            Geometryfile = filelocation
            print(Geometryfile)
            OMG3GeometryfileEntry.insert(0, Geometryfile)
            OMG3GeometryfileEntry['state'] = 'disabled'
            OMG3BrowseGeometryButton['state'] = 'disabled'
            for Geometryformat in SupportedGeometryFormats:
                if Geometryfile.endswith(Geometryformat[2]):
                    GeometryFormatPropertyVariable.set(DetermineFormat(Geometryfile,SupportedGeometryFormats))
                    GeometryDatatypeVariable.set(Geometryformat[6])
                    if int(Geometryformat[3]) == 1:
                        ReferencedContentVariable.set(1)
                        for refcontentformat in SupportedGeometryFormats:
                            if Geometryformat[4] == refcontentformat[0]:
                                Refcontentfile = Geometryfile.replace(Geometryformat[2], refcontentformat[2])
                                if os.path.exists(Refcontentfile):
                                    ReferencedContentLocationVariable.set(Refcontentfile)
                                    ReferencedContentFormatPropertyVariable.set(DetermineFormat(Refcontentfile,SupportedGeometryFormats))
                                    ReferencedDescription.set(READfile(Refcontentfile))
                                    ReferencedContentDatatypeVariable.set(refcontentformat[6])
                    GeometryDescription.set(READfile(Geometryfile))
        
        if objectid:
            GeometryNameVariable.set(objectid)
            PartofIDVariable.set(objectid)
        
        if Origin:
            if Origin == 'gom:Rhinoceros_v6':
                PartofPropertyVariable.set('fog:hasRhinoId-object')
                PartofPropertyCombobox.set('fog:hasRhinoId-object')

        if GeometriesSaveAsCombobox.get() == 'Part of':
            PartofFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)  
        else:
            FileFrame.grid(column =0, row = 4, columnspan = 3, sticky = NW)

       

        #Metadata
        CreatedByVariable = StringVar()
        CreatedInVariable = StringVar()
        CoordinateSystemVariable = StringVar()
        GeometryTypeVariable = StringVar()
        CoordinateSystemVariable.set(DefaultCS)

        metadatalabelframe.grid(column = 0, row = 6, sticky = NW, columnspan = 2)

        CreatorLabel = Label(metadatalabelframe, text = 'Creator: ')
        CreatorLabel.grid(column =0, row =0, sticky = NW)

        CreatorEntry = Entry(metadatalabelframe, textvariable = CreatedByVariable, width = 50)
        CreatorEntry.grid(column =1, row = 0, sticky = NW)

        GeometryTypeLabel = Label(metadatalabelframe, text = 'Type of geometry:')
        GeometryTypeLabel.grid(column = 0, row = 1, sticky = NW)

        GeometryTypesTreeview = Treeview(metadatalabelframe)
        GeometryTypesTreeview.grid(column = 0, row = 2, sticky = NW, columnspan = 3)

        GeometryTypesTreeview["selectmode"] = BROWSE
        GeometryTypesTreeview["columns"] = ("one")
        GeometryTypesTreeview.column("#0", width = 400)
        GeometryTypesTreeview.heading("#0", text = "URI")
        GeometryTypesTreeview.column("one", width = 200)
        GeometryTypesTreeview.heading("one", text = "Name")

        ListGeometryTypes = SearchGeometryTypes()
        for geometrytype in ListGeometryTypes:
            GeometryTypesTreeview.insert("",END,geometrytype[0],text = geometrytype[0], values = (geometrytype[1],))
                
        def SETGeometryTypeVariable(*args):
            GeometryTypeVariable.set(GeometryTypesTreeview.selection()[0])
                
        GeometryTypesTreeview.bind('<<TreeviewSelect>>', SETGeometryTypeVariable)

        GeometryCreatedInLabel = Label(metadatalabelframe, text = 'Created in:')
        GeometryCreatedInLabel.grid(column = 0, row = 3, sticky = NW)

        GeometryCreatedInTreeview = Treeview(metadatalabelframe)
        GeometryCreatedInTreeview.grid(column = 0, row = 4, sticky = NW, columnspan = 3)

        GeometryCreatedInTreeview["selectmode"] = BROWSE
        GeometryCreatedInTreeview["columns"] = ("one")
        GeometryCreatedInTreeview.column("#0", width = 400)
        GeometryCreatedInTreeview.heading("#0", text = "URI")
        GeometryCreatedInTreeview.column("one", width = 200)
        GeometryCreatedInTreeview.heading("one", text = "Name")

        ListGeometryApplications = SearchGeometryApplications()
        for geometryapplication in ListGeometryApplications:
            GeometryCreatedInTreeview.insert("",END,geometryapplication[0],text = geometryapplication[0], values = (geometryapplication[1],))

        if Origin:
            createdIn = Origin
            createdIn = createdIn.replace('gom:', 'https://w3id.org/gom#')
            CreatedInVariable.set(createdIn)
            GeometryCreatedInTreeview.selection_set(CreatedInVariable.get())
        
        def SETGeometryCreatedInVariable(*args):
            CreatedInVariable.set(GeometryCreatedInTreeview.selection()[0])
                
        GeometryCreatedInTreeview.bind('<<TreeviewSelect>>', SETGeometryCreatedInVariable)

        CoordinatesLabel = Label(metadatalabelframe, text = "Coordinate systems:")
        CoordinatesLabel.grid(column =0, row = 6, sticky = NW)

        CoordinatesTreeview = Treeview(metadatalabelframe)
        CoordinatesTreeview.grid(column = 0, row = 7, sticky = NW, columnspan = 3)

        CoordinatesTreeview["selectmode"] = BROWSE
        CoordinatesTreeview["columns"] = ("one")
        CoordinatesTreeview.column("#0", width = 400)
        CoordinatesTreeview.heading("#0", text = "URI")
        CoordinatesTreeview.column("one", width = 200)
        CoordinatesTreeview.heading("one", text = "Name")


        ListCoordinates = SearchCoordinates()
        for CS in ListCoordinates:
            CoordinatesTreeview.insert("",END,CS[0],text = CS[0], values = (CS[1],))
        
        CoordinatesTreeview.selection_set(CoordinateSystemVariable.get())
                
        def SETCoordinateSystemVariable(*args):
            CoordinateSystemVariable.set(CoordinatesTreeview.selection()[0])
                
        CoordinatesTreeview.bind('<<TreeviewSelect>>', SETCoordinateSystemVariable)

        GeometryContextsLabel = Label(metadatalabelframe, text = "Geometry contexts:")
        GeometryContextsLabel.grid(column =0, row = 8, sticky = NW)

        GeometryContextsTreeview2 = Treeview(metadatalabelframe)
        GeometryContextsTreeview2.grid(column = 0, row = 9, sticky = NW, columnspan = 3)

        GeometryContextsTreeview2["selectmode"] = EXTENDED
        GeometryContextsTreeview2["columns"] = ("one")
        GeometryContextsTreeview2.column("#0", width = 400)
        GeometryContextsTreeview2.heading("#0", text = "URI")
        GeometryContextsTreeview2.column("one", width = 200)
        GeometryContextsTreeview2.heading("one", text = "Name")

        ListGeometryContexts = SearchGeometryContexts()
        for geometrycontext in ListGeometryContexts:
            GeometryContextsTreeview2.insert("",END, geometrycontext[0], text = geometrycontext[0], values = (geometrycontext[1],))
        
        derivedfromLabel = Label(metadatalabelframe, text = "Derived from:")
        derivedfromLabel.grid(column =0, row = 10, sticky = NW)

        DerivedfromTreeview = Treeview(metadatalabelframe)
        DerivedfromTreeview.grid(column =0, row = 11, sticky = NW, columnspan = 3)

        DerivedfromTreeview["selectmode"] = EXTENDED
        DerivedfromTreeview["columns"] = ("one")
        DerivedfromTreeview.column("#0", width = 400)
        DerivedfromTreeview.heading("#0", text = "URI")
        DerivedfromTreeview.column("one", width = 200)
        DerivedfromTreeview.heading("one", text = "Name")

        SearchGeometries2(DerivedfromTreeview)

         #Possible Links
        def LinkObjects():
            for f in tempFrames:
                f.destroy()
            def OMG3GeometrySelected(event):
                        
                def LinkedObject(geometry):
                    LinkedObject = SearchLinkedObject(geometry[0])
                            
                    LinkedObjectURIVariable = StringVar()
                    LinkedObjectURIVariable.set(LinkedObject[0])
                    LinkedObjectNameVariable = StringVar()
                    LinkedObjectNameVariable.set(LinkedObject[1])

                    LinkedObjectInfoLabel = Label(LinkedToObjectFrame, text = 'This geometry is already linked to an object in the graph')
                    LinkedObjectInfoLabel.grid(column = 0, row =0, sticky = NW)

                    GeometryLinkedToLabel = Label(LinkedToObjectFrame, text = "URI:")
                    GeometryLinkedToLabel.grid(column =0, row = 1, sticky = NW)
                    GeometryLinkedToMessage = Message(LinkedToObjectFrame, textvariable = LinkedObjectURIVariable, width = 300)
                    GeometryLinkedToMessage.grid(column = 1, row = 1, sticky = NW)

                    GeometryLinkedToLabel = Label(LinkedToObjectFrame, text = "Name:")
                    GeometryLinkedToLabel.grid(column =0, row = 2, sticky = NW)
                    GeometryLinkedToMessage = Message(LinkedToObjectFrame, textvariable = LinkedObjectNameVariable, width = 300)
                    GeometryLinkedToMessage.grid(column = 1, row = 2, sticky = NW)

                    
                SaveButton['command'] = GetOMG3
                SaveButton['state'] = 'enabled'        
                geometry = GeometriesTreeview.selection()
                        
                GeometryNameVariable.set(GeometriesTreeview.item(geometry)['values'][0])
                GeometryNameEntry['state'] = 'disabled'

                GeometryURIVariable.set(geometry[0])
                GeometryURIEntry['state'] = 'disabled'
                        
                ##Geometrystate
                ListGeometrystates = SearchGeometryStateNodes(geometry[0])
                    
                LinkOMG3GeometrystateLabel = Label(LinkToFrame2, text = "Already existing geometry states")
                LinkOMG3GeometrystateLabel.grid(column = 0, row = 3, sticky = NW)

                GeometrystateTreeview = Treeview(LinkToFrame2)
                GeometrystateTreeview.grid(column = 0, row = 4, sticky = NW)
                GeometrystateTreeview["selectmode"] = NONE
                GeometrystateTreeview["columns"] = ("one")
                GeometrystateTreeview.column("#0", width = 400)
                GeometrystateTreeview.heading("#0", text = "URI")
                GeometrystateTreeview.column("one", width = 200)
                GeometrystateTreeview.heading("one", text = "Name")

                NewGeometryButton = Button(LinkToFrame2, text = 'New', command = LinkObjects)
                NewGeometryButton.grid(column =0, row =2, sticky = NW)

                for geometrystate in ListGeometrystates:
                    GeometrystateTreeview.insert("",END,geometrystate[0],text = geometrystate[0], values =(geometrystate[1],))

                ZonesTreeview.destroy()
                ElementsTreeview.destroy()
                DamagesTreeview.destroy()

                for frame in LinkedObjectsFrameList:
                    frame.destroy()
                LinkedToObjectFrame = Frame(LinkToFrame2)
                LinkedToObjectFrame.grid(column =0, row = 5, sticky = NW)
                LinkedObject(geometry)
                LinkedObjectsFrameList.append(LinkedToObjectFrame)

            def OMG3ElementSelected(event):
                subject = ElementsTreeview.selection()[0]
                label = ElementsTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG3
                SaveButton['state'] = 'enabled'

            def OMG3ZoneSelected(event):
                subject = ZonesTreeview.selection()[0]
                label = ZonesTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG3
                SaveButton['state'] = 'enabled'

            def OMG3DamageSelected(event):
                subject = DamagesTreeview.selection()[0]
                label = DamagesTreeview.item(subject)['values'][0]
                SubjectURIVariable.set(subject)

                SaveButton['command'] = GetOMG3
                SaveButton['state'] = 'enabled'
                
            #print("link possible  objects")
                    
            #Can be linked to 
            ##Geometry
            LinkedObjectsFrameList =[]
            LinkToFrame2 = Frame(LinkToFrame)
            LinkToFrame2.grid(column = 0, row = 0, sticky = NW)
            tempFrames.append(LinkToFrame2)
            ListGeometries = SearchGeometryNodes()
            GeometryNameEntry['state'] = 'enabled'
            GeometryURIEntry['state'] = 'enabled'
            LinkOMG3GeometriesLabel = Label(LinkToFrame2, text = "Geometries")
            LinkOMG3GeometriesLabel.grid(column = 0, row = 0, sticky = NW)

            GeometriesTreeview = Treeview(LinkToFrame2)
            GeometriesTreeview.grid(column = 0, row = 1, sticky = NW)
            GeometriesTreeview["selectmode"] = BROWSE
            GeometriesTreeview["columns"] = ("one")
            GeometriesTreeview.column("#0", width = 400)
            GeometriesTreeview.heading("#0", text = "URI")
            GeometriesTreeview.column("one", width = 200)
            GeometriesTreeview.heading("one", text = "Name")

            for geometry in ListGeometries:
                GeometriesTreeview.insert("",END,geometry[0],text = geometry[0], values =(geometry[1],))

            GeometriesTreeview.bind('<<TreeviewSelect>>', OMG3GeometrySelected)

            ##Zone
                    
            ListZones = SearchZones()
                    
            LinkOMG3ZoneLabel = Label(LinkToFrame2, text = "Zones")
            LinkOMG3ZoneLabel.grid(column = 0, row = 2, sticky = NW)

            ZonesTreeview = Treeview(LinkToFrame2)
            ZonesTreeview.grid(column = 0, row = 3, sticky = NW)
            ZonesTreeview["selectmode"] = BROWSE
            ZonesTreeview["columns"] = ("one")
            ZonesTreeview.column("#0", width = 400)
            ZonesTreeview.heading("#0", text = "URI")
            ZonesTreeview.column("one", width = 200)
            ZonesTreeview.heading("one", text = "Name")

            for zone in ListZones:
                ZonesTreeview.insert("",END,zone[0],text = zone[0], values =(zone[1],))

            ZonesTreeview.bind('<<TreeviewSelect>>', OMG3ZoneSelected)
                    
            #Element
                    
            ListElements = SearchElements()

            LinkOMG3ElementLabel = Label(LinkToFrame2, text = "Elements")
            LinkOMG3ElementLabel.grid(column = 0, row = 4, sticky = NW)
                    
            ElementsTreeview = Treeview(LinkToFrame2)
            ElementsTreeview.grid(column = 0, row = 5, sticky = NW)
            ElementsTreeview["selectmode"] = BROWSE
            ElementsTreeview["columns"] = ("one")
            ElementsTreeview.column("#0", width = 400)
            ElementsTreeview.heading("#0", text = "URI")
            ElementsTreeview.column("one", width = 200)
            ElementsTreeview.heading("one", text = "Name")

            for element in ListElements:
                ElementsTreeview.insert("",END,element[0],text = element[0], values =(element[1],))

            ElementsTreeview.bind('<<TreeviewSelect>>', OMG3ElementSelected)
            #Damage
            ListDamages = SearchDamages()

            LinkOMG3DamageLabel = Label(LinkToFrame2, text = "Damages")
            LinkOMG3DamageLabel.grid(column = 0, row = 6, sticky = NW)
                    
            DamagesTreeview = Treeview(LinkToFrame2)
            DamagesTreeview.grid(column = 0, row = 7, sticky = NW)
            DamagesTreeview["selectmode"] = BROWSE
            DamagesTreeview["columns"] = ("one")
            DamagesTreeview.column("#0", width = 400)
            DamagesTreeview.heading("#0", text = "URI")
            DamagesTreeview.column("one", width = 200)
            DamagesTreeview.heading("one", text = "Name")

            for damage in ListDamages:
                DamagesTreeview.insert("",END,damage[0],text = damage[0], values =(damage[1],))

            DamagesTreeview.bind('<<TreeviewSelect>>', OMG3DamageSelected)

        LinkObjects()

    tempFrames = []

    AddNewGeometryFrame = Labelframe(frame, text = 'New Geometry')
    AddNewGeometryFrame.grid(column = 0, row = 0, sticky = NW)
    Frames.append(AddNewGeometryFrame)
    

    LinkToFrame = LabelFrame(frame, text = 'Link to')
    LinkToFrame.grid(column = 1, row = 0, sticky = NW)
    Frames.append(LinkToFrame)

    metadatalabelframe = LabelFrame(AddNewGeometryFrame, text ='Metadata')

    AddNewGeometryButtonFrame = Frame(frame)
    AddNewGeometryButtonFrame.grid(column =0, row = 1, sticky = SE)
    Frames.append(AddNewGeometryButtonFrame)

    CancelButton = Button(AddNewGeometryButtonFrame, text = 'Cancel', command = partial(Geometries, frame))
    CancelButton.grid(column = 0, row = 0, sticky = SE)

    SaveButton = Button(AddNewGeometryButtonFrame, text = 'Save', state = 'disabled')
    SaveButton.grid(column =1, row = 0, sticky = SE)
            
    GeometryOMGLevelLabel = Label(AddNewGeometryFrame, text = "OMG level:")
    GeometryOMGLevelLabel.grid(column = 0, row = 1, sticky = NW)

    OMGLevels = ["OMG L3", "OMG L2", "OMG L1"]
    OMGLevelCombobox = Combobox(AddNewGeometryFrame, width = 50, values = OMGLevels)
    print(GeometriesSettings)
    OMGLevelCombobox.set(GeometriesSettings[2])
    OMGLevelCombobox.grid(column = 1, row = 1, sticky = NW)
    OMGLevelCombobox.bind("<<ComboboxSelected>>", OMGLevelSelected)

    OMGinfo = StringVar()
    OMGLevelMessage = Message(AddNewGeometryFrame, textvariable = OMGinfo, width = 300)
    OMGLevelMessage.grid(column = 1, row = 2, sticky = NW)
    OMG1LabelFrame = LabelFrame(AddNewGeometryFrame, text = "OMG level 1")
    OMG2LabelFrame = LabelFrame(AddNewGeometryFrame, text = "OMG level 2")
    OMG3LabelFrame = LabelFrame(AddNewGeometryFrame, text = "OMG level 3")
            
    if OMGLevelCombobox.get() == "OMG L1":
        OMGinfo.set("Link the geometry directly to the object itself")
        SaveAsOptions = ["Embeded","Local file", "Online file"]

    if OMGLevelCombobox.get() == "OMG L2":
        OMGinfo.set("Link the geometry to the object using one intermediate node. This methods allows to link relevant metadata.")
        SaveAsOptions = ["Embeded","Local file", "Online file", "Part of"]
            
    if OMGLevelCombobox.get() == "OMG L3":
        OMGinfo.set("Link the geometry to the object using two intermediate nodes. This methods allows to link relevant metadata and use version control.")
        SaveAsOptions = ["Embeded","Local file", "Online file", "Part of"]
            
    GeometriesSaveAsLabel = Label(AddNewGeometryFrame, text = "Save as:")
    GeometriesSaveAsLabel.grid(column = 0, row = 3, sticky = NW)
    
    GeometriesSaveAsCombobox = Combobox(AddNewGeometryFrame, width = 50,values = SaveAsOptions)
    GeometriesSaveAsCombobox.grid(column = 1, row = 3, sticky = NW)
    GeometriesSaveAsCombobox.set(GeometriesSettings[3])
    GeometriesSaveAsCombobox.bind("<<ComboboxSelected>>", SaveAsSelected)
            
    SaveAsinfo = StringVar()
    GeometriesSaveAsMessage = Message(AddNewGeometryFrame, textvariable = SaveAsinfo, width = 300)
    GeometriesSaveAsMessage.grid(column = 1, row = 4, sticky = NW)
    if GeometriesSaveAsCombobox.get() == 'Embeded':
        SaveAsinfo.set("""The entire geometry description will be saved into a Linked Data literal 
        "..."^^xsd:string or "..."^^xsd:base64""")
    if GeometriesSaveAsCombobox.get() == 'Local file':
        SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
    if GeometriesSaveAsCombobox.get() == 'Online file':
        SaveAsinfo.set("""The path to the local file will be saved into a Linked Data literal 
                "..."^^xsd:anyURI""")
    if GeometriesSaveAsCombobox.get() == 'Part of':
        SaveAsinfo.set("""A reference will be saved to another geometry containig the element literal 
                "...\"""")
    
    if not kwargs.get('geometrylocation', None) == None:
        filelocation = kwargs.get('geometrylocation')
        print(filelocation)
    else:
        filelocation = ""
    if not kwargs.get('objectid', None) == None:
        objectid = kwargs.get('objectid')
        print(objectid)
    else:
        objectid = ""
    if not kwargs.get('origin', None) == None:
        Origin = kwargs.get('origin')
        print(Origin)
    else:
        Origin = ""

    if OMGLevelCombobox.get() == "OMG L1":
        OMGLevel1Frame()    
    if OMGLevelCombobox.get() == "OMG L2":
        OMGLevel2Frame()       
    if OMGLevelCombobox.get() == "OMG L3":
        OMGLevel3Frame()

def Elements(frame):
    CloseFrame()

    elementsdetailframe = Frame(frame)
    elementsdetailframe.pack()

    Frames.append(elementsdetailframe)

    #Treeview containing all the geometries
    ElementsTreeview = Treeview(elementsdetailframe)
    ElementsTreeview.grid(column = 0, row = 0, padx = 10, pady = 10, sticky = NW)
    ElementsTreeview["selectmode"] = BROWSE
    ElementsTreeview["columns"] = ("one")
    ElementsTreeview.column("#0", width = 500, stretch = NO)
    ElementsTreeview.heading("#0", text = "URI")
    ElementsTreeview.column("one", width = 250)
    ElementsTreeview.heading("one", text = "Name")
    
    for element in SearchElements():
        ElementsTreeview.insert("", END ,element[0], text = element[0], values = (element[1],))
    
    ElementsButtonFrame = Frame(elementsdetailframe)
    ElementsButtonFrame.grid(column = 0, row = 1,sticky = SE)

    AddNewElementButton = Button(ElementsButtonFrame, text = 'Add', command = partial(NewElementUI,frame))
    AddNewElementButton.grid(column =0, row = 0)

def NewElementUI(frame):
    def GETnewElement(frame):
        #Create a list with al classes that need to be added to the object
        element = ElementNameVariable.get()
        elementuri = ElementURIVariable.get()
        graph = ElementsSettings[0]

        #The extra classes that have been added an can be seen in the listbox, have already been stored in ListClasses.
        #sending al this variables to the function that will save them
        SaveNewElementProcess = Process(target = ElementToGraph, args =(SaveToDefaultGraph,UrlPost, headersPost, graph, element, elementuri, ListClasses))
        processes.append(SaveNewElementProcess)
        Elements(frame)
        SaveNewElementProcess.start()

    def AddExtraElementClass():
        #print("Ontology classes")
        #print(OntologyClasses)
        for Class in OntologyClasses:
            if Class[0] == AddElementClassClassCombobox.get():
                ListClasses.append(Class[1])
                ElementClassesListbox.insert(END, Class[0])
        OntologyClasses.clear()
        OntologyClassesLabels.clear()
        AddElementClassOntologyCombobox.set('')
        AddElementClassClassCombobox.set('')
        ClassComment.set('')
        AddElementClassClassCombobox['values'] = OntologyClassesLabels
            
    def AdditionalClassSelected(event):
        for Class in OntologyClasses:
            if Class[0] == AddElementClassClassCombobox.get():
                ClassComment.set(Class[2])
            
    def AddElementClassOntologySelected(event):
        #print(ListClasses)
        OntologyClassesLabels.clear()
        OntologyClasses.clear()
        for Ontology in OntologiesList:
            if Ontology[0] == AddElementClassOntologyCombobox.get():
                #print(ListClasses)
                for Class in SearchAllClassesWithoutSubclasses(Ontology[2],ListClasses):
                    OntologyClasses.append(Class)
                    OntologyClassesLabels.append(Class[0])
        AddElementClassClassCombobox['values'] = OntologyClassesLabels
    
    CloseFrame()

    ListClasses =[]
    ListClasses.append("https://w3id.org/bot#Element")
    ElementNameVariable = StringVar()
    ElementURIVariable = StringVar()

    AddNewElementFrame = Labelframe(frame, text = 'New Element')
    AddNewElementFrame.grid(column = 2, row = 0, rowspan = 10, sticky = NW, pady =10)
    Frames.append(AddNewElementFrame)
    
    ElementNameLabel = Label(AddNewElementFrame, text = 'Element:')
    ElementNameLabel.grid(column = 0, row = 0, pady = 10, sticky = NW)
    
    ElementNameEntry = Entry(AddNewElementFrame,textvariable = ElementNameVariable, width = 50)
    ElementNameEntry.grid(column =1, row = 0, pady =10, sticky = NW)

    def ElementName(*args):
        ElementURI = ElementsSettings[1] + quote(ElementNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
        ElementURIVariable.set(ElementURI)
    
    ElementNameVariable.trace_add("write", ElementName)

    ElementURILabel = Label(AddNewElementFrame, text = "URI:")
    ElementURILabel.grid(column =0, row =1, sticky = NW)

    ElementURIEntry = Entry(AddNewElementFrame, textvariable = ElementURIVariable, width = 75)
    ElementURIEntry.grid(column =1, row =1, sticky = NW)
    
    ElementBOTClassLabel = Label(AddNewElementFrame, text = "BOT class:")
    ElementBOTClassLabel.grid(column = 0, row = 4, sticky = NW)
    BOTinfo = StringVar()

    BOTClassesCombobox = Combobox(AddNewElementFrame, width = 50)
    BOTClassesCombobox.insert(0,'Element')
    BOTClassesCombobox.grid(column = 1, row = 4, sticky = NW)
    BOTClassesCombobox['state'] = 'disabled'
    BOTinfo.set("Constituent of a construction entity with a characteristic technical function, form or position [12006-2, 3.4.7]")
            
    ExtraClassesLabelframe = LabelFrame(AddNewElementFrame, text = 'Extra classes')
    ExtraClassesLabelframe.grid(column = 0, row = 6, sticky = NW, columnspan = 2)

    ElementClasseLabel = Label(ExtraClassesLabelframe, text = 'Element classes:')
    ElementClasseLabel.grid(column = 0, row = 0, sticky = NW)
    
    ElementClassesListbox = Listbox(ExtraClassesLabelframe, selectmode = MULTIPLE)
    ElementClassesListbox.grid(column = 1, row = 0, sticky = NW)
    
    AddElementClassLabelFrame = LabelFrame(ExtraClassesLabelframe, text = 'Add Element class:')
    AddElementClassLabelFrame.grid(column = 0, row = 1, sticky = NW , padx = 10, columnspan = 2)
    
    AddElementClassOntologyLabel = Label(AddElementClassLabelFrame, text = 'Ontology: ')
    AddElementClassOntologyLabel.grid(column = 0, row = 0 , sticky = NW)
    
    #creating a list with the ontologies for the combobox to use
    OntologiesList = []
    ValuesOntologyCombobox = []
    for Ontology in SearchOntologies():
        OntologiesList.append(Ontology)
        ValuesOntologyCombobox.append(Ontology[0])
    #Create a list to store all classes of the object until it can be saved
    AddElementClassOntologyCombobox = Combobox(AddElementClassLabelFrame, width = 50, values = ValuesOntologyCombobox)
    AddElementClassOntologyCombobox.grid(column = 1, row = 0, sticky = NW)
    AddElementClassOntologyCombobox.bind('<<ComboboxSelected>>', AddElementClassOntologySelected)
            
    OntologyClasses =[]
    OntologyClassesLabels = []

    AddElementClassClassLabel = Label(AddElementClassLabelFrame, text = 'Class:')
    AddElementClassClassLabel.grid(column = 0, row =1, sticky = NW)

    AddElementClassClassCombobox = Combobox(AddElementClassLabelFrame)
    AddElementClassClassCombobox.grid(column = 1, row = 1, sticky = NW)
    AddElementClassClassCombobox.bind("<<ComboboxSelected>>",AdditionalClassSelected)

    AddElementClassClassCommentLabel = Label(AddElementClassLabelFrame, text = 'Explanation:')
    AddElementClassClassCommentLabel.grid(column =0, row = 2, sticky = NW, columnspan = 2)

    ClassComment = StringVar()
    AddElementClassClassComment = Message(AddElementClassLabelFrame, textvariable = ClassComment, width = 300)
    AddElementClassClassComment.grid(column = 1, row = 2, sticky = NW)

    AddElementClassButton = Button(AddElementClassLabelFrame, text = 'Add', command = AddExtraElementClass)
    AddElementClassButton.grid(column =3, row = 1, sticky = SE)

    SaveNewElementButton = Button(AddNewElementFrame, text = 'Save Element',command = partial(GETnewElement,frame))
    SaveNewElementButton.grid(column = 1, row = 10, sticky = SE)

    CancelButton = Button(AddNewElementFrame, text = 'Cancel', command = partial(Elements,frame))
    CancelButton.grid(column = 1, row = 11, sticky = SE)

def Zones(frame):
    CloseFrame()

    zonesdetailframe = Frame(frame)
    zonesdetailframe.pack()

    Frames.append(zonesdetailframe)

    #Treeview containing all the geometries
    ZonesTreeview = Treeview(zonesdetailframe)
    ZonesTreeview.grid(column = 0, row = 0, padx = 10, pady = 10, sticky = NW)
    ZonesTreeview["selectmode"] = BROWSE
    ZonesTreeview["columns"] = ("one")
    ZonesTreeview.column("#0", width = 500, stretch = NO)
    ZonesTreeview.heading("#0", text = "URI")
    ZonesTreeview.column("one", width = 250)
    ZonesTreeview.heading("one", text = "Name")
    
    for zone in SearchZones():
        ZonesTreeview.insert("", END ,zone[0], text = zone[0], values = (zone[1],))
    
    ZonesButtonFrame = Frame(zonesdetailframe)
    ZonesButtonFrame.grid(column = 0, row = 1,sticky = SE)

    AddNewZoneButton = Button(ZonesButtonFrame, text = 'Add', command = partial(NewZoneUI,frame))
    AddNewZoneButton.grid(column =0, row = 0)
    
def NewZoneUI(frame):
    #Function that will show the explanation of the bot class
    def BOTclassSelected(event):
        for BOTclass in BOTzoneClasses:
            if BOTclass[0] == BOTClassesCombobox.get():
                BOTinfo.set(BOTclass[2])
                if len(ListClasses) >= 1:
                    ListClasses[0] = BOTclass[1]
                else: 
                    ListClasses.append(BOTclass[1])
                #print("BOT class: %s " % BOTclass[0])
                #print(ListClasses)
            
    #Function to search all classes without subclasses in a certain graph/ontology
    def AddZoneClassOntologySelected(event):
        #print(ListClasses)
        OntologyClassesLabels.clear()
        OntologyClasses.clear()
        for Ontology in OntologiesList:
            if Ontology[0] == AddZoneClassOntologyCombobox.get():
                #print(ListClasses)
                for Class in SearchAllClassesWithoutSubclasses(Ontology[2],ListClasses):
                    OntologyClasses.append(Class)
                    OntologyClassesLabels.append(Class[0])
        AddZoneClassClassCombobox['values'] = OntologyClassesLabels
            
    #function that gives the explanation when an additional class is selected
    def AdditionalClassSelected(event):
        for Class in OntologyClasses:
            if Class[0] == AddZoneClassClassCombobox.get():
                ClassComment.set(Class[2])
            
    #function to store a extra class, will be added to the listbox
    def AddExtraZoneClass():
        #print("Ontology classes")
        #print(OntologyClasses)
        for Class in OntologyClasses:
            if Class[0] == AddZoneClassClassCombobox.get():
                if len(ListClasses) == 0:
                    ListClasses.append("")
                    ListClasses.append(Class[1])
                else:
                    ListClasses.append(Class)
                ZoneClassesListbox.insert(END, Class[0])
        OntologyClasses.clear()
        OntologyClassesLabels.clear()
        AddZoneClassOntologyCombobox.set('')
        AddZoneClassClassCombobox.set('')
        ClassComment.set('')
        AddZoneClassClassCombobox['values'] = OntologyClassesLabels

    #function that gets the needed values to send to the graph
    def GETnewZone(frame):
        #Create a list with al classes that need to be added to the object
        zone = ZoneNameVariable.get()
        zoneuri = ZoneURIVariable.get()
        graph = ZonesSettings[0]

        #The extra classes that have been added an can be seen in the listbox, have already been stored in ListClasses.
        #sending all this variables to the function that will save them
        SaveNewZoneProcess = Process(target = ZoneToGraph, args =(SaveToDefaultGraph, UrlPost, headersPost, graph, zone, zoneuri,ListClasses))
        processes.append(SaveNewZoneProcess)
        Zones(frame)
        SaveNewZoneProcess.start()



    CloseFrame()

    ListClasses = [] #creating a list to temporarly store the added classes
    ZoneNameVariable = StringVar()
    ZoneURIVariable = StringVar()

    AddNewZoneFrame = Labelframe(frame, text = 'New Zone')
    AddNewZoneFrame.grid(column = 2, row = 0, rowspan = 10, sticky = NW, pady =10)
    Frames.append(AddNewZoneFrame)

    ZoneNameLabel = Label(AddNewZoneFrame, text = 'Name:')
    ZoneNameLabel.grid(column = 0, row = 0, pady = 10, sticky = NW)
    
    ZoneNameEntry = Entry(AddNewZoneFrame, textvariable = ZoneNameVariable, width = 50)
    ZoneNameEntry.grid(column =1, row = 0, pady =10, sticky = NW)

    def ZoneName(*args):
        ZoneURI = ZonesSettings[1] + quote(ZoneNameVariable.get()+ datetime.now().strftime('%H%M%d%m%Y'))
        ZoneURIVariable.set(ZoneURI)
    
    ZoneNameVariable.trace_add("write", ZoneName)

    ZoneURILabel = Label(AddNewZoneFrame, text = 'URI:')
    ZoneURILabel.grid(column =0, row =1, sticky = NW)
    
    ZoneURIEntry = Entry(AddNewZoneFrame, textvariable = ZoneURIVariable, width = 50)
    ZoneURIEntry.grid(column = 1, row =1, sticky = NW)
            
    ZoneBOTClassLabel = Label(AddNewZoneFrame, text = "BOT class:")
    ZoneBOTClassLabel.grid(column = 0, row = 4, sticky = NW)
    BOTinfo = StringVar()
            
    #Creating a list of all BOT classes
    BOTzoneClasses = ListBOTZoneClasses()
    #print(BOTzoneClasses)
    BOTzoneClassLabels = []
    for BOTzoneClass in BOTzoneClasses:
        BOTzoneClassLabels.append(BOTzoneClass[0])

    BOTClassesCombobox = Combobox(AddNewZoneFrame, width = 50, values = BOTzoneClassLabels)
    BOTClassesCombobox.grid(column = 1, row = 4, sticky = NW)
    BOTClassesCombobox.bind('<<ComboboxSelected>>', BOTclassSelected)
    ZoneBOTClassInfoLabel = Message(AddNewZoneFrame, textvariable = BOTinfo, width = 300)
    ZoneBOTClassInfoLabel.grid(column = 1, row = 5, sticky = NW)

    ExtraClassesLabelframe = LabelFrame(AddNewZoneFrame, text = 'Extra classes')
    ExtraClassesLabelframe.grid(column = 0, row = 6, sticky = NW, columnspan = 2)

    ZoneClasseLabel = Label(ExtraClassesLabelframe, text = 'Zone classes:')
    ZoneClasseLabel.grid(column = 0, row = 0, sticky = NW)

    ZoneClassesListbox = Listbox(ExtraClassesLabelframe, selectmode = MULTIPLE)
    ZoneClassesListbox.grid(column = 1, row = 0, sticky = NW)
    
    AddZoneClassLabelFrame = LabelFrame(ExtraClassesLabelframe, text = 'Add zone class:')
    AddZoneClassLabelFrame.grid(column = 0, row = 1, sticky = NW , padx = 10, columnspan = 2)
    
    AddZoneClassOntologyLabel = Label(AddZoneClassLabelFrame, text = 'Ontology: ')
    AddZoneClassOntologyLabel.grid(column = 0, row = 0 , sticky = NW)

    #creating a list with the ontologies for the combobox to use
    OntologiesList = []
    ValuesOntologyCombobox = []
    for Ontology in SearchOntologies():
        OntologiesList.append(Ontology)
        ValuesOntologyCombobox.append(Ontology[0])

    #Create a list to store all classes of the object until it can be saved
    AddZoneClassOntologyCombobox = Combobox(AddZoneClassLabelFrame, width = 50, values = ValuesOntologyCombobox)
    AddZoneClassOntologyCombobox.grid(column = 1, row = 0, sticky = NW)
    AddZoneClassOntologyCombobox.bind('<<ComboboxSelected>>', AddZoneClassOntologySelected)
            
    OntologyClasses =[]
    OntologyClassesLabels = []
    
    AddZoneClassClassLabel = Label(AddZoneClassLabelFrame, text = 'Class:')
    AddZoneClassClassLabel.grid(column = 0, row =1, sticky = NW)
    
    AddZoneClassClassCombobox = Combobox(AddZoneClassLabelFrame)
    AddZoneClassClassCombobox.grid(column = 1, row = 1, sticky = NW)
    AddZoneClassClassCombobox.bind("<<ComboboxSelected>>",AdditionalClassSelected)
    
    AddZoneClassClassCommentLabel = Label(AddZoneClassLabelFrame, text = 'Explanation:')
    AddZoneClassClassCommentLabel.grid(column =0, row = 2, sticky = NW, columnspan = 2)
    
    ClassComment = StringVar()
    AddZoneClassClassComment = Message(AddZoneClassLabelFrame, textvariable = ClassComment, width = 300)
    AddZoneClassClassComment.grid(column = 1, row = 2, sticky = NW)
    
    AddZoneClassButton = Button(AddZoneClassLabelFrame, text = 'Add', command = AddExtraZoneClass)
    AddZoneClassButton.grid(column =3, row = 1, sticky = SE)

    SaveNewZoneButton = Button(AddNewZoneFrame, text = 'Save zone',command = partial(GETnewZone, frame))
    SaveNewZoneButton.grid(column = 1, row = 10, sticky = SE)
    
    CancelButton = Button(AddNewZoneFrame, text = 'Cancel', command = partial(Zones, frame))
    CancelButton.grid(column = 1, row = 11, sticky = SE)

def Damages(frame):
    CloseFrame()

    damagesdetailframe = Frame(frame)
    damagesdetailframe.pack()

    Frames.append(damagesdetailframe)

    #Treeview containing all the geometries
    DamagesTreeview = Treeview(damagesdetailframe)
    DamagesTreeview.grid(column = 0, row = 0, padx = 10, pady = 10, sticky = NW)
    DamagesTreeview["selectmode"] = BROWSE
    DamagesTreeview["columns"] = ("one")
    DamagesTreeview.column("#0", width = 500, stretch = NO)
    DamagesTreeview.heading("#0", text = "URI")
    DamagesTreeview.column("one", width = 250)
    DamagesTreeview.heading("one", text = "Name")
    
    for damage in SearchDamages():
        DamagesTreeview.insert("", END ,damage[0], text = damage[0], values = (damage[1],))

def UserInterface(master):

    master = master
    master.title("Linked Building Data Application - KU Leuven")
    master.geometry("1200x600")
    
    #create a scrollable canvas for the other frames to be placed in, this makes sure the application does not take on weird dimensions
    mainframe = Frame(master)
    mainframe.pack(fill=BOTH, expand = True)

    maincanvas = Canvas(mainframe)
    yscrollbar = Scrollbar(mainframe, orient = "vertical", command = maincanvas.yview)
    xscrollbar = Scrollbar(mainframe, orient = "horizontal", command = maincanvas.xview)

    scrollableframe = Frame(maincanvas)
    scrollableframe.bind("<Configure>",lambda e: maincanvas.configure(scrollregion = maincanvas.bbox("all")))
    maincanvas.create_window((0,0),window = scrollableframe,anchor = "nw")
    maincanvas.configure(yscrollcommand = yscrollbar.set)
    maincanvas.configure(xscrollcommand = xscrollbar.set)
    xscrollbar.pack(side="bottom", fill = "x")
    yscrollbar.pack(side = "right", fill = "y")
    maincanvas.pack(side = "left", fill = BOTH, expand = True)
    
    #Create a Menubar for the application
    menubar = Menu(master)
    filemenu = Menu(menubar, tearoff = 0)
    filemenu.add_command(label = "Save", command = SAVEtoFile)
    filemenu.add_command(label = "New", command = partial(CreateNewProjectFrame, scrollableframe))
    filemenu.add_command(label = "Open", command = partial(LoadExsistingProjectFrame, scrollableframe))
    filemenu.add_command(label = "Export")
    filemenu.add_separator()
    filemenu.add_command(label = "Exit", command = on_closing)
    menubar.add_cascade(label = "File", menu = filemenu)

    projectmenu = Menu(menubar, tearoff =0)
    projectmenu.add_command(label = "Info", command = partial(ProjectInformation, scrollableframe))
    projectmenu.add_command(label = "Coordinates", command = partial(Coordinates, scrollableframe))
    projectmenu.add_command(label = "Ontologies", command = partial(Ontologies, scrollableframe))
    menubar.add_cascade(label = "Project", menu = projectmenu)

    settingsmenu = Menu(menubar, tearoff = 0)
    settingsmenu.add_command(label = "Linked Data", command = partial(LinkedData, scrollableframe))
    settingsmenu.add_command(label = "Geometry", command = partial(GeometrySettings, scrollableframe))
    menubar.add_cascade(label = "Settings", menu = settingsmenu)

    objectmenu = Menu(menubar, tearoff=0)
    objectmenu.add_command(label = "Overview")
    objectmenu.add_command(label = "Geometries", command = partial(Geometries, scrollableframe))
    objectmenu.add_command(label = "Zones", command = partial(Zones, scrollableframe))
    objectmenu.add_command(label = "Elements", command = partial(Elements, scrollableframe))
    objectmenu.add_command(label = "Damages", command = partial(Damages, scrollableframe))
    menubar.add_cascade(label = "Objects", menu = objectmenu)

    master.config(menu = menubar)
    StartupFrame(scrollableframe)

    def Listener():
        InternalListener = socket.socket()
        InternalListener.bind((host,internalport))
        #print('listening on: ', host, ':', internalport)
        InternalListener.listen(10)
        while STATUS:
            InternalClient, addr = InternalListener.accept()
            #print ('Got connection from', addr)
            var = InternalClient.recv(1024).decode()
            #print(var)
            if var == 'New':
                InternalClient.close()
            if var =='END':
                InternalClient.close()
                InternalListener.close()
                break

    ##CAD Connections

    def ExternalListener():
        ExternalListener = socket.socket()
        ExternalListener.bind((host,externalport))
        print('listening on: ', host, ':', externalport)
        ExternalListener.listen(10)
        while STATUS:
            ExternalClient, addr = ExternalListener.accept()
            print ('Got connection from', addr)
            var = ExternalClient.recv(1024).decode()
            print(var)
            if var == 'AddNewGeometry':
                
                ExternalClient.send(ProjectGeometryDirectory.encode()) #Project directory doorsturen naar Rhino
                
                objectid = ExternalClient.recv(1024).decode()
                
                ExternalClient.send(PreferedFormat[2].encode())

                Origin = ExternalClient.recv(1024).decode()
                ExternalClient.send('OK'.encode())
                
                filelocation = ExternalClient.recv(1024).decode()
                ExternalClient.close()
                if not filelocation == "Not Supported Format":
                    NewGeometryUI(scrollableframe, geometrylocation = filelocation, objectid = objectid, origin = Origin)
                else:
                    print("Format not supported by CAD application")
                
            if var == 'ExportModelSTEP1':
                ExternalClient.send(ProjectGeometryDirectory.encode()) #Project directory doorsturen naar Rhino
                ExternalClient.recv(1024)
                ExternalClient.send(ExportLevel.encode())
                ExternalClient.close()
        
            if var == 'ExportModelSTEP2':
                ExternalClient.send("OK".encode())
                
                LayerClass = ExternalClient.recv(1024).decode()
                ExternalClient.send("OK".encode())
                
                Name = ExternalClient.recv(1024).decode()
                ExternalClient.send("OK".encode())
                
                location = ExternalClient.recv(1024).decode()
                ExternalClient.send('OK'.encode())

                Origin = ExternalClient.recv(1024).decode()
                ExternalClient.close()

                UploadpartOfModel = Process(target = RecievedPartOfModel, args = (SaveToDefaultGraph,UrlPost, headersPost,ProjectName,LayerClass,Name,location,GeometriesSettings,PreferedFormat,SupportedGeometryFormats, Origin,DefaultCS))
                processes.append(UploadpartOfModel)
                UploadpartOfModel.start()
            if var == 'ExportModelOption2':
                ExternalClient.send("OK".encode())
                LayerClass = ExternalClient.recv(1024).decode()
                ExternalClient.send("OK".encode())
                Name = ExternalClient.recv(1024).decode()
                ExternalClient.send("OK".encode())
                groups = []
                while True:
                    msg = ExternalClient.recv(1024).decode()
                    if not msg == "END":
                        groups.append(msg)
                        ExternalClient.send("OK".encode())
                    else:
                        ExternalClient.send("OK".encode())
                        break

                location = ExternalClient.recv(1024).decode()
                ExternalClient.send('OK'.encode())

                Origin = ExternalClient.recv(1024).decode()
                ExternalClient.close()
                if len(groups)> 0:
                    UploadpartOfModel = Process(target = RecievedPartOfModelrelations, args = (SaveToDefaultGraph,UrlPost, headersPost,ProjectName,LayerClass,Name,location,GeometriesSettings,PreferedFormat,SupportedGeometryFormats,groups,Origin,DefaultCS))
                else:
                    UploadpartOfModel = Process(target = RecievedPartOfModel, args = (SaveToDefaultGraph,UrlPost, headersPost,ProjectName,LayerClass,Name,location,GeometriesSettings,PreferedFormat,SupportedGeometryFormats,Origin,DefaultCS))
                processes.append(UploadpartOfModel)
                UploadpartOfModel.start()
            if var =='FORMAT':
                ExternalClient.send(PreferedFormat[2].encode())
                ExternalClient.close()
            if var == 'CHECK':
                ExternalClient.send("OK".encode())
                namespace = "http://LBDA_%s.edu/Model/" % quote(ProjectName)
                name = ExternalClient.recv(1024).decode()
                objectIRI = namespace + quote(name)
                Answer = CheckIfObjectExists(objectIRI)
                ExternalClient.send(str(Answer).encode())
                ExternalClient.close()

            if var =='END':
                ExternalClient.close()
                ExternalListener.close()
                break
        
    InternalServer = Thread(target = Listener)
    InternalServer.start()
    threads.append(InternalServer)
    ExternalServer = Thread(target = ExternalListener)
    ExternalServer.start()
    threads.append(ExternalServer)
    
def Main():
    UserInterface(root)
    root.protocol("WM_DELETE_WINDOW", on_closing)
    root.mainloop()

if __name__ == '__main__':
    print("session:" + datetime.now().strftime('%H:%M %d/%m/%Y') + " started on: %s " % host + " IP: %s " % ip_address +  "Version: %s"% Version)


    #reading the settings
    dir = os.path.dirname(__file__)
    filename = os.path.join(dir, 'Settings','GeometryFormats.txt')
    Geometryformatsfile = open(filename,"r")
    for line in Geometryformatsfile:
        line = line.split(",")
        SupportedGeometryFormat = (line[0],line[1],line[2],line[3],line[4],line[5],line[6],line[7])
        #print("( %s, %s, %s, %s, %s , %s , %s , %s )" % (SupportedGeometryFormat[0],SupportedGeometryFormat[1],SupportedGeometryFormat[2],SupportedGeometryFormat[3],SupportedGeometryFormat[4],SupportedGeometryFormat[5],SupportedGeometryFormat[6],SupportedGeometryFormat[7]))
        SupportedGeometryFormats.append(SupportedGeometryFormat)

    #giving the global variables their default values
    ProjectName ="Untitled"
    dir = os.path.dirname(__file__)
    ProjectDirectory = os.path.join(dir, 'Projects')
    ProjectGeometryDirectory = ProjectDirectory + "\Geometry"
    ProjectFile = ""
    ProjectCreator = "Unknown"
    Language = "en"
    LinkedDataManager = ""
    PreferedFormat = SupportedGeometryFormats[0]
    ExportLevel = "Element level"
    Main()
    

