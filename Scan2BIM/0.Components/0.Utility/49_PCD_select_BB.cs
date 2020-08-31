using System;
using System.Collections.Generic;
using System.Linq;
using Volvox_Cloud;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;

namespace Scan2BIM
{
    public class PCD_select_BB : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PCD_select_BB()
          : base("PCD_select_BB", "BB",
              "Select all point in a BoundingBox ",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddGeometryParameter("Bounding Box", "BB", "Bounding Box", GH_ParamAccess.list); pManager[1].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point Cloud", "PCD", " selected point clouds per Bounding Box", GH_ParamAccess.list);
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
            PointCloud PCD_in = new PointCloud();
            List < BoundingBox> BB_in = new List<BoundingBox>();

            /// read inputs
            if (!DA.GetData(0, ref PCD_in)) return;
            if (!DA.GetDataList(1, BB_in)) return;


            if (PCD_in.Count <= 1)
            {
                throw new Size_Exception(string.Format("Use point clouds with more than 1 point"));
            }

            /// interal parameters 


            /// 0. create for loop for all BB's
            /// 1. create for loop for all PCD's
            /// 2. extract BB dimensions and PCD XYZ
            /// 3. sort list/ select relevant indices
            /// 4. create new list of PCD's
            /// 5. output
            /// 

            Point3d[] XYZ = PCD_in.GetPoints();
            var PCD2 = XYZ.OrderBy((Point3d x ) => x.X).ThenBy((Point3d x) => x.Y).ThenBy((Point3d x) => x.Z);


            for (int i = 0; i < BB_in.Count; i++)
            {

                for (int j=0;j<PCD_in.Count;j++)
            {
                    
            }

            }

            X_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].X) >= Math.Abs(V[index].Y) && Math.Abs(V[index].X) >= Math.Abs(V[index].Z))
                    .ToList();

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
                // return Resources.IconForThisComponent;
                return null;
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