using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{ 
    public class DecomposeTransformationMatrix : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DecomposeTransformationMatrix()
          : base("Decompose transformation matrix", "Decompose",
              "Decompose [4x4] transformation matrix to translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTransformParameter("transform", "T", " Rigid Transformation matrix [4x4] ", GH_ParamAccess.item); pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("R", "R", "rotation {Rx,Ry,Rz} in radians [0;2π] ", GH_ParamAccess.item);
            pManager.AddVectorParameter("t", "t", "translation {X,Y,Z} in radians [0;2π]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Define i/o parameters
            //List<double> tform = new List<double>();
            Transform transform = new Transform();

            // Read inputs
            if (!DA.GetData(0, ref transform)) return;

            // Exceptions
            if (!transform.IsAffine) throw new Exception("This is not an affine Transformation matrix [4x4]");
            if (transform.IsRigid(0.1) != TransformRigidType.Rigid ) throw new Exception("This is not an a rigid Transformation matrix [4x4]");

            transform.DecomposeRigid(out Vector3d translation, out Transform rotation, 0.001);
            rotation.GetEulerZYZ(out double alpha, out double beta, out double gamma);

            //transform.DecomposeTransformationMatrix(out Vector3d R, out Vector3d t);

            DA.SetData(0, new Vector3d(gamma, beta, alpha)); // X, Y ,Z
            DA.SetData(1, translation);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_DecomposeTransformationMatrix;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C05-412D-8FF8-B76439197730"); }
        }
    }
}