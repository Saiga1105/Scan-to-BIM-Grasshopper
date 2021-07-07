using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class SplitPointCloudVectors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SplitPointCloudVectors class.
        /// </summary>
        public SplitPointCloudVectors()
          : base("Split Point Cloud", "Split PCD",
              "Split a point cloud based on its normals (up to tolerance)",
              "Saiga", "Segmentation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("Tolerance", "t", "(optional) tolerance for normal inliers to a cluster [radians]", GH_ParamAccess.item, Math.PI/36); pManager[1].Optional = true;
            pManager.AddNumberParameter("Distance", "d", "(optional) distance threshold for connected components of similarly oriented parts [m]", GH_ParamAccess.item, 0.1); pManager[2].Optional = true;
            pManager.AddBooleanParameter("Estimate Z?", "Z?", "(optional) Estimate up direction of point cloud based on PCA ?", GH_ParamAccess.item, false); pManager[3].Optional = true;
            // is this necessary?
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
            GH_PointCloud pc = null;
            double tolerance = Math.PI/36; //5°
            double distance = 0.1;
            bool computeZ = false;
            List<GH_PointCloud> pc_outs = new List<GH_PointCloud>();
            List<Vector3d> normals = new List<Vector3d>();

            // read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData(1, ref tolerance)) return;
            if (!DA.GetData(2, ref distance)) return;
            if (!DA.GetData(3, ref computeZ)) return;

            // some error catching here
            if (tolerance <0 || tolerance>Math.PI) throw new Exception("Choose a value between [0;Pi/2]");
            if (!pc.IsValid) throw new Exception("Invalid Point Cloud");

            // method
            pc.SegmentGaussianSphere(out pc_outs, distance,tolerance, computeZ);
            
            /// Output
            DA.SetDataList(0, pc_outs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_SplitByNormals;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3e993a24-9653-4649-9e6c-8682449920b7"); }
        }
    }
}