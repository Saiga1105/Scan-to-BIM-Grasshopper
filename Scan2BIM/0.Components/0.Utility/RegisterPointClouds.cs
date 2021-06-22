using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class RegisterPointClouds : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RegisterPointClouds class.
        /// </summary>
        public RegisterPointClouds()
          : base("Allign two Point Clouds", "ICP",
              "Compute the transformation matrix from point cloud A to B",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud A", "Cloud A", "Point cloud for which the transformation is computed (GH_cloud)", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud B", "Cloud B", "Point cloud that remains in place", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddNumberParameter("Metric", "Metric", " Metric that is used to minimize the distance between the two point clouds. 0: pointToPoint (default), 1: pointToPlane ", GH_ParamAccess.item, 0); pManager[2].Optional = true;
            pManager.AddNumberParameter("InlierRatio", "Inlier", "Percentage of inliers [0;1] that fall within the given Euclidean distance threshold e.g. 0.6", GH_ParamAccess.item, 0.6); pManager[3].Optional = true;
            pManager.AddNumberParameter("MaxIterations", "Iter", "Max iterations for ICP e.g. 100", GH_ParamAccess.item, 100); pManager[4].Optional = true;
            pManager.AddNumberParameter("Downsample A", "R0", "Percentage of points [0.01;0.99] to use for ICP e.g. 0.8", GH_ParamAccess.item, 0.1); pManager[5].Optional = true;
            pManager.AddNumberParameter("Downsample B", "R1", "Percentage of points [0.01;0.99] to use for ICP e.g. 0.8", GH_ParamAccess.item, 0.5); pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTransformParameter("transform", "T", "[4x4] transformation matrix ", GH_ParamAccess.item);
            pManager.AddNumberParameter("RMSE", "RMSE", "RMSE value [m] of the Euclidean distance between both point clouds", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Define i/o parameters
            GH_PointCloud pcA = null;
            GH_PointCloud pcB = null;
            double metric = 0.0, inlierRatio = 0.2, maxIterations = 20, downsamplingA = 1.0, downsamplingB = 1.0;

            // Read inputs
            if (!DA.GetData(0, ref pcA)) return;
            if (!DA.GetData(1, ref pcB)) return;
            if (!DA.GetData("Metric", ref metric)) return;
            if (!DA.GetData("InlierRatio", ref inlierRatio)) return;
            if (!DA.GetData("MaxIterations", ref maxIterations)) return;
            if (!DA.GetData("Downsample A", ref downsamplingA)) return;
            if (!DA.GetData("Downsample B", ref downsamplingB)) return;

            // Exceptions
            if (pcA.Value.Count <= 1 || pcB.Value.Count <= 1)throw new Exception("Use point clouds with more than 1 point");            
            if (downsamplingA < 0.01 || downsamplingA > 0.99)throw new Exception("Use downsampling between 0.01 and 0.99 for point cloud A");
            if (downsamplingB < 0.01 || downsamplingB > 0.99) throw new Exception("Use downsampling between 0.01 and 0.99 for point cloud A");

            if(metric==1 && !pcA.Value.ContainsNormals)
            {
                pcA.ComputeNormals();      
            }

            SpatialTools.RegisterPointClouds(out Transform transform, out Double rmse, ref pcA, ref pcB, metric, inlierRatio, maxIterations, downsamplingA, downsamplingB);

            DA.SetData(0, transform);
            DA.SetData(1, rmse);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_ICP1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C08-412D-8FF8-B76439197730"); }
        }
    }
}