using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Scan2BIM2
{
    public class Test : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Test()
          : base("Test2", "T",
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
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            PointCloud pointCloud = new PointCloud();
            //GH_PointCloud gH_PointCloud = new GH_PointCloud();
            //Double k = 6; 
            List<Point3d> points = new List<Point3d>();

            /// read inputs
            if (!DA.GetData(0, ref pointCloud)) return;
            if (!DA.GetDataList(1, points)) return;

            double output = 2.5;
            //var output2 = output;
            string data = "lalalal";
            DA.SetData(0, data);
            DA.SetData(1, Math.Cos(output));
        }

        Curve CreateSpiral(Plane plane, double r0, double r1, Int32 turns)
        {
            Line l0 = new Line(plane.Origin + r0 * plane.XAxis, plane.Origin + r1 * plane.XAxis);
            Line l1 = new Line(plane.Origin - r0 * plane.XAxis, plane.Origin - r1 * plane.XAxis);

            Point3d[] p0;
            Point3d[] p1;

            l0.ToNurbsCurve().DivideByCount(turns, true, out p0);
            l1.ToNurbsCurve().DivideByCount(turns, true, out p1);

            PolyCurve spiral = new PolyCurve();

            for (int i = 0; i < p0.Length - 1; i++)
            {
                Arc arc0 = new Arc(p0[i], plane.YAxis, p1[i + 1]);
                Arc arc1 = new Arc(p1[i + 1], -plane.YAxis, p0[i + 1]);

                spiral.Append(arc0);
                spiral.Append(arc1);
            }

            return spiral;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("EF327515-96C7-4F14-ACB2-6A593BB96721");
    }
}