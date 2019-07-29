using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Collections;
using Segmentation;
using System.Linq;

namespace Scan2BIM
{
    public class Region_Growing_Mesh_Normals1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Region_Growing_Mesh_Normals1()
          : base("Region_Growing_Mesh_Normals1", "RG",
              "Seperate a mesh based on normals. Computes component indices of primitives incl. planes and fluent surfaces",
              "Saiga", "Segmentation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// input mesh & segmentation parameters
            pManager.AddMeshParameter("Mesh", "M", "Mesh ", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("ThresValN", "T_N", "Threshold normal, default 30° deviation ", GH_ParamAccess.item, 30.0); pManager[1].Optional = true;
            pManager.AddNumberParameter("ThresValC", "T_C", "Threshold color, default 30 over all channels ", GH_ParamAccess.item, 30.0); pManager[2].Optional = true;
            pManager.AddNumberParameter("MaxDist", "D", "Distance Search Area, default is dynamic starting at r=0.1m ", GH_ParamAccess.item, 0.1); pManager[3].Optional = true;
            pManager.AddNumberParameter("Minsize", "M", "Minimum cluster size, default 2000 points ", GH_ParamAccess.item, 2000.0); pManager[4].Optional = true;
            pManager.AddNumberParameter("Offset", "T_copl", "Threshold coplanarity, default 0.05m ", GH_ParamAccess.item, 0.05); pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput indices vertices mesh conform segmentation
            pManager.AddNumberParameter("face indices", "i", "Output segmented vertex indices", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            Mesh Rhino_mesh = new Mesh();

            /// initialise parameters
            double T_N = 0.0, T_C = 0.0, D = 0.0, M = 0.0, O = 0.0;
            List<double> Rhino_indices = new List<double>();

            ///import data
            if (!DA.GetData("Mesh", ref Rhino_mesh)) return;
            if (!DA.GetData("ThresValN", ref T_N)) return;
            if (!DA.GetData("ThresValC", ref T_C)) return;
            if (!DA.GetData("MaxDist", ref D)) return;
            if (!DA.GetData("Minsize", ref M)) return;
            if (!DA.GetData("Offset", ref O)) return;

            ///get mesh data to Matlab
            /// 1. decompose mesh into face centroids/normals (rhinocommon => .NET)
            /// 1a. compute normals if not present
            /// 2. create appropriete matlab arrays (.NET => Matlab)
            /// 3. run matlab function (Matlab)
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. output data
            
            /// 1. 
            var Rhino_c = new Point3f();
            var Rhino_n = new Vector3f();
            var C_xyz = new float[Rhino_mesh.Faces.Count * 3];
            var C_n = new float[Rhino_mesh.Faces.Count * 3];

            ///1a. check if normals are present
            if (Rhino_mesh.FaceNormals.Count ==0)
                {
                Rhino_mesh.FaceNormals.ComputeFaceNormals();
            }
            
            /// 2.
            for (int i = 0; i < Rhino_mesh.Faces.Count; i++)
            {
                Rhino_c = (Point3f)Rhino_mesh.Faces.GetFaceCenter(i);
                C_xyz[i*3] = Rhino_c.X;
                C_xyz[i*3+1] = Rhino_c.Y;
                C_xyz[i * 3 + 2] = Rhino_c.Z;

                Rhino_n = Rhino_mesh.FaceNormals[i];
                C_n[i * 3] = Rhino_n.X;
                C_n[i * 3 + 1] = Rhino_n.Y;
                C_n[i * 3 + 2] = Rhino_n.Z;

            }
                       
            var Matlab_xyz = new MWNumericArray(Rhino_mesh.Faces.Count, 3, C_xyz);
            var Matlab_n = new MWNumericArray(Rhino_mesh.Faces.Count, 3, C_n);

            /// 3.
            Segmentation.segment segment_mesh = new Segmentation.segment();

            MWArray cluster = new MWNumericArray();
            cluster = segment_mesh.G_RegionGrowingNC2(Matlab_xyz, Matlab_n, T_N, T_C, D, M, O);

            /// 4.
            MWNumericArray na = (MWNumericArray)cluster;
            double[] dc = (double[])na.ToVector(0);

            /// 5.
            Rhino_indices = new List<double>(dc);

            /// 6.
            DA.SetDataList(0, Rhino_indices);

        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("07ae1d3c-4fea-4819-b9a8-7430423affe3"); }
        }
    }
}