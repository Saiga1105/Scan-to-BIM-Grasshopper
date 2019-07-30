using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Collections;
using Segmentation;
using System.Linq;
using System.Drawing;

namespace Scan2BIM
{
    public class Region_Growing_Mesh_NormalsColors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Region_Growing_Mesh_NormalsColors()
          : base("Region_Growing_Mesh_NormalsColors", "RG",
              "Seperate a mesh based on normals+colors. Computes component indices of primitives incl. planes and fluent surfaces",
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
            pManager.AddNumberParameter("Tilesize", "T_s", "Tilesize for parallel processing ", GH_ParamAccess.item, 200000); pManager[6].Optional = true;
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
            double T_N = 0.0, T_C = 0.0, D = 0.0, M = 0.0, O = 0.0,T_s=0.0;
            List<double> Rhino_indices = new List<double>();

            ///import data
            if (!DA.GetData("Mesh", ref Rhino_mesh)) return;
            if (!DA.GetData("ThresValN", ref T_N)) return;
            if (!DA.GetData("ThresValC", ref T_C)) return;
            if (!DA.GetData("MaxDist", ref D)) return;
            if (!DA.GetData("Minsize", ref M)) return;
            if (!DA.GetData("Offset", ref O)) return;
            if (!DA.GetData("Tilesize", ref T_s)) return;
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
            MeshFace Rhino_face = new MeshFace();

            var C_xyz = new float[Rhino_mesh.Faces.Count * 3];
            var C_n = new float[Rhino_mesh.Faces.Count * 3];
            var C_c = new float[Rhino_mesh.Faces.Count * 3];
            int r1, r2, r3, g1, g2, g3, b1, b2, b3, r, g, b;
            ///1a. check if normals are present
            if (Rhino_mesh.FaceNormals.Count == 0)
            {
                Rhino_mesh.FaceNormals.ComputeFaceNormals();
            }

            
            /// 2.
            for (int i = 0; i < Rhino_mesh.Faces.Count; i++)
            {
                Rhino_c = (Point3f)Rhino_mesh.Faces.GetFaceCenter(i);
                C_xyz[i * 3] = Rhino_c.X;
                C_xyz[i * 3 + 1] = Rhino_c.Y;
                C_xyz[i * 3 + 2] = Rhino_c.Z;

                Rhino_n = Rhino_mesh.FaceNormals[i];
                C_n[i * 3] = Rhino_n.X;
                C_n[i * 3 + 1] = Rhino_n.Y;
                C_n[i * 3 + 2] = Rhino_n.Z;

                Rhino_face = Rhino_mesh.Faces.GetFace(i);
                
                if (Rhino_mesh.VertexColors.Count >0) 
                {
                    r1 = Rhino_mesh.VertexColors[Rhino_face[0]].R;
                    g1 = Rhino_mesh.VertexColors[Rhino_face[0]].G;
                    b1 = Rhino_mesh.VertexColors[Rhino_face[0]].B;
                    r2 = Rhino_mesh.VertexColors[Rhino_face[1]].R;
                    g2 = Rhino_mesh.VertexColors[Rhino_face[1]].G;
                    b2 = Rhino_mesh.VertexColors[Rhino_face[1]].B;
                    r3 = Rhino_mesh.VertexColors[Rhino_face[2]].R;
                    g3 = Rhino_mesh.VertexColors[Rhino_face[2]].G;
                    b3 = Rhino_mesh.VertexColors[Rhino_face[2]].B;
                    r = (r1 + r2 + r3) / 3;
                    g = (g1 + g2 + g3) / 3;
                    b = (b1 + b2 + b3) / 3;
                    C_c[i * 3] = r;
                    C_c[i * 3 + 1] = g;
                    C_c[i * 3 + 2] = b;
                }
                
            }

            var Matlab_xyz = new MWNumericArray(Rhino_mesh.Faces.Count, 3, C_xyz);
            var Matlab_n = new MWNumericArray(Rhino_mesh.Faces.Count, 3, C_n);
            var Matlab_c = new MWNumericArray(Rhino_mesh.Faces.Count, 3, C_c);

            /// 3.
            Segmentation.segment segment_mesh = new Segmentation.segment();

            MWArray cluster = new MWNumericArray();
            cluster = segment_mesh.G_RegionGrowingNC2(Matlab_xyz, Matlab_n, Matlab_c, T_N, T_C, D, M, O,T_s);

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
            get { return new Guid("b1606c60-3e1d-4b45-8302-973152e04f50"); }
        }
    }
}