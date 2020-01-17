using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace Scan2BIM
{
    public class Wallfit : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Wallfit()
          : base("Wallfit", "RG_PCD",
              "Compute best fit 2D wall line (line, arc, poly) to a set of points C with respect to contourpoints P and thickness dw",
              "Saiga", "Reconstruction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("C", "C", "List of 3D coordinates of centre points of the wall ", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddPointParameter("P", "P", "List of 3D coordinates of contour (surface) points of the wall ", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddIntegerParameter("k", "k", "Number of iterations for RANSAC fitting ", GH_ParamAccess.item,100); pManager[2].Optional = true;
            pManager.AddNumberParameter("t", "t", "threshold distance for the model inliers e.g. LOA30=[-0.015m;+0.015m]", GH_ParamAccess.item, 0.015); pManager[3].Optional = true;
            pManager.AddNumberParameter("dw", "dw", "Theoretical thickness of the wall e.g. 0.1m", GH_ParamAccess.item, 0.1); pManager[4].Optional = true;
            pManager.AddNumberParameter("w", "w", "Expected inlier percentage of C given t e.g. 0.7", GH_ParamAccess.item, 0.7); pManager[5].Optional = true;
            pManager.AddIntegerParameter("n", "n", "Number of control points for an n-dimensional polyline e.g. 4 (dependent on wall length)", GH_ParamAccess.item, 4); pManager[6].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Hearth_line", "G", "Computed lines, arc and n-D polylines", GH_ParamAccess.item);
            pManager.AddNumberParameter("inlrNum", "inlrNum", "inlrNum", GH_ParamAccess.item);
           // pManager.AddNumberParameter("error", "error", "error", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            List<Point3d> Rhino_C= new List<Point3d>();
            List<Point3d> Rhino_P = new List<Point3d>();

            /// initialise internal parameters
            int k = 100, n=4; double t = 0.015, w = 0.7,dw=0.1;

            if (!DA.GetDataList<Point3d>("C", Rhino_C)) return;
            if (!DA.GetDataList<Point3d>("P", Rhino_P)) return;
            if (!DA.GetData("k", ref k)) return;
            if (!DA.GetData("t", ref t)) return;
            if (!DA.GetData("dw", ref dw)) return;
            if (!DA.GetData("w", ref w)) return;
            if (!DA.GetData("n", ref n)) return;

            /// 1. decompose P,C into lists of coordinates (rhinocommon => .NET)
            /// 2. create appropriete matlab arrays (.NET => Matlab)
            /// 3. run matlab function (Matlab)
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. compute proper geometry based on n=2,3,...n
            /// 7. output data

            ///Point3d test = new Point3d();
            

            List<double> Cx = new List<double>();
            List<double> Cy = new List<double>();
            List<double> Cz = new List<double>();
            List<double> Px = new List<double>();
            List<double> Py = new List<double>();
            List<double> Pz = new List<double>();

            /// List<double> n = new List<double>();
            ///List<double> c= new List<double>();

            //MWNumericArray Matlab_Cx = new MWNumericArray();
            //MWNumericArray Matlab_Cy = new MWNumericArray();
            //MWNumericArray Matlab_Px = new MWNumericArray();
            //MWNumericArray Matlab_Py = new MWNumericArray();

            for (int i = 0; i < Rhino_C.Count; i++)
            {
                Cx.Add(Rhino_C[i].X);
                Cy.Add(Rhino_C[i].Y);
                Cz.Add(Rhino_C[i].Z);
            }
            for (int i = 0; i < Rhino_P.Count; i++)
            {
                Px.Add(Rhino_P[i].X);
                Py.Add(Rhino_P[i].Y);
                Pz.Add(Rhino_P[i].Z);

            }

            ///2.
            var Matlab_Cx= new MWNumericArray(Rhino_C.Count, 1, Cx.ToArray());
            var Matlab_Cy = new MWNumericArray(Rhino_C.Count, 1, Cy.ToArray());
            var Matlab_Px = new MWNumericArray(Rhino_P.Count, 1, Px.ToArray());
            var Matlab_Py = new MWNumericArray(Rhino_P.Count, 1, Py.ToArray());

            /// 3.
            Reconstruction.fittwall fitt_wall = new Reconstruction.fittwall();

            ///MWArray result = new MWNumericArray(4);
            var result = fitt_wall.G_fitwall(4, Matlab_Cx, Matlab_Cy, Matlab_Px, Matlab_Py, k, t, dw, w, n);

            /// 4. 
            MWNumericArray out1 = (MWNumericArray)result[0];
            double[] Mx = (double[])out1.ToVector(0);

            MWNumericArray out2 = (MWNumericArray)result[1];
            double[] My = (double[])out2.ToVector(0);

            MWNumericArray out3 = (MWNumericArray)result[2];
            double[] inlrNum = (double[])out3.ToVector(0);

            MWNumericArray out4 = (MWNumericArray)result[3];
            double[] error = (double[])out4.ToVector(0);


            /// 5.
            var Rhino_Mx = new List<double>(Mx);
            var Rhino_My = new List<double>(My);
            double Rhino_inlrNum = inlrNum[0];
            double Rhino_error = error[0];

            /// 6.
            List<Point3d> Rhino_M = new List<Point3d>();
            
            for (int i = 0; i < Rhino_Mx.Count; i++)
            {
                var temp = new Point3d(Rhino_Mx[i], Rhino_My[i], Cz.Min());
                Rhino_M.Add(temp);
            }

            if (Rhino_M.Count == 2)
            {
                var geometry = new Line(Rhino_M[0] , Rhino_M[1] );
                DA.SetData(0, geometry);
            }
            if (Rhino_Mx.Count == 3)
            {
                var geometry = new Arc(Rhino_M[0], Rhino_M[1], Rhino_M[2]);
                DA.SetData(0, geometry);
            }
             else
            {
                var geometry = new Polyline(Rhino_M);
                DA.SetData(0, geometry);
            }


            /// 7.
            DA.SetData(1, Rhino_inlrNum);
            //DA.SetData(2, Rhino_error);

        }
        //public interface Try_fitt_wall

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
            get { return new Guid("fe5784b1-1b29-4f17-97b9-abced47ef677"); }
        }
    }
}