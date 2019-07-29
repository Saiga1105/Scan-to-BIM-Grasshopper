using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Scan2BIM
{
    public class Region_Growing_Walls : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _24_Region_Growing_Walls class.
        /// </summary>
        public Region_Growing_Walls()
          : base("Region_Growing_Walls", "RG_W",
              "Cluster the walls in a connected component based on potential wall thickness",
              "Saiga", "Clustering")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Initial List of Meshes", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddIntegerParameter("Mesh_si", "si", "Mesh Connections: indices of meshes at the beginning of a graph edge", GH_ParamAccess.tree); pManager[1].Optional = false;
            pManager.AddIntegerParameter("Mesh_sj", "sj", "Mesh Connections: indices of meshes at the end of a graph edge ", GH_ParamAccess.tree); pManager[2].Optional = false;
            pManager.AddNumberParameter("Threshold_Thickness", "t", "threshold bounding box thickness of the growing criteria of the walls ", GH_ParamAccess.item,0.8); pManager[3].Optional = true;

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
            List<Mesh> Rhino_Mesh = new List<Mesh>();
            GH_Structure<GH_Integer> Rhino_si = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Integer> Rhino_sj = new GH_Structure<GH_Integer>();
            double t = 0.8;
            DataTree<int> Rhino_Cluster = new DataTree<int>();///output

            ///import data
            if (!DA.GetDataList("Mesh", Rhino_Mesh)) return;
            if (!DA.GetDataTree("Mesh_si",  out Rhino_si)) return;
            if (!DA.GetDataTree("Mesh_sj",  out Rhino_sj)) return;
            if (!DA.GetData("Threshold_Thickness", ref t)) return;

            /// 0. loop through si[]
            ///     Add largest si to "cluster[]"
            ///     compute initial interval_0
            /// 1. loop through cluster[a]
            ///    select seed= cluster[a]
            ///    search all occurances of seed in si 
            ///    select corresponding indices in sj => add to queue[]
            ///    delete si.occurances
            ///    delete sj.occurances
            /// 2. loop through queue[i]
            ///     select next largest mesh in "queue[]" 
            ///     compute queue[i].BB given plane of s0
            ///     if length.interval within t => 
            ///         add queue[i] to cluster[]
            ///         update interval
            ///     else 
            ///         add queue[i] to si and sj
            ///     remove queue[i]
            ///     
            /// 

            /// local parameters
            ///Datatree<int> cluster = new List<int>();
            Box box_0 = new Box();
            Box box_i = new Box();
            Plane Plane_mesh = new Plane();
            Interval t_test = new Interval();
            int seed;
            

            for (int i = 0; i < Rhino_si.Branches.Count; i++)
            {
                int a = 0;
                var Rhino_si_branch = new List<int>() ;
                var Rhino_sj_branch = new List<int>();
                GH_Path gH_Path = Rhino_si.Paths[i];
                
                int d = new int(); int e = new int();
                for (int j = 0; j < Rhino_si.get_Branch(gH_Path).Count; j++)
                {
                    GH_Convert.ToInt32_Primary(Rhino_si.get_DataItem(gH_Path, j), ref d);
                    Rhino_si_branch.Add(d);
                    GH_Convert.ToInt32_Primary(Rhino_sj.get_DataItem(gH_Path, j), ref e);
                    Rhino_sj_branch.Add(e);
                }


                ///0.
                while (Rhino_si_branch.Any())
            {
                seed = Rhino_si_branch[0];

                GH_Path path = new GH_Path(i,a);
                Rhino_Cluster.Add(seed, path);

                /// create initial BB based on largest surface
                var Mesh_0 = Rhino_Mesh.ElementAt(Rhino_Cluster[path, 0]); /// select largest mesh
                var bbox = Mesh_0.GetBoundingBox(false); /// create world BB
                var Mesh_centroid = bbox.Center; /// retrieve centre
                List<Point3d> centroids = new List<Point3d>();
                for (int l = 0; l < Mesh_0.Faces.Count; l++) /// retrieve best fit plane
                {
                    centroids.Add(Mesh_0.Faces.GetFaceCenter(l));
                }
                Plane Plane_temp = new Plane();
                Plane.FitPlaneToPoints(centroids, out Plane_temp);
                Plane_mesh = Plane_temp; /// plane to rhino.plane (just a neccesity)
                Mesh_0.GetBoundingBox(Plane_mesh, out box_0);/// realign BB according plane mesh
                var t_0 = box_0.Z;
                t_test.Grow(t_0.Min);
                t_test.Grow(t_0.Max);
                int b = 0;

                ///1.
                while (Rhino_Cluster.Branch(path).Count > b)
                {
                    seed = Rhino_Cluster.Branch(path)[b];
                        /// find all occurances of seed in si

                        List<string> test = new List<string>();
                        test.Add("test");
                        var aret = "t";
                        var A = test.FindIndex(k => k == aret);

                        var si = Rhino_si_branch.FindIndex(s => s == seed);
                        var sj = Rhino_sj_branch.FindIndex(s => s == seed);
                        var queue = new List<int>();
                    /// select corresponding sj and add to queue
                    while (si != -1 || sj != -1)
                    {
                        si = Rhino_si_branch.FindIndex(s => s == seed);
                        
                            if (si != -1 )
                        {
                            Rhino_si_branch.RemoveAt(si);
                            if (Rhino_Cluster.Branch(path).FindIndex(s => s == Rhino_sj_branch[si]) == -1)
                            {
                                queue.Add((int)Rhino_sj_branch[si]);
                            }
                            Rhino_sj_branch.RemoveAt(si);
                        }
                        sj = Rhino_sj_branch.FindIndex(s => s == seed);
                            if (sj != -1)
                            {
                                Rhino_sj_branch.RemoveAt(sj);
                                if (Rhino_Cluster.Branch(path).FindIndex(s => s == Rhino_si_branch[sj]) == -1)
                                {
                                    queue.Add((int)Rhino_si_branch[sj]);
                                }
                                Rhino_si_branch.RemoveAt(sj);
                            }

                        }

                    var t_i = t_0;
                    while (queue.Count != 0)
                    {
                        Rhino_Mesh.ElementAt(queue[0]).GetBoundingBox(Plane_mesh, out box_i); /// select largest mesh
                        t_test = t_i;
                        t_test.Grow(box_i.Z.Max);
                        t_test.Grow(box_i.Z.Min);
                        if (Math.Abs(t_test.Length) <= t)
                        {
                            t_i = t_test;

                            if (Rhino_Cluster.Branch(path).FindIndex(s => s == queue[0]) == -1)
                            {
                                Rhino_Cluster.Add((int)queue[0], path);
                            }

                        }
                        else
                        {
                            Rhino_si_branch.Add(queue[0]);
                            Rhino_sj_branch.Add(queue[0]);

                        }

                        queue.RemoveAt(0);
                    }
                    b++;
                }
                a++;
                
            }
            }
            /// output
            DA.SetDataTree(0, Rhino_Cluster);
            

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
            get { return new Guid("3c6ade3b-8a19-4481-a8c8-7d51e1617ac8"); }
        }
    }
}