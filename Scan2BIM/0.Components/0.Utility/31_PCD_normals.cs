using System;
using System.Collections.Generic;
using System.Linq;
using Volvox_Cloud;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathWorks.MATLAB.NET.Arrays;

namespace Scan2BIM
{
    public class PCD_normals : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PCD_normals()
          : base("PCD_normals", "N",
              "Computes normals for point cloud if there aren't any yet",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point_Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("k", "k", "number of neighbors to consult for the normal estimation", GH_ParamAccess.item,6); pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point_Cloud_with_normals", "PCDn", "Point Cloud data with normals", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            PointCloud Rhino_Cloud = new PointCloud();

            /// read inputs
            if (!DA.GetData("Point_Cloud", ref Rhino_Cloud)) return;
            if (!DA.GetData("k", ref k)) return;

            /// interal parameters 
            var Rhino_xyz = Rhino_Cloud.GetPoints();
            List<double> xyz = new List<double>();

            if (!Rhino_Cloud.ContainsNormals)

                for (int i = 0; i < Rhino_Cloud.Count; i++)
                {
                xyz.Add(Rhino_xyz[i].X);
                xyz.Add(Rhino_xyz[i].Y);
                xyz.Add(Rhino_xyz[i].Z);
                }

                ///2.
                var Matlab_xyz = new MWNumericArray(Rhino_Cloud.Count, 3, xyz.ToArray());
            
            

                /// 3.
                Segmentation.segment segment_mesh = new Segmentation.segment();

                MWArray array = new MWNumericArray();
                array = pointcloudutilities.G_compute_normals(Matlab_xyz, k);

                /// 4.
                MWNumericArray na = (MWNumericArray)array;
                double[] dc = (double[])na.ToVector(0);

            /// 5.%£%/:
            Resolution = dc[0];

            /// 6.
            DA.SetData(0, Resolution);
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
            get { return new Guid("e8b70f9e-2cc0-4a78-bcd7-1c520f873ec8"); }
        }
    }
}