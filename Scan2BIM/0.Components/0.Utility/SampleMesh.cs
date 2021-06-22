using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class SampleMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SampleMesh()
          : base("Sample Mesh", "Sample Mesh",
              "Sample 3D points on a mesh either by nrSamples (e.g. 1000), or by resolution (e.g. 100 #points per m²) ",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh Geometry", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("resolution", "r", "resolution of the point sampling on the mesh (e.g. 100 #points per m²)", GH_ParamAccess.item, 100); pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = null;
            Mesh mesh = new Mesh();
            double resolution = 0.05;

            // read inputs
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref resolution)) return;

            // Exceptions
            if (!mesh.IsValid) throw new Exception("Invalid mesh");
            if (resolution < 2) pc = mesh.GetVertexCloud();
            else pc = mesh.GenerateSpatialCloud(resolution);

            /// Output
            DA.SetData(0, pc);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_SampleMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4c54f01b-8d30-4efd-950b-0e4f2592fc56"); }
        }
    }
}