using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ImportPointCloud : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SamplePointCloud class.
        /// </summary>
        public ImportPointCloud()
          : base("Import E57 Point Cloud", "Import E57 Point Cloud",
              "Import an E57 Point Cloud from a file e.g. D:Data test.e57",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "f", "File path (e.g. D:Data\test.e57)", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("Percent", "%", "Percent of points to load [0;1]", GH_ParamAccess.item, 1.0); pManager[1].Optional = true;
            pManager.AddTextParameter("Local storage", "l", "Local file storage for out of core point clouds (e.g. C: tmp mystore)", GH_ParamAccess.item, "C:\\temp\\mystore"); pManager[2].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            string filename = "";
            double percent = 1.0;
            string temp_storage = "";

            // read inputs
            if (!DA.GetData(0, ref filename)) return;
            if (!DA.GetData(1, ref percent)) return;
            if (!DA.GetData(2, ref temp_storage)) return;

            // Exceptions
            if (!filename.EndsWith(".e57")) throw new Exception("Invalid Point Cloud. Enter a point cloud with .e57 extension.");
            if (percent <= 0 || percent > 1) throw new Exception("Percentage should be between 0 and 1.");
            if (!temp_storage.IsDirectoryWritable())
            {
                bool exists = System.IO.Directory.Exists(temp_storage);
                if (!exists)
                {
                    try
                    {
                    
                        System.IO.Directory.CreateDirectory(temp_storage);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Can't create local storage at this location."); ;
                    }
                }
                throw new Exception("You don't have access to this location."); 
            }
            var pc = filename.ReadPointCloud(temp_storage,percent); // is this percentage included?

            /// Output
            DA.SetDataList(0, pc);
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
            get { return new Guid("955ff797-fe44-4cfc-9aa8-2ce1461af7fd"); }
        }
    }
}