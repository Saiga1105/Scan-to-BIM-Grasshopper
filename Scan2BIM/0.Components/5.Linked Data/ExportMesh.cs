using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ExportMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SamplePointCloud class.
        /// </summary>
        public ExportMesh()
          : base("Export meshes", "Mesh to OBJ",
              "Export a list of meshes to a file as .obj e.g. D:Data test.obj",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<string> settings = new List<string>{ 
                " _Geometry=_Mesh ",
                " _EndOfLine=CRLF ",
                " _ExportRhinoObjectNames=_DoNotExportObjectNames ",
                " _ExportRhinoGroupOrLayerNames=_DoNotExportGroupNames ",
                " _ExportMeshTextureCoordinates=_Yes ",
                " _ExportMeshVertexNormals=_No ",
                " _CreateNGons=_No ",
                " _ExportMaterialDefinitions=_No ",
                " _YUp=_No ",
                " _WrapLongLines=Yes ",
                " _VertexWelding=_Welded ",
                " _WritePrecision=4 ",
                " _Enter ",
                " _DetailedOptions ",
                " _JaggedSeams=_No ",
                " _PackTextures=_No ",
                " _Refine=_Yes ",
                " _SimplePlane=_No ",
                " _AdvancedOptions ",
                " _Angle=50 ",
                " _AspectRatio=0 ",
                " _Distance=0.0",
                " _Density=0 ",
                " _Density=0.45 ",
                " _Grid=0 ",
                " _MaxEdgeLength=0 ",
                " _MinEdgeLength=0.0001 "
                };
            pManager.AddTextParameter("File path", "F", "File path (e.g. D:Data test.obj)", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddMeshParameter("Meshes", "G", "Meshes to export (Mesh or Brep)", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddTextParameter("Settings", "s", "List of settings for .obj export", GH_ParamAccess.list,settings); pManager[2].Optional = true;
            pManager.AddBooleanParameter("Run", "r", "Start export", GH_ParamAccess.item, false); pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "o", "Output", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            string filename = "";
            List<Mesh> meshes = new List<Mesh>();
            List<string> settings = new List<string>();
            Boolean success = false;
            Boolean run= false;


            // read inputs
            if (!DA.GetData(0, ref filename)) return;
            if (!DA.GetDataList(1, meshes)) return;
            if (!DA.GetDataList(2, settings)) return;
            if (!DA.GetData(3, ref run)) return;

            // Exceptions
            if (!filename.EndsWith(".obj")) throw new Exception("Invalid file path. Enter a filename with .obj extension.");
            if (!meshes.Any()) throw new Exception("Add at least one mesh.");
            if (!filename.IsValidFileName(true)) throw new Exception("filename contains invalid character.");

            if (run)
            {
                success = meshes.ExportMesh(filename, settings);
            }

            /// Output
            if (success) DA.SetData(0, "success");
            else DA.SetData(0, "failure");

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_SamplePointCloud;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("955ff697-fe44-5cfc-9aa8-2ce1461af7fd"); }
        }
    }
}