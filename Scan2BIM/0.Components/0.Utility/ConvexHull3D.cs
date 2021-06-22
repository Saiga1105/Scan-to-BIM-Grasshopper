using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ConvexHull3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConvexHull3D class.
        /// </summary>
        public ConvexHull3D()
          : base("Convexhull 3D", "Convexhull 3D",
              "Compute the contourpoints of a PointCloud",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("3D ConvexHull", "Mesh", "3D Convex hull mesh of the input point cloud", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = null;

            // read inputs
            if (!DA.GetData(0, ref pc)) return;

            // Exceptions
            if (pc.Value.Count <4) throw new Exception("Use point clouds with more than 3 points");

            Mesh mesh = pc.ComputeConvexHull3D();

            /// Output
            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_ConvexHull3D;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d8595a70-a130-4408-b475-f253f213d266"); }
        }
    }
}