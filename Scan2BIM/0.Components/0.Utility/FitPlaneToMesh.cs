using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM._0.Components._0.Utility
{
    public class FitPlaneToMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FitPlaneToMesh()
          : base("Fit plane to Mesh", "Fit Plane",
              "Compute the best fit (least squares) planar surface through a mesh",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh Geometry", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("downsampling", "R0", "Percentage of points [0;1] to use for the planefitting", GH_ParamAccess.item, 1.0); pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "Best fit planar surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("rmse", "rmse", "Root mean squared error of deviations from the used points to the planar surface", GH_ParamAccess.item); 

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        { 
            // Define i/o parameters
            Mesh mesh = new Mesh();
            Double downsampling = 1.0;

            // Read inputs
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref downsampling)) return;

            // Exceptions
            if (!mesh.IsValid) throw new Exception("Provide a valid mesh");

            mesh.FitPlanarSurface(out Surface surface, out Double rmse, downsampling);

            DA.SetData(0, surface);
            DA.SetData(1, rmse);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                 return Properties.Resources.Icon_CRF1;
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