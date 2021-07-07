using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ComputePrincipalComponents : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PrincipalComponentAnalysis class.
        /// </summary>
        public ComputePrincipalComponents()
          : base("Principal Components", "PCA",
              "Compute the principal components of a set of 3D coordinates",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Geometry", "Geometry", "Input Point Cloud or Mesh", GH_ParamAccess.item); pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Eigenvectors", "v", "Eigenvectors of the 3D set {X,Y,Z}", GH_ParamAccess.list);
            pManager.AddNumberParameter("Eigenvalues", "λ", "Loading components of the eigenvectors", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = null;
            List<Vector3d> eigenvectors = null;
            List<double> eigenvalues = null;

            // read inputs
            if (!DA.GetData(0, ref pc)) return;

            // some error catching here
            if (pc.Value.Count < 3 ) throw new Exception("Pick a larger set");
            if (!pc.IsValid) throw new Exception("Invalid Point Cloud");

            // method
            pc.ComputePrincipalComponents(out eigenvectors, out eigenvalues);
            
            /// Output
            DA.SetDataList(0, eigenvectors);
            DA.SetDataList(1, eigenvalues);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                 return Properties.Resources.Icon_PCA;                
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("39aa7b32-4c7a-424a-a2b9-9f713788a538"); }
        }
    }
}