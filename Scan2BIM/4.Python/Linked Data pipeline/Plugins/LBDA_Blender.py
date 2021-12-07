import bpy
from bpy.props import EnumProperty
from bpy.types import Operator
from bpy.types import Panel
from bpy.types import Object
from bpy.utils import register_class, unregister_class
import socket
import os
from datetime import datetime



class LayoutDemoPanel(Panel):
    """Creates a Panel in the scene context of the properties editor"""
    bl_label = "Linked Building Data Application"
    bl_idname = "SCENE_PT_layout"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_category = "scene"

    def draw(self, context):
        layout = self.layout
        layout.operator('lbda.export_object', text='Add geometries').action = "EXPORTOBJECTS"
        layout.operator('lbda.export_object', text='Add geometries').action = "EXPORTMODEL"


class LBDA_Export_Objects(Operator):
    bl_idname = 'lbda.export_object'
    bl_label = 'Export Object'
    bl_description = 'Exporting object'
    bl_options = {'REGISTER', 'UNDO'}
    action: EnumProperty(
        items=[
            ('EXPORTOBJECTS', 'export some objects', 'export some objects'),
            ('EXPORTMODEL', 'export model', 'export model')
        ]
    )
    def execute(self, context):
        if self.action == 'EXPORTOBJECTS':
            self.export_some_objects(context=context)
        if self.action == 'EXPORTMODEL':
            self.export_model(context=context)
        return {'FINISHED'}
    def AskFormat(self):
        s = socket.socket()                 # Create a socket object
        host = socket.gethostname()         # Get local machine name
        port = 2412                         # Reserve a port for your service.
        s.connect((host, port))             #conecteer
        s.send('FORMAT'.encode())
        preferedformat = s.recv(1024).decode()
        s.close()
        print(preferedformat)
        return preferedformat
    
    @staticmethod        
    def export_some_objects(context):
        selection_names = bpy.context.selected_objects
        print(selection_names)
        for name in selection_names:
            print(name)
            information = name.name
            print(information)
    
        s = socket.socket()                 # Create a socket object
        host = socket.gethostname()         # Get local machine name
        port = 2412
        s.connect((host, port))             #conecteer
        s.send('AddNewGeometry'.encode())
        PathGeometry = s.recv(1024).decode()
        s.send(str(selection_names[0].name).encode())
        Format = s.recv(1024).decode()
        s.send("gom:Blender_v2_82".encode())
        s.recv(1024)
        try:
            os.mkdir(PathGeometry)
        except OSError:
            print("Creation of the directory %s failed" % PathGeometry)
        else:
            print("Successfully created the directory %s " % PathGeometry)
        filelocation = PathGeometry +"\\"+str(selection_names[0].name)+ datetime.now().strftime('%H%M%d%m%Y')+ Format
        print(filelocation)
        if Format == ".obj":
            bpy.ops.export_scene.obj(filepath=filelocation, check_existing=True, filter_glob="*.obj;*.mtl", axis_forward='-Z', axis_up='Y')
            s.send(str(filelocation).encode())
        if Format == '.dae':
            bpy.ops.wm.collada_export(filepath=filelocation, hide_props_region=True, check_existing=True, filter_blender=False, filter_backup=False, filter_image=False, filter_movie=False, filter_python=False, filter_font=False, filter_sound=False, filter_text=False, filter_archive=False, filter_btx=False, filter_collada=True, filter_alembic=False, filter_usd=False, filter_folder=True, filter_blenlib=False, filemode=8, display_type='DEFAULT', sort_method='FILE_SORT_ALPHA', prop_bc_export_ui_section='main', apply_modifiers=False, export_mesh_type=0, export_mesh_type_selection='view', export_global_forward_selection='Y', export_global_up_selection='Z', apply_global_orientation=False, selected=False, include_children=False, include_armatures=False, include_shapekeys=False, deform_bones_only=False, include_animations=True, include_all_actions=True, export_animation_type_selection='sample', sampling_rate=1, keep_smooth_curves=False, keep_keyframes=False, keep_flat_curves=False, active_uv_only=False, use_texture_copies=True, triangulate=True, use_object_instantiation=True, use_blender_profile=True, sort_by_name=False, export_object_transformation_type=0, export_object_transformation_type_selection='matrix', export_animation_transformation_type=0, export_animation_transformation_type_selection='matrix', open_sim=False, limit_precision=False, keep_bind_info=False)
            s.send(str(filelocation).encode())
        if Format =='.ply':
            bpy.ops.export_mesh.ply(filepath= filelocation, check_existing=True, filter_glob="*.ply", use_selection=True, use_mesh_modifiers=True, use_normals=True, use_uv_coords=True, use_colors=True, global_scale=1.0, axis_forward='Y', axis_up='Z')
            s.send(str(filelocation).encode())
        else:
            print("Format not supported")
            s.send("Not Supported Format".encode())
        
        s.close()
    @staticmethod        
    def export_model(context):
        Format = self.AskFormat()
        s = socket.socket()                 # Create a socket object
        host = socket.gethostname()         # Get local machine name
        port = 2412                         # Reserve a port for your service.
        s.connect((host, port))             #conecteer
        s.send('ExportModelSTEP1'.encode())
        ProjectDirectory = s.recv(1024).decode()
        print(ProjectDirectory)
        PathGeometry = str(ProjectDirectory)+"\\Geometry"
        print(PathGeometry)
        s.send("OK".encode())
        ExportLOD = s.recv(1024).decode()
        print(ExportLOD)
        s.close()
        
        Groups = bpy.data.collections()
        print(Groups)
        if Groups:
            for group in Groups:
                print(group)
                groupedObjects = group.all_objects
                print(groupedObjects)
                #layer = self.DetermineLayerOfGroup(group)
                #print(layer)
        
        

def register():
    bpy.utils.register_class(LayoutDemoPanel)
    bpy.utils.register_class(LBDA_Export_Objects)
    


def unregister():
    bpy.utils.unregister_class(LayoutDemoPanel)
    bpy.utils.unregister_class(LBDA_Export_Objects)


if __name__ == "__main__":
    register()
