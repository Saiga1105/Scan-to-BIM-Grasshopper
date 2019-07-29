using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class Restructure_Mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Restructure_Mesh()
          : base("Restructure_Mesh", "Restructure",
              "restructure mesh according to computed indices of regions (Region_Growing_Normals) ",
              "Saiga", "Segmentation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh Geometry", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("Indices", "indices", "Face indices of the Region Growing methods ", GH_ParamAccess.list); pManager[1].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput submeshes
            pManager.AddMeshParameter("submeshes", "M_s", "Output segmented submeshes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            Mesh Rhino_mesh = new Mesh();
            List<double> Rhino_indices = new List<double>();
            List<Mesh> Rhino_submeshes = new List<Mesh>();

            /// read inputs
            if (!DA.GetData("Mesh", ref Rhino_mesh)) return;
            if (!DA.GetDataList<double>("Indices", Rhino_indices)) return;

            /// define internal parameters
            int cluster_count = (int)Rhino_indices.Max();
            Point3f vertex;
            ///1. maak lijsten van idexen per value van Rhino-indices
            ///2. Retrieve de Mesh.face van idere index
            ///3. Retrieve hoekpunten van iedere Mesh.face van iedere index
            ///4. Maak nieuwe face met vertices
            ///5. maak nieuwe mesh
            ///6. verwijder dubbele vertices
            ///7. output
            for (int i = 0; i < cluster_count; i++)
            {
                Mesh Rhino_submesh = new Mesh();
                List<MeshFace> Rhino_faces = new List<MeshFace>();
                
                ///1.
                var C_indexlist = Enumerable.Range(0, Rhino_indices.Count).Where(flap => Rhino_indices[flap] == i + 1).ToList();
                ///2.
                
                for (int j = 0; j < C_indexlist.Count; j++)
                {
                    ///3,4.
                    vertex = Rhino_mesh.Vertices.ElementAt(Rhino_mesh.Faces.GetFace(C_indexlist[j]).A);
                    Rhino_submesh.Vertices.Add(vertex);
                    vertex = Rhino_mesh.Vertices.ElementAt(Rhino_mesh.Faces.GetFace(C_indexlist[j]).B);
                    Rhino_submesh.Vertices.Add(vertex);
                    vertex = Rhino_mesh.Vertices.ElementAt(Rhino_mesh.Faces.GetFace(C_indexlist[j]).C);
                    Rhino_submesh.Vertices.Add(vertex);
                    ///5.
                    Rhino_submesh.Faces.AddFace(j*3+0, j*3+1, j*3+2);

                }
                ///6.
                Rhino_submesh.Vertices.CombineIdentical(false,false);

                ///7.
                Rhino_submeshes.Add(Rhino_submesh);

            }

            /// create tree 
            /// test1.Add( data,pth);
            /// GH_Path pth = new GH_Path(i);

            ///output
            DA.SetDataList(0, Rhino_submeshes);
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
            get { return new Guid("f15cac02-324b-41ff-af77-511a6d45a44e"); }
        }
    }
}