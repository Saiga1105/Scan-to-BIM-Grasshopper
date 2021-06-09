using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;

namespace Scan2BIM_WIP
{
    public class Test : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Test()
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
            //pManager.AddAngleParameter("Angle", "a", "", GH_ParamAccess.item); pManager[i].Optional = true;i++;
            //pManager.AddArcParameter("Arc", "a", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddBooleanParameter("Boolean", "b", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddBoxParameter("Box", "b", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddBrepParameter("Brep", "b", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddCircleParameter("Circle", "c", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddColourParameter("Colour", "c", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddCurveParameter("Curve", "c", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            pManager.AddGeometryParameter("Geometry", "g", "Point Cloud data", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddIntegerParameter("Integer", "i", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddLineParameter("Line", "l", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddMeshParameter("Mesh", "m", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            //pManager.AddNumberParameter("Number", "n", "", GH_ParamAccess.item); pManager[i].Optional = true; i++;
            pManager.AddPointParameter("Point", "p", "", GH_ParamAccess.list); pManager[i].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Out", "O", "Descr. of Out", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cos", "cos", "The cosine of the Angle.", GH_ParamAccess.item);

        }
        
        /// <summary>
        /// Test something
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            PointCloud pointCloud = new PointCloud();
            //GH_PointCloud gH_PointCloud = new GH_PointCloud();
            //Double k = 6; 
            List<Point3d> points = new List<Point3d>();

            /// read inputs
            if (!DA.GetData(0, ref pointCloud)) return;
            if (!DA.GetDataList(1, points)) return;

            /// interal parameters 
                       
            //List<float> output = new List<float>();
            //if (points2.ToMWNumericArray(ref mWNumericArray)) //
            //{
            //    /// 3.
            //    //mWNumericArray.ToPoint3d(ref points);
            //    output = 1.0; 
            //}
            //output.Add(5);
            //var points2 = points;
            double output = 2.5;
            
            //var output2 = output;
            string data = "lalalal";
            DA.SetData(0, data);
            DA.SetData(1, Math.Cos(output));
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
            get { return new Guid("e8b70f9e-2cc0-4a78-bcd7-1c520f873ec4"); }
        }
    }
}