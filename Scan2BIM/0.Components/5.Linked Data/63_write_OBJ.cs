using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Scan2BIM
{
    public class _62_write_TTL_document : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public _62_write_TTL_document()
          : base("export OBJ with objects", "OBJ export",
              "Export a set of meshes with their name to a file location",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "M", "List of meshes will be stored as OBJ objects with their name", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddTextParameter("Names", "L", "(Optional) List of names that will be stored as the reference for the objects in the OBJ", GH_ParamAccess.list); pManager[1].Optional = true;
            pManager.AddTextParameter("File path", "P", "Complete output file location e.g. D:/mesh.obj", GH_ParamAccess.item); pManager[2].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "O", "Succes/Failure", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /// input / output parameters
            List<Mesh> meshes = new List<Mesh>();
            List<String> names = new List<String>();

            string filepath = "";
            string O = "";

            ///import data
            if (!DA.GetDataList("Meshes", meshes)) return;
            if (!DA.GetDataList("Names", names)) return;
            if (!DA.GetData("File path", ref filepath)) return;
            //if (!DA.GetDataList("Guid", GH_guids)) return;

            /// PROCESS information
            /// 0. merge meshes if necessary
            /// 1. create obj write options

            // PROBLEM: mesh doesn't have any colors/names/guids

            ///define local parameters
            //var doc = Rhino.RhinoDoc.ActiveDoc;
            var R_meshes = meshes;
            var R_names = names;
            var joinedmesh = new Mesh();
            var mesh = R_meshes[0];
            mesh.GetObjectData();

            MeshObject meshObject = new MeshObject();

            meshObject.Name = R_names[i];


            ///0.
            if (R_meshes.Count >1)
            {
                for (int i = 0; i < R_meshes.Count; i++)
                {
                    if (R_names !=null)
                    {

                        R_meshes[i].SetUserString(Name, R_names[i]);
                        
                    }
                    joinedmesh.Append(R_meshes[i]);
                }
            }
            else
            {
                joinedmesh = R_meshes[0];
                if (R_names != null)
                {
                    R_meshes[0].SetUserString(Name, R_names[0]);
                }
            }

            ///1.
            var file_options = new FileWriteOptions();
            file_options.WriteUserData = true;
            var obj_options = new FileObjWriteOptions(file_options);
            obj_options.ExportNormals =true;
            obj_options.ExportMaterialDefinitions=true;
            obj_options.ExportTcs = true;
            obj_options.ExportObjectNames = FileObjWriteOptions.ObjObjectNames.ObjectAsObject;

            var wfr = FileObj.Write(filepath, new Mesh[] { joinedmesh }, obj_options);

            if (wfr.ToString() == "Success")
                O = filepath;
            else
                O = "Failed";

            //ouput
            DA.SetDataList(0, O);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("acec5f57-028b-452f-99c5-3b931526566d"); }
        }
    }
}