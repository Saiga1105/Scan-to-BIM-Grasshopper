using System;
using System.Collections.Generic;
using System.Linq;
using Volvox_Cloud;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;

namespace Scan2BIM
{
    public class PCD_normals : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PCD_normals()
          : base("PCD_normals", "N",
              "Computes normals for point cloud if there aren't any yet",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("k", "k", "number of neighbors to consult for the normal estimation", GH_ParamAccess.item,6); pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point Cloud", "PCDn", "Point Cloud data with normals", GH_ParamAccess.item);
        }
        public class Size_Exception : Exception
        {
            public Size_Exception(string message) : base(message) { }

        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            PointCloud Rhino_Cloud = new PointCloud();
            Double k = 6; 

            /// read inputs
            if (!DA.GetData(0, ref Rhino_Cloud)) return;
            if (!DA.GetData("k", ref k)) return;


            if (Rhino_Cloud.Count <= k)
            {
                throw new Size_Exception(string.Format("Use point clouds with more than k points"));
            }

            /// interal parameters 
              List<Vector3d> normals = new List<Vector3d>();

            if (!Rhino_Cloud.ContainsNormals)
            {
                var X = Rhino_Cloud.Select(x => x.X).ToList();
                var Y = Rhino_Cloud.Select(x => x.Y).ToList();
                var Z = Rhino_Cloud.Select(x => x.Z).ToList();
                
                ///2.
                var Matlab_X = new MWNumericArray(Rhino_Cloud.Count, 1, X.ToArray());
                var Matlab_Y = new MWNumericArray(Rhino_Cloud.Count, 1, Y.ToArray());
                var Matlab_Z = new MWNumericArray(Rhino_Cloud.Count, 1, Z.ToArray());

                /// 3.
                Segmentation.segment segment_mesh = new Segmentation.segment();

                var mwca = (MWCellArray)segment_mesh.G_Normals(Matlab_X, Matlab_Y, Matlab_Z, k); 

                /// 4.
                MWNumericArray na0 = (MWNumericArray)mwca[1];
                double[] dc0 = (double[])na0.ToVector(0);

                MWNumericArray na1 = (MWNumericArray)mwca[2];
                double[] dc1 = (double[])na1.ToVector(0);

                MWNumericArray na2 = (MWNumericArray)mwca[3];
                double[] dc2 = (double[])na2.ToVector(0);


                /// 5.
                var Rhino_param0 = new List<double>(dc0);
                var Rhino_param1 = new List<double>(dc1);
                var Rhino_param2 = new List<double>(dc2);
                Vector3d R = new Vector3d();
                for (int i =0;i< Rhino_param0.Count;i++)
                {
                    R = new Vector3d(Rhino_param0[i], Rhino_param1[i], Rhino_param2[i]);
                    normals.Add(R);
                }

            }

            else
            {
                normals= Rhino_Cloud.GetNormals().ToList();
            }

            PointCloud Rhino_Cloud_out = new PointCloud();
            Rhino_Cloud_out.AddRange(Rhino_Cloud.GetPoints(), normals);
            GH_Cloud Rhino_Cloud_out2 = new GH_Cloud(Rhino_Cloud_out);

            /// 6.
            DA.SetData(0, Rhino_Cloud_out2);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                 return Properties.Resources.Icon_Normals;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e8b70f9e-2cc0-4a78-bcd7-1c520f873ec8"); }
        }
    }
}