using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Segmentation;
using System.Linq;
using Volvox_Instr;
using Volvox_Cloud;

namespace Scan2BIM
{
    public class ICP_pointclouds : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public ICP_pointclouds()
          : base("ICP_pointclouds", "RG",
              "Compute transformation matrix from point cloud A to B",
              "Saiga", "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// input mesh & segmentation parameters
            pManager.AddGeometryParameter("pc_moving", "Cloud_A", "Point cloud for which the transformation is computed (GH_cloud)", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddGeometryParameter("pc_fixed", "Cloud_B", "Point cloud that remains in place (GH_cloud)", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddNumberParameter("Metric", "Metric", " Metric that is used to minimize the distance between the two point clouds. 0: pointToPoint (default), 1: pointToPlane ", GH_ParamAccess.item, 0); pManager[2].Optional = true;
            pManager.AddNumberParameter("InlierRatio", "Inlier", "Percentage of inliers [0;1] that fall within the given Euclidean distance threshold e.g. 0.6", GH_ParamAccess.item, 1.0); pManager[3].Optional = true;
            pManager.AddNumberParameter("MaxIterations", "Iter", "Max iterations for ICP e.g. 100", GH_ParamAccess.item, 100); pManager[4].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput indices vertices mesh conform segmentation
            pManager.AddNumberParameter("tform", "T", "[16,1] transformation matrix ", GH_ParamAccess.list);
            //pManager.AddNumberParameter("rmse", "RMSE", "RMSE value of the Euclidean distance between both point clouds", GH_ParamAccess.list);

        }
 

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            PointCloud Rhino_Cloud1 = new PointCloud();
            PointCloud Rhino_Cloud2 = new PointCloud();

            /// initialise parameters
            double Metric = 0.0, InlierRatio = 1.0, MaxIterations = 60;

            ///import 
            if (!DA.GetData("pc_moving", ref Rhino_Cloud1)) return;
            if (!DA.GetData("pc_fixed", ref Rhino_Cloud2)) return;
            if (!DA.GetData("Metric", ref Metric)) return;
            if (!DA.GetData("InlierRatio", ref InlierRatio)) return;
            if (!DA.GetData("MaxIterations", ref MaxIterations)) return;

            /// 1. decompose points into doubles (rhinocommon => .NET)
            /// 2. create appropriete matlab arrays (.NET => Matlab)
            /// 3. run matlab function (Matlab)
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. output data


            /// 1. 
            var Rhino_xyz1 = Rhino_Cloud1.GetPoints();
            var Rhino_xyz2 = Rhino_Cloud2.GetPoints();

            List<double> xyz1 = new List<double>();
            List<double> xyz2 = new List<double>();

            for (int i = 0; i < Rhino_Cloud1.Count; i++)
            {
                xyz1.Add(Rhino_xyz1[i].X);
                xyz1.Add(Rhino_xyz1[i].Y);
                xyz1.Add(Rhino_xyz1[i].Z);
            }
            var Matlab_xyz1 = new MWNumericArray(Rhino_Cloud1.Count, 3, xyz1.ToArray());

            for (int j = 0; j < Rhino_Cloud2.Count; j++)
            {
                xyz2.Add(Rhino_xyz2[j].X);
                xyz2.Add(Rhino_xyz2[j].Y);
                xyz2.Add(Rhino_xyz2[j].Z);
            }
            var Matlab_xyz2 = new MWNumericArray(Rhino_Cloud2.Count, 3, xyz2.ToArray());
            /// 3.
            Segmentation.segment segment_mesh = new Segmentation.segment();
            //MWArray[] Result = new MWArray[2];
            MWArray cluster = new MWNumericArray();
            cluster = segment_mesh.G_ICP(Matlab_xyz1, Matlab_xyz2, Metric, InlierRatio, MaxIterations);

            /// 4.
            MWNumericArray na = (MWNumericArray)cluster;
            double[] dc = (double[])na.ToVector(0);

            /// 5.
            var Rhino_param = new List<double>(dc);

            /// 6.
            DA.SetDataList(0, Rhino_param);

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
            get { return new Guid("8a568810-210d-41b7-b78e-19adc0485f7e"); }
        }
    }
}