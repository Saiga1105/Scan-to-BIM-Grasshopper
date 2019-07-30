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
    public class CRF_walls_decode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public CRF_walls_decode()
          : base("CRF_walls_decode", "CRF_decode",
              "Given a graph G with edges={si,sj}, a set of descriptors and a set of trained parameters, compute the best fit network configuration ",
              "Saiga", "Clustering")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddNumberParameter("nodeFeature1", "fn1", "numerical value describing a node characteristic ", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddNumberParameter("nodeFeature2", "fn2", "numerical value describing a node characteristic ", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddNumberParameter("edgeFeature1", "fn1", "numerical value describing an edge characteristic ", GH_ParamAccess.list); pManager[2].Optional = false;
            pManager.AddNumberParameter("edgeFeature2", "fn2", "numerical value describing an edge characteristic ", GH_ParamAccess.list); pManager[3].Optional = false;

            pManager.AddIntegerParameter("Mesh_si", "si", "Mesh Connections: indices of meshes at the beginning of a graph edge", GH_ParamAccess.list); pManager[4].Optional = false;
            pManager.AddIntegerParameter("Mesh_sj", "sj", "Mesh Connections: indices of meshes at the end of a graph edge ", GH_ParamAccess.list); pManager[5].Optional = false;

            pManager.AddNumberParameter("Parameters", "w", "Pretrained weights for the node and edge potentials that will aid the clustering ", GH_ParamAccess.list); pManager[6].Optional = false;
            pManager.AddIntegerParameter("Nstates", "Nstates", "Max number of possible wall clusters that is considered for a node (default equal #seeds)", GH_ParamAccess.list); pManager[7].Optional = false;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Cluster_indices", "i", "Output clustered Mesh indices", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            List<double> nodeFeature1 = new List<double>();
            List<double> nodeFeature2 = new List<double>();
            List<double> edgeFeature1 = new List<double>();
            List<double> edgeFeature2 = new List<double>();
            List<int> Rhino_si = new List<int>();
            List<int> Rhino_sj = new List<int>();
            List<double> w = new List<double>();
            List<int> Nstates = new List<int>();

            ///import data
            if (!DA.GetDataList("nodeFeature1",  nodeFeature1)) return;
            if (!DA.GetDataList("nodeFeature2",  nodeFeature2)) return;
            if (!DA.GetDataList("edgeFeature1",  edgeFeature1)) return;
            if (!DA.GetDataList("edgeFeature2",  edgeFeature2)) return;
            if (!DA.GetDataList("Mesh_si",  Rhino_si)) return;
            if (!DA.GetDataList("Mesh_sj",  Rhino_sj)) return;
            if (!DA.GetDataList("Parameters", w)) return;
            if (!DA.GetDataList("Nstates", Nstates)) return;



            ///     0.cast list to MWarrays
            ///     1.run decode function Matlab
            ///     2.cast MWaray to list
            ///     3.output list

            ///0.
                var Matlab_fn1 = new MWNumericArray(nodeFeature1.Count, 1, nodeFeature1.ToArray());
                var Matlab_fn2 = new MWNumericArray(nodeFeature2.Count, 1, nodeFeature2.ToArray());
                var Matlab_fe1 = new MWNumericArray(edgeFeature1.Count, 1, edgeFeature1.ToArray());
                var Matlab_fe2 = new MWNumericArray(edgeFeature2.Count, 1, edgeFeature2.ToArray());
                var Matlab_si = new MWNumericArray(Rhino_si.Count, 1, Rhino_si.ToArray());
                var Matlab_sj = new MWNumericArray(Rhino_sj.Count, 1, Rhino_sj.ToArray());
                var Matlab_w = new MWNumericArray(w.Count, 1, w.ToArray());
                var Matlab_states = new MWNumericArray(Nstates.Count, 1, Nstates.ToArray());

         
            /// 1.
            Clustering.cluster cluster_mesh = new Clustering.cluster();

            MWArray cluster = new MWNumericArray();
            cluster = cluster_mesh.G_CRF_wall_decoding(Matlab_states, Matlab_fn1, Matlab_fn2, Matlab_fe1, Matlab_fe2, Matlab_si, Matlab_sj, Matlab_w);
            
            ///2.
            MWNumericArray na = (MWNumericArray)cluster;
            
            var dc = (int[])na.ToVector(0);
            var Rhino_indices = dc.ToList();
            for (int i = 0; i < Rhino_indices.Count; i++)
            {
                Rhino_indices[i] = Rhino_indices[i] - 1;
            }

            /// 3.
            DA.SetDataList(0, Rhino_indices);


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
            get { return new Guid("bebdb828-c80e-417d-b9d4-6c48df1b1b88"); }
        }
    }
}