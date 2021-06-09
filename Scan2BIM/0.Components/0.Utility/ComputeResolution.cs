using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class ComputeResolution : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComputeResolution()
          : base("Compute resolution", "Resolution",
              "Computes the average resolution of a point cloud based on a % number of samples",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("Percentage", "k", "% of samples", GH_ParamAccess.item, 0.01); pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Resolution", "R", "Average resolution of the point cloud", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Define i/o parameters
            GH_PointCloud pc = null;
            Double k = 0.0;

            // Read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData("k", ref k)) return;
   
            // Exceptions
            if (!pc.IsValid) throw new Exception("Invalid point cloud");            
            if (k < 0 || k > 1) throw new Exception("provide resolution between 0 and 1");
            if ((double)pc.Value.Count * k <2) throw new Exception("insufficient points in point cloud");

            // Output
            DA.SetData(0, pc.ComputeResolution(k));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_Sample1;

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