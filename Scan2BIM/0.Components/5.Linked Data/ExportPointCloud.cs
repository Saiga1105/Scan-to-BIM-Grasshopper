using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ExportPointClouds : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SamplePointCloud class.
        /// </summary>
        public ExportPointClouds()
          : base("Export Point Cloud", "Export PLY Point Cloud",
              "Export an Point Cloud as PLY e.g. D:Data test.ply",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<string> settings = new List<string>{
                " _ExportType=_Binary",
                " _VertexesAsDoubles=_Yes ",
                " _ExportNormals=_Yes ",
                " _ExportColors=_Yes ",
                " _ExportMaterial=_No "
                };
            pManager.AddTextParameter("File path", "f", "File path+name (e.g. D:Data test.ply)", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddTextParameter("Settings", "s", "List of settings for .obj export", GH_ParamAccess.list, settings); pManager[2].Optional = true;
            pManager.AddBooleanParameter("Run", "r", "Start export", GH_ParamAccess.item, false); pManager[3].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "o", "Report success or failure on the export", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            List<GH_PointCloud> gH_PointClouds = new List<GH_PointCloud>();
            string filename = "";
            List<string> settings = new List<string>();
            Boolean run = false;
            Boolean success = false;

            // read inputs
            if (!DA.GetData(0, ref filename)) return;
            if (!DA.GetDataList(1, gH_PointClouds)) return;
            if (!DA.GetDataList(2, settings)) return;
            if (!DA.GetData(3, ref run)) return;

            // Exceptions
            foreach (GH_PointCloud gH_PointCloud in gH_PointClouds)
            {
                if (!gH_PointCloud.IsValid) throw new Exception("Invalid Point Cloud");
            }            
            if (!filename.EndsWith(".ply")) throw new Exception("Invalid Point Cloud. Enter a location + name with .ply as extension."); 
            //if (names.Count != 0 || names.Count!=pc.Count) throw new Exception("Number of elements in Point Clouds and Names should be the same.");            
            if (!filename.IsValidFileName(true)) throw new Exception("filename contains invalid character.");
            //if (!names.Any())
            //{
            //    for (int i =0;i< pc.Count;i++)
            //    {
            //        names.Add(i.ToString());
            //    }
            //}
            if (run)
            {                               
                success = gH_PointClouds.ExportPointCloud(filename,settings);                 
            }

            /// Output
            DA.SetData(0, success);
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
            get { return new Guid("855ff797-fe44-4cfc-9ab8-2ce1361af7fd"); }
        }
    }
}