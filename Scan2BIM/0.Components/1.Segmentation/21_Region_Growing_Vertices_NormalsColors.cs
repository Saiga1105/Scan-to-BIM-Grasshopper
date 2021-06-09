using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Scan2BIM_Matlab;
using System.Linq;
using System.Drawing;


namespace Scan2BIM
{
    public class Region_Growing_Vertices_NormalsColors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Region_Growing_Vertices_NormalsColors()
          : base("Region_Growing_Vertices_NormalsColors", "RG",
              "Segment a set of 3Dpoints based on normals according to component primitives incl. planes and fluent surfaces (parallel processing enabled)",
              "Saiga", "Segmentation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// input mesh & segmentation parameters
            pManager.AddPointParameter("Points", "M", "Mesh Geometry", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddColourParameter("Colors", "M", "Mesh Geometry", GH_ParamAccess.list); pManager[0].Optional = true;
            pManager.AddNumberParameter("ThresValN", "T_N", "Threshold normal, default 30° deviation ", GH_ParamAccess.item, 30.0); pManager[1].Optional = true;
            pManager.AddNumberParameter("MaxDist", "D", "Distance Search Area, default is dynamic starting at r=0.1m ", GH_ParamAccess.item, 0.1); pManager[3].Optional = true;
            pManager.AddNumberParameter("Minsize", "M", "Minimum cluster size, default 2000 points ", GH_ParamAccess.item, 2000.0); pManager[4].Optional = true;
            pManager.AddNumberParameter("Offset", "T_copl", "Threshold coplanarity, default 0.05m ", GH_ParamAccess.item, 0.05); pManager[5].Optional = true;
            pManager.AddNumberParameter("Tilesize", "T_s", "Tilesize for parallel processing ", GH_ParamAccess.item, 200000); pManager[6].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput indices vertices mesh conform segmentation
            pManager.AddNumberParameter("indices", "i", "Output segmented vertex indices", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            List<Point3d> Rhino_points = new List<Point3d>();
            List<Color> Rhino_Color = new List<Color>();


            /// initialise parameters
            double T_N = 0.0, T_C = 0.0, D = 0.0, M = 0.0, O = 0.0, T_s = 0.0;
            List<double> Rhino_indices = new List<double>();
            List<double> Rhino_xyz = new List<double>();
            List<double> Rhino_c = new List<double>();


            ///import data
            if (!DA.GetDataList("Points", Rhino_points)) return;
            if (!DA.GetDataList("Colors", Rhino_Color)) return;
            if (!DA.GetData("ThresValN", ref T_N)) return;
            if (!DA.GetData("MaxDist", ref D)) return;
            if (!DA.GetData("Minsize", ref M)) return;
            if (!DA.GetData("Offset", ref O)) return;
            if (!DA.GetData("Tilesize", ref T_s)) return;

            /// 1. decompose points into doubles (rhinocommon => .NET)
            /// 2. create appropriete matlab arrays (.NET => Matlab)
            /// 3. run matlab function (Matlab)
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. output data

            /// 1. 
            for (int i = 0; i < Rhino_points.Count; i++)
            {
                Rhino_xyz[i * 3] = Rhino_points[i].X;
                Rhino_xyz[i * 3 + 1] = Rhino_points[i].Y;
                Rhino_xyz[i * 3 + 2] = Rhino_points[i].Z;

                if (Rhino_Color.Count >0)
                {
                    Rhino_c[i * 3] = Rhino_Color[i].R;
                    Rhino_c[i * 3 + 1] = Rhino_Color[i].G;
                    Rhino_c[i * 3 + 2] = Rhino_Color[i].B;
                }
            }

            /// 2.
            var Matlab_xyz = new MWNumericArray(Rhino_points.Count, 3, Rhino_xyz.ToArray());
            var Matlab_n = new MWNumericArray();
            var Matlab_c = new MWNumericArray();
            if (Rhino_Color.Count > 0)
            {
                Matlab_c= new MWNumericArray(Rhino_points.Count, 3, Rhino_c.ToArray());
            }


            /// 3.
            Scan2BIM_Matlab.Segmentation segmentation = new Scan2BIM_Matlab.Segmentation();

            MWArray cluster = new MWNumericArray();
            cluster = segmentation.S2B_RegionGrowing_2(Matlab_xyz, Matlab_n, Matlab_c, T_N, T_C,  D, M, O, T_s);

            /// 4.
            MWNumericArray na = (MWNumericArray)cluster;
            double[] dc = (double[])na.ToVector(0);

            /// 5.
            Rhino_indices = new List<double>(dc);

            /// 6.
            DA.SetDataList(0, Rhino_indices);

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
        public override Guid ComponentGuid => new Guid("37A8C46F-CB5B-49FD-A483-B03D1FE14A26");
    }
}