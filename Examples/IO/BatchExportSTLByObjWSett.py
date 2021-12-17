"""Exports one STL file per object selected, export folder is same as current
folder if file has been saved; otherwise, choice of file name/location.
Choice of a four "standard" settings for export, inch or metric.
Script by Mitch Heynick 04.09.15"""

import rhinoscriptsyntax as rs
import scriptcontext as sc
import os

def GetSTLSettings(ang,ar,dist,grid,den,maxL=0,minL=.0001):
    e_str = "_ExportFileAs=_Binary "
    e_str+= "_ExportUnfinishedObjects=_Yes "
    e_str+= "_UseSimpleDialog=_No "
    e_str+= "_UseSimpleParameters=_No "
    e_str+= "_Enter _DetailedOptions "
    e_str+= "_JaggedSeams=_No "
    e_str+= "_PackTextures=_No "
    e_str+= "_Refine=_Yes "
    e_str+= "_SimplePlane=_Yes "
    e_str+= "_AdvancedOptions "
    e_str+= "_Angle={} ".format(ang)
    e_str+= "_AspectRatio={} ".format(ar)
    e_str+= "_Distance={} ".format(dist)
    e_str+= "_Density={} ".format(den)
    e_str+= "_Grid={} ".format(grid)
    e_str+= "_MaxEdgeLength={} ".format(maxL)
    e_str+= "_MinEdgeLength={} ".format(minL)
    e_str+= "_Enter _Enter"
    return e_str
    
def BatchExportSTLByObject():
    #get unit system
    us=rs.UnitSystem()
    if us != 2 and us!= 8:
        msg="Unsupported file units detected for STL.\n"
        msg+="Please use either inches or millimeters."
        rs.MessageBox(msg)
        return
    
    msg="Select objects to export as individual .stl files"
    objs = rs.GetObjects(msg, 8 + 16 + 32,preselect=True)
    if not objs : return
    doc_name=sc.doc.Name
    
    filt = "STL Files (*.stl)|*.stl||"
    if not doc_name:
        #document hasn't been saved
        filename=rs.SaveFileName("Main file name/folder for STL export?", filt)
        if filename==None: return
    else:
        #document has been saved, get path
        msg="Folder for export? (Enter to save in current folder)"
        folder = rs.BrowseForFolder(rs.WorkingFolder(), msg)
        if not folder: return
        filename=os.path.join(folder,doc_name)
    
    #numerical settings:(angle,aspect,dist,grid,density,max_edgelen,min_edgelen)
    coarse_mm=[30,0,0.03,16,0,0,0.01]
    coarse_in=[30,0,0.001,16,0,0,0.0005]
    medium_mm=[15,0,0.01,32,0,0,0.001]
    medium_in=[15,0,0.0005,32,0,0,0.00005]
    fine_mm=[5,0,0.005,64,0,0,0.0001]
    fine_in=[5,0,0.0003,64,0,0,0.00001]
    extrafine_mm=[2,0,0.0025,64,0,0,0.0001]
    extrafine_in=[2,0,0.0001,64,0,0,0.00001]
    
    #user select settings
    C='Coarse' ; M='Medium' ; F='Fine   ' ; XF='Extrafine'
    if us==2:
        exs=[C+' (0.03mm)',M+' (0.01mm)',F+' (0.005mm)',XF+' (0.0025mm)']
    else:
        exs=[C+' (.001")',M+' (.0005")',F+' (.0003")',XF+' (.0001")']
    
    msg="Choose export parameters"
    ex_choice=rs.ListBox(exs,msg,"STL export",exs[2])
    if ex_choice is None: return
    if ex_choice==exs[0]:
        if us==2: sett_str=GetSTLSettings(*coarse_mm)
        else: sett_str=GetSTLSettings(*coarse_in)
    elif ex_choice==exs[1]:
        if us==2: sett_str=GetSTLSettings(*medium_mm)
        else: sett_str=GetSTLSettings(*medium_in)
    elif ex_choice==exs[2]:
        if us==2: sett_str=GetSTLSettings(*fine_mm)
        else: sett_str=GetSTLSettings(*fine_in)
    elif ex_choice==exs[3]:
        if us==2: sett_str=GetSTLSettings(*extrafine_mm)
        else: sett_str=GetSTLSettings(*extrafine_in)
    
    #start the export sequence
    rs.EnableRedraw(False)
    for i, obj in enumerate(objs):
        e_file_name = "{}-{}.stl".format(filename[:-4], str(i+1))
        print e_file_name
        rs.UnselectAllObjects()
        rs.SelectObject(obj)
        rs.Command('-_Export "{}" {} _Enter'.format(e_file_name,sett_str), True)
    rs.EnableRedraw(True)

BatchExportSTLByObject()