using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

namespace Scan2BIM
{
    public class FitPlanarSurfaces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FitPlanarSurfaecs class.
        /// </summary>
        public FitPlanarSurfaces()
          : base("Fit planes to Geometry", "Fit Planes",
              "Compute an optional number of planar surfaces through a Mesh or Point Cloud ( if empty, the number of surfaces will be optimized automatically)",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Geometry", "Geometry", "Input Point Cloud or Mesh", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("nrPlanes", "n", "(optional) number of planar surfaces to fit to the point cloud", GH_ParamAccess.item,1.0); pManager[1].Optional = true;
            pManager.AddNumberParameter("Tolerance", "t", "(optional) tolerance for point inliers to the point cloud", GH_ParamAccess.item, 0.05); pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "s", "Best fit planar surface", GH_ParamAccess.list);
            pManager.AddNumberParameter("rmse", "rmse", "Root mean squared error of deviations from the used points to the planar surfaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = null;
            double nrPlanes = 1.0;
            double tolerance = 0.05;
            List<Brep> breps = null;
            List<double> rmse = new List<double>();

            // read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData(1, ref nrPlanes)) return;
            if (!DA.GetData(2, ref tolerance)) return;

            // include some more error catching here
            if (tolerance < 0 ) throw new Exception("Choose a value >0");
            if (!pc.IsValid || pc.Value.Count <2) throw new Exception("Invalid Point Cloud");
            try
            {
                pc.FitPlanarSurfaces(out breps, out rmse, (int)nrPlanes, tolerance);        
            }
            catch
            {
                throw new Exception("Something went wrong with the planefitting.");
            }

            /// Output
            DA.SetDataList(0, breps);
            DA.SetDataList(1, rmse);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_FitPlanarSurfaces;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("18bcdfec-5a0d-4285-ab71-03e7a9c9d08f"); }
        }
    }
}