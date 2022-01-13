using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;
using Rhino.DocObjects;


using RDFSharp; // https://github.com/mdesalvo/RDFSharp
using RDFSharp.Model; // https://github.com/mdesalvo/RDFSharp/issues/79 => see intstruction manual
using RDFSharp.Query;
using RDFSharp.Semantics;
using RDFSharp.Store;


namespace Scan2BIM
{
    public class ExportRDF : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public ExportRDF()
          : base("Export RDF", "ExportRDF",
              "Export the metadata of a set of meshes or point clouds to tll or a xml file as Linked Data ",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometries", "G", " Meshes or Point Clouds of which the metadata that should be published as linked open data", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddTextParameter("Filename", "C:/", "location and filename e.g. D:/test.ttl", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddTextParameter("Names", "Names", "GUIDs or Names of the geometries.", GH_ParamAccess.list); pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("RDF", "RDF", "Representation of output RDF as text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /// input / output parameters
            List<Object> geometries = new List<Object>();
            var filename = "";
            List<string> names = new List<string>();
            string output = "failed to export graph";

            // internal parameters
            List<GH_Mesh> meshes = new List<GH_Mesh>();
            List<GH_PointCloud> pointClouds = new List<GH_PointCloud>();
            RDFGraph graph = null;

            ///import data
            if (!DA.GetDataList("Geometries", geometries)) return;
            if (!DA.GetData("Filename", ref filename)) return;
            if (!DA.GetDataList("Names", names)) return;

            // add exceptions
            if (!filename.EndsWith(".ttl") && !filename.EndsWith(".xml")) throw new Exception("Invalid file path. Enter a filename with .ttl or .xml extension.");
            if (!geometries.Any()) throw new Exception("Add at least one mesh or pointcloud.");
            if (!filename.IsValidFileName(true)) throw new Exception("filename contains invalid character.");

            // Create graphs for meshes or pointclouds
            if (geometries[0].GetType() == typeof(GH_PointCloud) || geometries[0].GetType() == typeof(PointCloud))
            {
                foreach (Object geometry in geometries)
                {
                    var pcd = (GH_PointCloud)geometry;
                    pointClouds.Add(pcd);
                    
                }
                graph = pointClouds.CreateGraph(names);
            }
            else if (geometries[0].GetType() == typeof(GH_Mesh) || geometries[0].GetType() == typeof(Mesh))
            {
                foreach (Object geometry in geometries)
                {
                    var mesh = (GH_Mesh)geometry;
                    meshes.Add(mesh);
                }
                graph = meshes.CreateGraph(names);

            }
            else throw new Exception("Only submit GH_PointCloud, PointCloud, Mesh geometry. \n If it's another type of point cloud (e.g. Volvox, Tarsier, etc.) \n just pass it through a normal Grasshopper cloud parameter first.");

            // export graph
            if (graph.ExportRDF(filename))
            {
                output = graph.ToString();             
            }
            
            // set output 
            DA.SetData(0, output);
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
            get { return new Guid("acec5f57-028b-452f-99c5-3b921527566d"); }
        }
    }
}