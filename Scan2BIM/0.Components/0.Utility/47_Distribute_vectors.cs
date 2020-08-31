using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Segmentation;
using System.Linq;
using Volvox_Instr;
using Volvox_Cloud;

namespace Scan2BIM
{
    public class Distribute_vectors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Distribute_vectors()
          : base("Distribute vectors", "N{X,Y,Z}",
              "Distribute vectors along cardinal axes. The method returns a list of vector indices for every axis",
              "Saiga", "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// input parameters
            pManager.AddVectorParameter("Vectors", "V", " Vector {X,Y,Z}", GH_ParamAccess.tree); pManager[0].Optional = false;
            pManager.AddBooleanParameter("Strict", "s", " 0:vectors can only be assigned to 1 axis, 1: Every vector within a 45°cone of an axis is considered ", GH_ParamAccess.item, false); pManager[1].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput indices vertices mesh conform segmentation
            pManager.AddIntegerParameter("X", "X", "Indices of vectors assigned to X-axis", GH_ParamAccess.tree);
            //pManager.AddIntegerParameter("Y", "Y", "Indices of vectors assigned to X-axis ", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Z", "Z", "Indices of vectors assigned to X-axis ", GH_ParamAccess.list);

        }
        public class Size_Exception : Exception
        {
            public Size_Exception(string message) : base(message) { }

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            GH_Structure<GH_Vector> Tree = new GH_Structure<GH_Vector>();
            bool s = false;


            /// initialise parameters
            List<int> X_i = new List<int>();
            List<int> Y_i = new List<int>();
            List<int> Z_i = new List<int>();
            GH_Path pth = null;
            Vector3d vector3D = new Vector3d();
            DataTree<int> indices = new DataTree<int>();


            ///import 
            if (!DA.GetDataTree("Vectors", out Tree)) return;
            if (!DA.GetData("Strict", ref s)) return;


            /// 0. error catching 
            /// 1. decompose datatree
            /// 2. convert GH_GOO structure to rhinocommon Vector3D
            /// 3. querry list
            /// 4. create new datatree
            /// 5. output data

            // most occuring value in a list
            //int test = 0; double t_h = 0.1;
            //List<int> lijst = new List<int>();
            //for (int k=0;k<W_i.Count;k++)
            //{
            //    test = X_i
            //      .Where(v => Math.Abs(v - X_i[k]) <= t_h)
            //      .Count();
            //    lijst.Add(test);
            //}
            //frequency = lijst;

            // efficient sum
            //int test = 0; double t_h = 0.1;
            List<Point3d> P1 = new List<Point3d>();
            List<Point3d> P2 = new List<Point3d>();
            List<Point3d> result = new List<Point3d>();
            //List<Double> x = new List<Double>();
            

            /// 1. decompose datatree
            for (int i = 0; i < Tree.PathCount; i++)
            {
                /// 2. convert GH_GOO structure to rhinocommon Vector3D
                pth = Tree.get_Path(i);
                var branch = Tree.Branches[i];
                List<Vector3d> V = new List<Vector3d>();

                foreach (GH_Vector thisGHVector in branch)
                {
                    GH_Convert.ToVector3d(thisGHVector, ref vector3D, 0);
                    V.Add(vector3D);
                }

                /// 3. querry list
                if (s == false)
                {
                    X_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].X) >= Math.Abs(V[index].Y) && Math.Abs(V[index].X) >= Math.Abs(V[index].Z))
                    .ToList();

                    Y_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Y) >= Math.Abs(V[index].X) && Math.Abs(V[index].Y) >= Math.Abs(V[index].Z))
                    .ToList();

                    Z_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Z) >= Math.Abs(V[index].X) && Math.Abs(V[index].Z) >= Math.Abs(V[index].Y))
                    .ToList();
                }
                else
                {
                    var a = 1 / Math.Sqrt(3);
                    X_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].X) >= a)
                    .ToList();

                    Y_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Y) >= a)
                    .ToList();

                    Z_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Z) >= a)
                    .ToList();
                }
               

                /// 4. create new datatree
                var path = pth.Indices.ToList();
                path.Add(0);
                var p = new GH_Path(path.ToArray());
                indices.AddRange(X_i, p);

                path = pth.Indices.ToList();
                path.Add(1);
                p = new GH_Path(path.ToArray());
                indices.AddRange(Y_i, p);

                path = pth.Indices.ToList();
                path.Add(2);
                p = new GH_Path(path.ToArray());
                indices.AddRange(Z_i, p);
            }



            /// 5.
            DA.SetDataTree(0, indices);

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
            get { return new Guid("8a667810-210d-41b7-b78e-19adc2485f7e"); }
        }
    }
}