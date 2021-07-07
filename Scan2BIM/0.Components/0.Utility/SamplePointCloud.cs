using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class SamplePointCloud : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SamplePointCloud class.
        /// </summary>
        public SamplePointCloud()
          : base("Sample Point Cloud", "Sample Point Cloud",
              "Subsample a point cloud according to a resolution (e.g. 0.05m)",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("resolution", "r", "resolution of the subsampling (e.g. a point every 0.05m)", GH_ParamAccess.item, 0.05); pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = null;
            double resolution = 0.05;

            // read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData(1, ref resolution)) return;

            // Exceptions
            if (!pc.IsValid) throw new Exception("Invalid Point Cloud");
            if (resolution <= 0.002) throw new Exception("This resolution is to small ");
            
            pc = pc.GenerateRandomCloud(resolution); // there is a problem here

            /// Output
            DA.SetData(0, pc);
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
            get { return new Guid("955ff797-fe44-4cfc-9aa8-2ce1461ae7fd"); }
        }
    }
}