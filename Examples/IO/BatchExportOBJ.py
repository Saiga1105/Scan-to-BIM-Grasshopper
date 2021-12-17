import os
import scriptcontext
import rhinoscriptsyntax as rs


print "//export run started/////////////"

# this function via mcneel/rhinoscriptsyntax
#https://github.com/mcneel/rhinoscriptsyntax/blob/master/Scripts/rhinoscript/layer.py
def layerNames(sort=False):
    rc = []
    for layer in scriptcontext.doc.Layers:
        if not layer.IsDeleted: rc.append(layer.FullPath)
    if sort: rc.sort()
    return rc


fileName = rs.DocumentName()
filePath = rs.DocumentPath().rstrip(fileName)
extension = ".obj"

arrLayers = layerNames(False)


def initExport():
    for layerName in arrLayers:
        layer = scriptcontext.doc.Layers.FindByFullPath(layerName, True)
        if layer >= 0:
            layer = scriptcontext.doc.Layers[layer]
            if layer.IsVisible and rs.IsLayerEmpty(layerName) == False:
                cutName = layerName.split("::")
                cutName = cutName[len(cutName)-1]
                objs = scriptcontext.doc.Objects.FindByLayer(cutName)
                if len(objs) > 0:
                    saveObjectsToFile(cutName, objs)

def saveObjectsToFile(name, objs):
    if len(objs) > 0:
        rs.UnselectAllObjects()
        for obj in objs:
            obj.Select(True)
        scriptcontext.doc.Views.Redraw()
        name = "".join(name.split(" "))
        rs.Command("_-Export "+filePath+name+extension+" _Enter PolygonDensity=1 _Enter")

initExport()

print "//export run ended/////////////"