using System;
using System.Collections.Generic;

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Scan2BIM
{
    public class SplitVectors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SplitVectors class.
        /// </summary>
        public SplitVectors()
          : base("Split vectors", "Vectors",
              "Distribute vectors along cardinal axes. The method returns a list of vectors for each cardinal axis",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vectors", "V", " Vector {X,Y,Z}", GH_ParamAccess.tree); pManager[0].Optional = false;
            pManager.AddBooleanParameter("Strict", "s", " 0:vectors can only be assigned to 1 axis, 1: Every vector within a 45°cone of an axis is considered ", GH_ParamAccess.item, false); pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("X", "X", "Indices of vectors assigned to X-axis", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Define i/o parameters
            bool s = false;

            // Read inputs
            if (!DA.GetDataTree("Vectors", out GH_Structure<GH_Vector> vectorTree)) return;
            if (!DA.GetData("Strict", ref s)) return;

            // Exceptions
            if (vectorTree.GetType() != typeof(GH_Structure<GH_Vector>)) throw new Exception("Invalid vectorTree");
            if (vectorTree.IsEmpty) throw new Exception("Empty vectorTree");
            if (vectorTree.PathCount <1) throw new Exception("This vectorTree is to small");

            DA.SetDataTree(0, vectorTree.SplitVectors(s));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_SplitVectors;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C09-412D-8FF8-B76439197730"); }
        }
    }
}