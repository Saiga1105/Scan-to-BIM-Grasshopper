using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;
using Scan2BIM_Matlab;

namespace Scan2BIM
{
    public class PCD_normals : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PCD_normals()
          : base("Compute normals", "N",
              "Computes normals for point cloud if there aren't any yet",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(),"Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("k", "k", "number of neighbors to consult for the normal estimation", GH_ParamAccess.item,6); pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCDn", "Point Cloud data with normals", GH_ParamAccess.item);
        }
        public class Size_Exception : Exception
        {
            public Size_Exception(string message) : base(message) { }

        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            GH_PointCloud pc = null;
            Double k = 6;

            /// read inputs
            //if (!DA.GetData(0, ref pointCloud)) return;
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData("k", ref k)) return;
            
            // Exceptions
            if (pc.Value.Count <= k) throw new Exception("Use point clouds with more than k points");     
            if (k < 3)throw new Size_Exception("enter value k >=3");
       
            /// interal parameters 
            if (!pc.Value.ContainsNormals)
            {
                pc.ComputeNormals((int)k);
            }
            
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
                 return Properties.Resources.Icon_Normals;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e8b70f9e-2cc0-4a68-bcd7-1c520f873ec8"); }
        }
    }
}