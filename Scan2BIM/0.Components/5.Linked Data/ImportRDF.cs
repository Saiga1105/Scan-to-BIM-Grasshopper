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
    public class ImportRDF : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public ImportRDF()
          : base("Import RDF", "Import RDF",
              "Import the Linked Data metadata from a .tll or .xml file as seperate subject, predicate and literal lists.",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filenames", "C:/", "location and filename e.g. D:/test.ttl", GH_ParamAccess.item); pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Subject", "Subj", "Representation of subject as text", GH_ParamAccess.item);
            pManager.AddTextParameter("Predicate", "Pred", "Representation of predicate as text", GH_ParamAccess.list);
            pManager.AddTextParameter("Literal", "Lit", "Representation of literal as text", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /// input / output parameters
            var filenames = new List<string>();
            

            // internal parameters
            //List<GH_Mesh> meshes = new List<GH_Mesh>();
            //List<GH_PointCloud> pointClouds = new List<GH_PointCloud>();
            RDFGraph graph = null;

            ///import data
            if (!DA.GetDataList("Filenames", filenames)) return;

            // add exceptions
            foreach (string filename in filenames)
            {
                if (!filename.EndsWith(".ttl") && !filename.EndsWith(".xml")) throw new Exception("Invalid file path. Enter a filename with .ttl or .xml extension.");
                if (!filename.IsValidFileName(true)) throw new Exception("filename contains invalid character.");
            }

            // create list for all the subjects, predicates and literals
            List<List<string>> subjectlists = null;
            List<List<string>> predicateLists = null;
            List<List<string>> literalLists = null;

            //read file
            foreach (string filename in filenames)
            {              
                graph = LinkedDataTools.ImportRDF(filename);

                // extract all the subject resources that are in the graph
                List<RDFResource> subjects = graph.RDFQuerySubjects();

                

                // create list for each subject resource
                List<string> subjectlist = null;
                List<string> predicateList = null;
                List<string> literalList = null;

                foreach (RDFResource subject in subjects)
                {
                    RDFGraph subgraph=graph.SelectTriplesBySubject(subject);
                    var dataTable=subgraph.ToDataTable();
                    //subgraph.ToList
                    //dataTable.DataSet.Tables.

                }

            }

            // set output 
            DA.SetDataList(0, subjectlists);
            DA.SetDataList(1, predicateLists);
            DA.SetDataList(2, literalLists);


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
            get { return new Guid("acdc5f57-028b-452f-99c5-3b921527566d"); }
        }
    }
}