using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
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
            pManager.AddNumberParameter("tolerance", "t", "tolerance [m] for the inliers of the plane", GH_ParamAccess.item, 0.05); pManager[1].Optional = true;
            pManager.AddNumberParameter("resolution", "r", "resolution of the point sampling on the mesh", GH_ParamAccess.item, 0.05); pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "s", "Best fit planar surface", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "p", "", GH_ParamAccess.list);
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
            Double tolerance = 0.05; Double resolution = 0.05;

            // Read inputs
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref tolerance)) return;
            if (!DA.GetData(2, ref resolution)) return;

            // Exceptions
            if (!mesh.IsValid) throw new Exception("Provide a valid mesh");

            mesh.FitPlanarSurface(out Brep brep, out List<Point3d> points3D, out Double rmse, tolerance, resolution );

            DA.SetData(0, brep);
            DA.SetDataList(1, points3D);
            DA.SetData(2, rmse);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_FitPlaneToMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C86-412D-8FF8-B76439197730"); }
        }
    }
}