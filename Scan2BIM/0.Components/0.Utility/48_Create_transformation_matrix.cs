using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace Scan2BIM
{
    public class Create_transformation_matrix : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Create_transformation_matrix()
          : base("Create transformation matrix", "Create",
              "Create [16,1] transformation matrix from translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]",
              "Saiga", "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// component input parameters
            pManager.AddVectorParameter("R", "R", "rotation {Rx,Ry,Rz}  ", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddVectorParameter("t", "T", "translation {X,Y,Z}", GH_ParamAccess.item); pManager[1].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// component output parameters
            pManager.AddNumberParameter("tform", "T", " Rigid Transformation matrix [16,1] ", GH_ParamAccess.list);
        }
        public class Size_Exception : Exception
        {
            public Size_Exception(string message) : base(message) { }
           
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            Vector3d R = new Vector3d();
            Vector3d t = new Vector3d();

            /// initialise parameters
            List<double> tform = new List<double>();
            double a = 0.0, b = 0.0, c = 0.0, x = 0.0, y = 0.0, z = 0.0;
            ///import 
            if (!DA.GetData("R", ref R)) return;
            if (!DA.GetData("t", ref t)) return;


            /// 0. error catching 
            /// 1. convert R and t to tform
            /// 2. output data

            ///0.

            /// 1. 
             a = R.Z;
             b = R.Y;
             c = R.X;
             x = t.X;
             y = t.Y;
             z = t.Z;

            /// rigid body transformation matrix
            //var Ex = Math.Cos(a) * Math.Cos(b) * x + (Math.Cos(a) * Math.Sin(b) * Math.Sin(c) - Math.Sin(a) * Math.Cos(c)) * y + (Math.Cos(a) * Math.Sin(b) * Math.Cos(c) + Math.Sin(a) * Math.Sin(c)) * z + t[0] - x;
            //var Ey = Math.Sin(a) * Math.Cos(b) * x + (Math.Sin(a) * Math.Sin(b) * Math.Sin(c) + Math.Cos(a) * Math.Cos(c)) * y + (Math.Sin(a) * Math.Sin(b) * Math.Cos(c) - Math.Cos(a) * Math.Sin(c)) * z + t[1] - y;
            //var Ez = -Math.Sin(b) * x + Math.Cos(b) * Math.Sin(c) * y + Math.Cos(b) * Math.Cos(c) * z + t[2] - z;

            /// transformation
            tform.Add(Math.Cos(a) * Math.Cos(b));
            tform.Add((Math.Cos(a) * Math.Sin(b) * Math.Sin(c) - Math.Sin(a) * Math.Cos(c)));
            tform.Add((Math.Cos(a) * Math.Sin(b) * Math.Cos(c) + Math.Sin(a) * Math.Sin(c)));
            tform.Add(x);

            tform.Add(Math.Sin(a) * Math.Cos(b));
            tform.Add((Math.Sin(a) * Math.Sin(b) * Math.Sin(c) + Math.Cos(a) * Math.Cos(c)));
            tform.Add((Math.Sin(a) * Math.Sin(b) * Math.Cos(c) - Math.Cos(a) * Math.Sin(c)));
            tform.Add(y);

            tform.Add(-Math.Sin(b));
            tform.Add(Math.Cos(b) * Math.Sin(c));
            tform.Add(Math.Cos(b) * Math.Cos(c));
            tform.Add(z);

            tform.Add(0.0);
            tform.Add(0.0);
            tform.Add(0.0);
            tform.Add(1.0);

            /// 2.
            DA.SetDataList(0, tform);


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
            get { return new Guid("8a667410-210d-41b7-b78e-19adc0485f7e"); }
        }
    }
}