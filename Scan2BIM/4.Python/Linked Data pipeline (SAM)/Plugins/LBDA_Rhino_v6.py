
import socket
import rhinoscriptsyntax as rs
import sys
import os
import scriptcontext
import Eto.Forms as forms
import Eto.Drawing as drawing
import Rhino.UI
from Rhino import *
from Rhino.Commands import *
from Rhino.DocObjects import *
from Rhino.Input import *
from scriptcontext import doc
import System
import threading
from collections import Counter


class RhinoPluginUI(forms.Form):

    def __init__(self):
        Listener = socket.socket()
        threads = []
        # Initialize dialog box
        self.Title = 'Linked Building Data Application v7'
        self.Padding = drawing.Padding(10)
        self.Resizable = False

        self.AddNewGeometryButton = forms.Button(Text = 'Add new geometry')
        self.AddNewGeometryButton.Click += self.AddNewGeometry
        
        self.ExportModelButton = forms.Button(Text = 'Create LD Model')
        self.ExportModelButton.Click += self.ExportModel

        
        # Create a table layout and add all the controls
        layout = forms.DynamicLayout()
        layout.Spacing = drawing.Size(5, 5)
        layout.AddRow(self.AddNewGeometryButton)
        layout.AddRow(self.ExportModelButton)

        # Set the dialog content
        self.Content = layout    

    def AddNewGeometry(self, sender, e):
        try:
            objects = rs.GetObjects("Please select a object", 0, True, True)
            print objects
            s = socket.socket()                 # Create a socket object
            host = socket.gethostname()         # Get local machine name
            port = 2412                         # Reserve a port for your service.
            s.connect((host, port))             #conecteer
            s.send('AddNewGeometry'.encode())
            ProjectDirectory = s.recv(1024).decode()
            print ProjectDirectory
            s.send(str(objects[0]).encode())
            Format = s.recv(1024).decode()
            PathGeometry = str(ProjectDirectory)+"\\Geometry"
        except:
            print "something went wrong"
            return Rhino.Commands.Result.Success
        try:
            os.mkdir(PathGeometry)
        except OSError:
            print ("Creation of the directory %s failed" % PathGeometry)
        else:
            print ("Successfully created the directory %s " % PathGeometry)
        for object in objects:
            rs.Command("_selId " + str(object) + " _Enter")

        try:
            filelocation = PathGeometry +"\\"+str(objects[0])+ Format
            print filelocation
            Command = "-_Export " + filelocation + " _Enter " + "_Enter "
            rs.Command(Command)
            print "object has been saved"
            s.send(str(filelocation).encode())
        except: 
            print "Export failed"
    def AskFormat(self):
        s = socket.socket()                 # Create a socket object
        host = socket.gethostname()         # Get local machine name
        port = 2412                         # Reserve a port for your service.
        s.connect((host, port))             #conecteer
        s.send('FORMAT'.encode())
        preferedformat = s.recv(1024).decode()
        s.close()
        print preferedformat
        return preferedformat

    def DetermineLayerOfGroup(self, group):
        print "determining the layer of a group"
        print group
        groupedObjects = rs.ObjectsByGroup(group,True)
        print groupedObjects
        Layers = []
        for objectid in groupedObjects:
            layer = rs.ObjectLayer(objectid)
            Layers.append(layer)
            print layer
        splittedlayersList = []
        shortestlayerTuple = []
        shortestlength = 20
        for layer in Layers:
            layer = str(layer).split('::')
            print layer
            splittedlayersList.append(layer)
            if len(layer) < shortestlength:
                shortestlayerTuple = layer
                shortestlength = len(layer)
            if len(layer) == shortestlength:
                length = len(layer)
                print length
                print shortestlayerTuple
                while not str(layer[length-1]) == str(shortestlayerTuple[length-1]):
                    shortestlayerTuple = []
                    shortestlength = shortestlength-1
                    i=0
                    print i
                    while i <= length-2:
                        shortestlayerTuple.insert(i, layer[i])
                        print shortestlayerTuple
                        i = i + 1
                    length = length -1

        Name = shortestlayerTuple[shortestlength-1]
        return Name

    def ExportModel(self, sender, e):
        Format = self.AskFormat()
        s = socket.socket()                 # Create a socket object
        host = socket.gethostname()         # Get local machine name
        port = 2412                         # Reserve a port for your service.
        s.connect((host, port))             #conecteer
        s.send('ExportModelSTEP1'.encode())
        ProjectDirectory = s.recv(1024).decode()
        print ProjectDirectory
        PathGeometry = str(ProjectDirectory)+"\\Geometry"
        print PathGeometry
        s.send("OK".encode())
        ExportLOD = s.recv(1024).decode()
        print ExportLOD
        s.close()

        #List All groups
        Groups = rs.GroupNames()
        print Groups
        if Groups:
            for group in Groups:
                if rs.IsGroupEmpty(group):
                    print group
                    groupedObjects = rs.ObjectsByGroup(group,True)
                    print groupedObjects
                    layer = self.DetermineLayerOfGroup(group)
                    print(layer)
                
                    try:
                        s = socket.socket()                 # Create a socket object
                        host = socket.gethostname()         # Get local machine name
                        port = 2412                         # Reserve a port for your service.
                        s.connect((host, port))             #conecteer
                        s.send('ExportModelSTEP2'.encode())
                        s.recv(1024)
                        s.send(str(layer).encode())
                        s.recv(1024)
                        s.send(str(group).encode())
                        s.recv(1024)
                        filelocation = PathGeometry +"\\"+str(group)+ Format
                        print filelocation
                        Command = "-_Export " + filelocation + " _Enter " + "_Enter "
                        rs.Command(Command)
                        print "object has been saved"
                        s.send(str(filelocation).encode())
                        s.close()
                    except: 
                        print "Export failed"
        else:
            print "No groups found"
        print ExportLOD
        if ExportLOD == "Element level":
            objects = rs.AllObjects(select=False, include_lights=False, include_grips=False, include_references=False)
            for objectid in objects:
                if not rs.IsObjectInGroup(objectid):
                    print objectid                
                    try:
                        layer = rs.ObjectLayer(objectid)
                        layer = rs.LayerId(layer)
                        layer = rs.LayerName(layer, fullpath=False)

                        s = socket.socket()                 # Create a socket object
                        host = socket.gethostname()         # Get local machine name
                        port = 2412                         # Reserve a port for your service.
                        s.connect((host, port))             #conecteer
                        s.send('ExportModelSTEP2'.encode())
                        s.recv(1024)
                        s.send(str(layer).encode())
                        s.recv(1024)
                        s.send(str(objectid).encode())
                        s.recv(1024)
                        filelocation = PathGeometry +"\\"+str(objectid)+ Format
                        print filelocation
                        Command = "-_Export " + filelocation + " _Enter " + "_Enter "
                        rs.Command(Command)
                        print "object has been saved"
                        s.send(str(filelocation).encode())
                        s.close()
                    except: 
                        print "Export failed"
        else:
            print "No objects found"
        if ExportLOD == "Subelement level":
            objects = rs.AllObjects(select=False, include_lights=False, include_grips=False, include_references=False)
            for objectid in objects:
                print objectid                
                try:
                    groups = rs.ObjectGroups(objectid)
                    lengthgroups = len(groups)
                    layer = rs.ObjectLayer(objectid)
                    layer = rs.LayerId(layer)
                    layer = rs.LayerName(layer, fullpath=False)
                    
                    s = socket.socket()                 # Create a socket object
                    host = socket.gethostname()         # Get local machine name
                    port = 2412                         # Reserve a port for your service.
                    s.connect((host, port))             #conecteer
                    s.send('ExportModelOption2'.encode())
                    s.recv(1024)
                    s.send(str(layer).encode())
                    s.recv(1024)
                    s.send(str(objectid).encode())
                    s.recv(1024)
                    if lengthgroups > 0:
                        i =0
                        while i < lengthgroups:
                            s.send(groups[i].encode())
                            s.recv(1024)
                            i = i +1
                        s.send("END".encode())
                    s.recv(1024)
                    filelocation = PathGeometry +"\\"+str(objectid)+ Format
                    print filelocation
                    Command = "-_Export " + filelocation + " _Enter " + "_Enter "
                    rs.Command(Command)
                    print "object has been saved"
                    s.send(str(filelocation).encode())
                    s.close()
                except: 
                    print "Export failed"
        else:
            print "No objects found"      




def Main():
    form = RhinoPluginUI()
    form.Owner = Rhino.UI.RhinoEtoApp.MainWindow
    form.Show()

if __name__ == "__main__":
    Main()