using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class CropPointCloud : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CropPointCloud class.
        /// </summary>
        public CropPointCloud()
          : base("Crop Point Cloud", "Crop Point Cloud",
              "Crop a point cloud so only points within the Brep are kept",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddBrepParameter("Brep", "Brep", "Brep geometry that functions as containment for the Point Cloud", GH_ParamAccess.item); pManager[1].Optional = false;
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

            Brep brep = new Brep();

            // read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData(1, ref brep)) return;

            // Exceptions
            if (!pc.IsValid) throw new Exception("Invalid Point Cloud");
            if (!brep.IsSolid) throw new Exception("Provide a closed Brep");
            if (!brep.IsManifold) throw new Exception("Provide a manifold Brep");

            GH_PointCloud pc_out =pc.CropPointCloud(brep);

            /// Output
            DA.SetData(0, pc_out);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_CropPointCloud;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4dfb101b-da45-45d9-8e18-5194142a3d4b"); }
        }
    }
}