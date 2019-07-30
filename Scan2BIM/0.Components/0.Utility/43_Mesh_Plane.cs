using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class _43_Mesh_Plane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _43_Mesh_Plane class.
        /// </summary>
        public _43_Mesh_Plane()
          : base("Mesh_Plane", "Mesh_Plane",
              "Compute the best fit plane through a mesh",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh Geometry", GH_ParamAccess.item); pManager[0].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Cluster_indices", "i", "Output segmented vertex indices", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
                      
            ///input parameters
            Mesh Rhino_Mesh = new Mesh();

            ///import data
            if (!DA.GetData("Mesh", ref Rhino_Mesh)) return;

            /// local parameters
            /// 
            Plane Plane_s = new Plane();
            List<Point3d> centroids = new List<Point3d>();


            for (int i = 0; i < Rhino_Mesh.Faces.Count; i++) /// retrieve best fit plane
            {
                centroids.Add(Rhino_Mesh.Faces.GetFaceCenter(i));
            }

            Rhino.Geometry.Plane.FitPlaneToPoints(centroids, out Plane_s);

            DA.SetData(0, Plane_s);

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
            get { return new Guid("bec5da04-8dc9-434d-9cfb-552dfb27b0f4"); }
        }
    }
}