using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;

namespace Scan2BIM_WIP
{
    public class Test2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Test2()
          : base("Test", "T",
              "Test something",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int i = 0;
            pManager.AddNumberParameter("Number", "n", "", GH_ParamAccess.item); pManager[i].Optional = false; i++;
            pManager.AddTextParameter("text", "n", "", GH_ParamAccess.item); pManager[i].Optional = false; i++;
            pManager.AddPointParameter("Point", "p", "", GH_ParamAccess.list); pManager[i].Optional = false; i++;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("text", "O", "Descr. of Out", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number", "cos", "The cosine of the Angle.", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "cos", "The cosine of the Angle.", GH_ParamAccess.list);

        }

        /// <summary>
        /// Test something
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            
            GH_PointCloud01 gH_PointCloud = new GH_PointCloud01();
            double number = 0.0;
            string text = "";
            List<Point3d> points = new List<Point3d>();
            

            /// read inputs
            if (!DA.GetData(0, ref number)) return;
            if (!DA.GetData(1, ref text)) return;
            if (!DA.GetDataList(2, points)) return;

            /// interal parameters 
            PointCloud pointCloud = new PointCloud(points.AsEnumerable());

            MWNumericArray mWNumericArray = new MWNumericArray();

            if (points.ToArray().ToMWNumericArray(ref mWNumericArray)) //
            {
                number = 10;
            }
            if (pointCloud.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArray))
            {
                number = 10;
            }
            Point3d[] target = new Point3d[mWNumericArray.Dimensions[0]];
            if (mWNumericArray.ToPoint3d(ref target))
            {
                number = 20;
            }
            
            else
            {
                number = -1;
            }               

            var points2 = pointCloud.AsReadOnlyListOfPoints();

            DA.SetData(0, text);
            DA.SetData(1, number );
            DA.SetDataList(2, points2);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                 return Properties.Resources.Icon_QA1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e8b80f9e-2cc0-4a78-bcd7-1c520f873ec4"); }
        }
    }
}