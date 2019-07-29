using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class Region_Growing_PCD1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Region_Growing_PCD1()
          : base("Region_Growing_PCD1", "RG_PCD",
              "Seperate a mesh according to component primitives incl. planes and fluent surfaces",
              "Saiga", "Segmentation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point_Cloud", "PCD", "Rhino Point Cloud Geometry (x,y,z) with optional colors and normals", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("ThresValN", "T_N", "Threshold normal, default 30° deviation ", GH_ParamAccess.item, 30.0); pManager[1].Optional = true;
            pManager.AddNumberParameter("ThresValC", "T_C", "Threshold color, default 30 over all channels ", GH_ParamAccess.item, 30.0); pManager[2].Optional = true;
            pManager.AddNumberParameter("MaxDist", "D", "Distance Search Area, default is dynamic starting at r=0.1m ", GH_ParamAccess.item, 0.1); pManager[3].Optional = true;
            pManager.AddNumberParameter("Minsize", "M", "Minimum cluster size, default 2000 points ", GH_ParamAccess.item, 2000.0); pManager[4].Optional = true;
            pManager.AddNumberParameter("Offset", "T_copl", "Threshold coplanarity, default 0.05m ", GH_ParamAccess.item, 0.05); pManager[5].Optional = true;
            pManager.AddNumberParameter("Tilesize", "T_s", "Tilesize for parallel processing ", GH_ParamAccess.item, 200000); pManager[6].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("indices", "i", "Output segmented vertex indices", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            PointCloud Rhino_Cloud = new PointCloud();
            List<double> Rhino_indices = new List<double>();

            /// initialise internal parameters
            double T_N = 0.0, T_C = 0.0, D = 0.0, M = 0.0, O = 0.0, T_s = 0.0;


            if (!DA.GetData("Point_Cloud", ref Rhino_Cloud)) return;
            if (!DA.GetData("ThresValN", ref T_N)) return;
            if (!DA.GetData("ThresValC", ref T_C)) return;
            if (!DA.GetData("MaxDist", ref D)) return;
            if (!DA.GetData("Minsize", ref M)) return;
            if (!DA.GetData("Offset", ref O)) return;
            if (!DA.GetData("Tilesize", ref T_s)) return;

            /// 1. decompose PCD into vertices, normals (rhinocommon => .NET)
            /// 1a. retrieve/construct colors
            /// 2. create appropriete matlab arrays (.NET => Matlab)
            /// 3. run matlab function (Matlab)
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. output data

            var Rhino_xyz = Rhino_Cloud.GetPoints();
            var Rhino_n = Rhino_Cloud.GetNormals();
            var Rhino_c = Rhino_Cloud.GetColors();

            List<double> xyz = new List<double>();
            List<double> n = new List<double>();
            List<double> c= new List<double>();
            MWNumericArray Matlab_n = new MWNumericArray();
            MWNumericArray Matlab_c = new MWNumericArray();

            for (int i = 0; i < Rhino_Cloud.Count; i++)
            {
                xyz.Add(Rhino_xyz[i].X);
                xyz.Add(Rhino_xyz[i].Y);
                xyz.Add(Rhino_xyz[i].Z);
            }
            

            if (Rhino_Cloud.ContainsNormals == true)
            {
                for (int i = 0; i < Rhino_Cloud.Count; i++)
                {
                    n.Add(Rhino_n[i].X);
                    n.Add(Rhino_n[i].Y);
                    n.Add(Rhino_n[i].Z);
                }
                Matlab_n = new MWNumericArray(Rhino_Cloud.Count, 3, n.ToArray());

            }


            if (Rhino_Cloud.ContainsColors == true)
            {
                for (int i = 0; i < Rhino_Cloud.Count; i++)
                {
                    c.Add(Rhino_c[i].R);
                    c.Add(Rhino_c[i].G);
                    c.Add(Rhino_c[i].B);
                }
                Matlab_c = new MWNumericArray(Rhino_Cloud.Count, 3, c.ToArray());
            }


            ///2.
            var Matlab_xyz = new MWNumericArray(Rhino_Cloud.Count, 3, xyz.ToArray());
            

            /// 3.
            Segmentation.segment segment_mesh = new Segmentation.segment();

            MWArray cluster = new MWNumericArray();
            cluster = segment_mesh.G_RegionGrowingNC2(Matlab_xyz, Matlab_n, Matlab_c, T_N, T_C, D, M, O, T_s);
 
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
        public override Guid ComponentGuid
        {
            get { return new Guid("fe5784b1-1b29-4f17-97b9-abced47ef696"); }
        }
    }
}