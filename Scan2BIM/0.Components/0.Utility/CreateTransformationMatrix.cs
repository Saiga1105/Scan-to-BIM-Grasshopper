using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class CreateTransformationMatrix : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CreateTransformationMatrix()
          : base("Create transformation matrix", "Transformation",
              "Create [16,1] transformation matrix from translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("R", "R", "rotation {Rx,Ry,Rz}  ", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddVectorParameter("t", "T", "translation {X,Y,Z}", GH_ParamAccess.item); pManager[1].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("tform", "T", " Rigid Transformation matrix [16,1] ", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Define i/o parameters
            Vector3d R = new Vector3d();
            Vector3d t = new Vector3d();

            // Read inputs
            if (!DA.GetData("R", ref R)) return;
            if (!DA.GetData("t", ref t)) return;

            // Exceptions
            if (!R.IsValid) throw new Exception("Invalid rotation {Rx,Ry,Rz} vector");
            if (!t.IsValid) throw new Exception("Invalid translation {tx,ty,tz} vector");

            List<double> tform = SpatialTools.CreateTransformationMatrix(R, t);

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
                return Properties.Resources.Icon_CreateTransformationMatrix;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C03-412D-8FF8-B76439197730"); }
        }
    }
}