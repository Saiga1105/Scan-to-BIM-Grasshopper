using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM.Components.Classification
{
    public class MyComponent1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MyComponent1()
          : base("Demo_Component", "Nickname",
              "Description",
              "Saiga", "Demo")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// <summary>
            /// Registers all the input parameters for this component.
            /// Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
            /// </summary>

             pManager.AddMeshParameter("Input1", "shortcut", "Description", GH_ParamAccess.list);
             pManager.AddNumberParameter("Number", "x", "Descr. of Number", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// <summary>
            /// Registers all the output parameters for this component.
            /// Use the pManager object to register your output parameters.
            /// Output parameters do not have default values, but they too must have the correct access type.
            /// </summary>
 
            pManager.AddMeshParameter("Output1", "Shorcut", "Description", GH_ParamAccess.list);
            pManager.AddNumberParameter("Out", "O", "Descr. of Out", GH_ParamAccess.list);
        }

             protected override void SolveInstance(IGH_DataAccess DA)
        {
            /// <summary>
            /// This is the method that actually does the work.
            /// </summary>
            /// 
            /// First, we need to retrieve all data from the input parameters.
            /// We'll start by declaring variables and assigning them starting values.  
            /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
            List<Mesh> Geometry = new List<Mesh>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> output = new List<double>();
            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetDataList(0, Geometry)) return;
            if (!DA.GetDataList(1, x)) return;

            // We should warn the user if invalid data is supplied.
            //if (GHArea < 0.0)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Area must be bigger than or equal to zero");
            //    return;

            /// run some function on the data

            //// Finally assign the output to the output parameter.
            DA.SetDataList(0, Geometry);
            DA.SetDataList(1, output);

        }


        protected override System.Drawing.Bitmap Icon
        {
            /// <summary>
            /// Provides an Icon for the component.
            /// </summary>
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
            get { return new Guid("cc930c14-203f-418a-b2cb-7b73303dda18"); }
        }
    }
}