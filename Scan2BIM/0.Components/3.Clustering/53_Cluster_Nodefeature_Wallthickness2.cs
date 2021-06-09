using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;


namespace Scan2BIM
{
    public class ClusterNodes2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public ClusterNodes2()
          : base("_52_Cluster_Nodefeature_Wallthickness", "fn1",
              "Compute potential wall thickness between every node and seed in the branch",
              "Saiga", "Clustering")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Nodes", "n", "nodes grouped per connected component (1 CP per branch) ", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddMeshParameter("Seeds", "s", "seeds grouped per connected component (1 CP per branch) ", GH_ParamAccess.list); pManager[0].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Potential Wall thickness", "i", "Output clustered Mesh indices", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            ///input parameters
            List<Mesh> nodes = new List<Mesh>();
            List<Mesh> seeds = new List<Mesh>();
            List<double> fn1 = new List<double>();

            ///import data
            if (!DA.GetDataList("Nodes", nodes)) return;
            if (!DA.GetDataList("Seeds", seeds)) return;

            /// compute seed parameters
            List<Plane> Plane_Seeds= new List<Plane>();
            var box = new Box();

            for (int j = 0; j < seeds.Count; j++)
            {
                var bbox = seeds[j].GetBoundingBox(false); /// create world BB
                var Mesh_centroid = bbox.Center; /// retrieve centre
                List<Point3d> centroids = new List<Point3d>();
                for (int l = 0; l < seeds[j].Faces.Count; l++) /// retrieve best fit plane
                {
                    centroids.Add(seeds[j].Faces.GetFaceCenter(l));
                }
                Plane Plane_temp = new Plane();
                Plane.FitPlaneToPoints(centroids, out Plane_temp);
                var Plane_mesh = Plane_temp; /// plane to rhino.plane (just a neccesity)
                Plane_Seeds.Add(Plane_mesh);
            }


            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < seeds.Count; j++)
                {
                    nodes[i].GetBoundingBox(Plane_Seeds[j], out box);/// realign BB according plane mesh
                    var t = box.Z;
                    t.Grow(0.0);
                    fn1.Add(Math.Abs(t.T0 - t.T1));
                }
            }
            DA.SetDataList(0, fn1);

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
            get { return new Guid("ccec5f57-028b-452f-99c5-3b921526566c"); }
        }
    }
}