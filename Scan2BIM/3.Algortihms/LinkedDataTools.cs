using System;
using System.IO;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


using Rhino.Geometry; // https://developer.rhino3d.com/api/
using Rhino.DocObjects.Custom;
using Rhino.Display;
using Rhino.FileIO;

using Grasshopper; // https://developer.rhino3d.com/api/
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using RDFSharp; // https://github.com/mdesalvo/RDFSharp
using RDFSharp.Model; // https://github.com/mdesalvo/RDFSharp/issues/79 => see intstruction manual
using RDFSharp.Query;
using RDFSharp.Semantics;
using RDFSharp.Store;

// we don't yet know the difference between these namespaces?


namespace Scan2BIM
{
    /// <summary>
    /// Linked Data operations such as reading/writing RDF files, etc.
    /// </summary>
    public static class LinkedDataTools
    {
        // should we make a class for the RDF
        // we want each record to have a branch
        // the only way you can work with a class is if you know its structure... but you don't
        // you can make a class for the exporter though

        // resource = an image, point cloud, ortho or mesh
        // triples = properties about the resources e.g. geometry location, bounding box, etc.
        // collection/container = all images, or point clouds, etc. with the same context
        // federation = all resources in a session 
        // graph joint representation of all resources and relations


        #region Methods

        /// <summary>
        /// Build a graph from a IEnumerable<GH_Mesh>
        /// </summary>
        public static RDFGraph CreateGraph(this IEnumerable<GH_Mesh> meshes, IEnumerable<string> names )
        {
            // local parameters
            var resourcenames = new List<string>();
            Guid g = new Guid();



            // create a resourcename for every mesh (GUID)
            if (names.Any()) resourcenames = names.ToList();
            else if (meshes.ElementAt(0).ReferenceID != Guid.Empty)
            {
                foreach (GH_Mesh mesh in meshes)
                {
                    resourcenames.Add(mesh.ReferenceID.ToString());
                }
            }
            else foreach (GH_Mesh mesh in meshes)
                {
                    g = Guid.NewGuid();
                    resourcenames.Add(g.ToString());
                }

            // create a resource for the collection of meshes that has the coordinate system, etc.
            //var rdf_collection = new RDFResource("http://" + Guid.NewGuid().ToString()); // given "uriString" parameter is null or cannot be converted to a valid Uri'


            // CREATE NAMESPACES
            var exif = new RDFNamespace("exif", "http://www.w3.org/2003/12/exif/ns"); //=> can you add new vocabularies
            RDFNamespaceRegister.AddNamespace(exif);


            // create new graph of the list 
            var rdfgraph = new RDFGraph(); // unsure of the namespaces will be added to the graph.


            // create resource for every object
            for (int i=0;i<meshes.Count();i++)
            {
                var rdf_resource = new RDFResource("http://" + resourcenames[i]); // GUID is used as resource name

                // extract typed literals (values) from the objects 
                var valid = meshes.ElementAt(i).Value.Normals.Any();


                // extract (typed) literals (values) from the objects and create triple for each literal
                RDFTriple dataType = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.RDFS.DATATYPE, // predicate
                                                     new RDFPlainLiteral("Rhino.Geometry.Mesh")); // literal                
                RDFTriple isValid = new RDFTriple(
                                                     rdf_resource, // subject
                                                     new RDFResource("http://xmlns.com/foaf/0.1/age"), // predicate
                                                     new RDFTypedLiteral(meshes.ElementAt(i).IsValid.ToString(), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)); // literal => this throws a RDFModelException if not correct
                RDFTriple boundingBoxMin = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.FOAF.KNOWS, // predicate
                                                     new RDFPlainLiteral(meshes.ElementAt(i).Boundingbox.Min.ToString())); // literal 
                RDFTriple boundingBoxMax = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.FOAF.KNOWS, // predicate
                                                     new RDFPlainLiteral(meshes.ElementAt(i).Boundingbox.Max.ToString())); // literal 
                RDFTriple vertexCount = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.FOAF.KNOWS, // predicate
                                                     new RDFTypedLiteral(meshes.ElementAt(i).Value.Vertices.Count.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)); // literal 
                RDFTriple faceCount = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.FOAF.KNOWS, // predicate
                                                     new RDFTypedLiteral(meshes.ElementAt(i).Value.Faces.Count.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)); // literal 
                RDFTriple hasNormals = new RDFTriple(
                                                     rdf_resource, // subject
                                                     RDFVocabulary.FOAF.KNOWS, // predicate
                                                     new RDFTypedLiteral(meshes.ElementAt(i).Value.Normals.Any().ToString(), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)); // literal 
                // we need a literal for storage on file

                if (0==0) // GH_Mesh.coordinateSystem.Any() => this coordinate system tracking should still be implemented
                {
                    // still need conversion to WGS84
                    // only do the conversion to calculate the offset of the entire dataset => don't mingle with local coordinates
                    
                    RDFTriple latitude = new RDFTriple(
                                                        rdf_resource, // subject
                                                        RDFVocabulary.GEO.LAT, // predicate
                                                        new RDFTypedLiteral(meshes.ElementAt(i).Boundingbox.Center.Y.ToString(), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)); // should be still converted to WGS84
                    RDFTriple longitude = new RDFTriple(
                                                        rdf_resource, // subject
                                                        RDFVocabulary.GEO.LONG, // predicate
                                                        new RDFTypedLiteral(meshes.ElementAt(i).Boundingbox.Center.X.ToString(), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)); // should be still converted to WGS84 
                    RDFTriple altitude = new RDFTriple(
                                                        rdf_resource, // subject
                                                        RDFVocabulary.GEO.ALT, // predicate
                                                        new RDFTypedLiteral(meshes.ElementAt(i).Boundingbox.Center.Z.ToString(), RDFModelEnums.RDFDatatypes.XSD_DOUBLE)); // should be still converted to WGS84 
                }

                rdfgraph.AddTriple(dataType);
                rdfgraph.AddTriple(isValid);
                rdfgraph.AddTriple(boundingBoxMin);
                rdfgraph.AddTriple(boundingBoxMax);
                rdfgraph.AddTriple(vertexCount);
                rdfgraph.AddTriple(faceCount);
                rdfgraph.AddTriple(hasNormals);
                //rdfgraph.AddTriple(latitude);
                //rdfgraph.AddTriple(longitude);
                //rdfgraph.AddTriple(altitude);

            }
            return rdfgraph;
        }

        /// <summary>
        /// Build a graph from a IEnumerable<GH_PointClouds>
        /// </summary>
        public static RDFGraph CreateGraph(this IEnumerable<GH_PointCloud> pointclouds, IEnumerable<string> names) // this is under construction
        {
            // create a resource that will serve as the subject of the linked ata e.g. an image guid
            var donaldduck = new RDFResource("http://www.waltdisney.com/donald_duck"); // use the GUID of existing stuff e.g. images and BIM meshes
            var disney_group = new RDFResource(); // blank resource for newly created stuff



            // extract typed literals (values) from the resources 
            var donaldduck_name = new RDFPlainLiteral("Donald Duck");
            var mickeymouse_age = new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER);


            // create triple for each literal
            RDFTriple mickeymouse_is85yr = new RDFTriple(
                                                 new RDFResource("http://www.waltdisney.com/mickey_mouse"), // subject
                                                 new RDFResource("http://xmlns.com/foaf/0.1/age"), // predicate
                                                 new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER)); // literal
            

            // create a list of all the triples you want to add to the graph
            var triples = new List<RDFTriple>();

            // or first  combine them in a collection and then add them to a graph
            RDFCollection beatles = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            beatles.AddItem(new RDFResource("http://beatles.com/ringo_starr"));


            // create new graph of the list 
            var rdfgraph = new RDFGraph(triples);

            // insert extra triples 
            rdfgraph.AddTriple(mickeymouse_is85yr);
            rdfgraph.AddCollection(beatles);

            // CREATE NAMESPACE
            var exif = new RDFNamespace("exif", "http://www.w3.org/2003/12/exif/ns");
            RDFNamespaceRegister.AddNamespace(exif);



            // SET CONTEXT OF A GRAPH
            rdfgraph.SetContext(new Uri("http://waltdisney.com/"));


            // select triples by subject/resource
            RDFGraph triples_by_subject = rdfgraph.SelectTriplesBySubject(donaldduck);

            return rdfgraph;
        }

        /// <summary>
        /// Write an RDF graph to a file (.ttl or .xml)
        /// </summary>
        public static Boolean ExportRDF(this RDFGraph graph, string filename)
        {
            var result = false;
            // check if filename is a valid path

            // check file extension
            if (filename.EndsWith("ttl"))
            {
                try
                {
                    var turtleFormat = RDFModelEnums.RDFFormats.Turtle;
                    graph.ToFile(turtleFormat, filename); // e.g. "C:\\newfile.ttl"
                    result = true;
                }
                catch (Exception) { throw new Exception("Couldn't write turtle file with RDFSHARP API."); }
            }

            if (filename.EndsWith("xml"))
            {
                try
                {
                    var xmlFormat = RDFModelEnums.RDFFormats.RdfXml;
                    graph.ToFile(xmlFormat, filename); // e.g. "C:\\file.rdf"
                    result = true;
                }
                catch (Exception) { throw new Exception("Couldn't write xml file with RDFSHARP API."); }
            }
            return result;
        }

        /// <summary>
        /// Read an RDF graph from a file (.ttl or .xml)
        /// </summary>
        public static RDFGraph ImportRDF(string filename)
        {
            var graph = new RDFGraph();

            // check file extension
            if (filename.EndsWith("ttl"))
            {
                try
                {
                    var turtleFormat = RDFModelEnums.RDFFormats.Turtle;
                    graph = RDFGraph.FromFile(turtleFormat, filename);
                }
                catch (Exception) { throw new Exception("Couldn't read turtle file with RDFSHARP API."); }
            }

            if (filename.EndsWith("xml"))
            {
                try
                {
                    var xmlFormat = RDFModelEnums.RDFFormats.RdfXml;
                    graph = RDFGraph.FromFile(xmlFormat, filename); // e.g. "C:\\file.rdf"
                }
                catch (Exception) { throw new Exception("Couldn't read xml file with RDFSHARP API."); }
            }
            return graph;                    
        }

        /// <summary>
        /// Query an RDF graph 
        /// </summary>
        public static List<RDFResource> RDFQuerySubjects(this RDFGraph graph)
        {
            List < RDFResource > resources = null;
            //// CREATE SELECT QUERY
            //RDFSelectQuery selectQuery = new RDFSelectQuery();

            //// example
            //RDFSelectQuery query = new RDFSelectQuery() 
            //    .AddPrefix(RDFNamespaceRegister.GetByPrefix("dc")) 
            //    .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf")) 
            //    .AddPatternGroup(new RDFPatternGroup("PG1") 
            //    .AddPattern(new RDFPattern(y, dogOf, x)) 
            //    .AddPattern(new RDFPattern(x, name,n).Optional())
            //    .AddPattern(new RDFPattern(x, knows, h))
            //    .AddFilter(new RDFRegexFilter(n, new Regex(@"Mouse",RegexOptions.IgnoreCase)))) 
            //    .AddModifier(new RDFOrderByModifier(y,RDFQueryEnums.RDFOrderByFlavors.DESC))
            //    .AddModifier(new RDFLimitModifier(5))
            //    .AddProjectionVariable(y)
            //    .AddProjectionVariable(x)
            //    .AddProjectionVariable(n);            

            //// APPLY SELECT QUERY TO GRAPH
            //RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);
            return resources;
        }

        /// <summary>
        /// Query an RDF graph 
        /// </summary>
        public static void RDFQuery1(this RDFGraph graph, List<string> commands)
        {
            //// CREATE SELECT QUERY
            //RDFSelectQuery selectQuery = new RDFSelectQuery();

            //// example
            //RDFSelectQuery query = new RDFSelectQuery() 
            //    .AddPrefix(RDFNamespaceRegister.GetByPrefix("dc")) 
            //    .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf")) 
            //    .AddPatternGroup(new RDFPatternGroup("PG1") 
            //    .AddPattern(new RDFPattern(y, dogOf, x)) 
            //    .AddPattern(new RDFPattern(x, name,n).Optional())
            //    .AddPattern(new RDFPattern(x, knows, h))
            //    .AddFilter(new RDFRegexFilter(n, new Regex(@"Mouse",RegexOptions.IgnoreCase)))) 
            //    .AddModifier(new RDFOrderByModifier(y,RDFQueryEnums.RDFOrderByFlavors.DESC))
            //    .AddModifier(new RDFLimitModifier(5))
            //    .AddProjectionVariable(y)
            //    .AddProjectionVariable(x)
            //    .AddProjectionVariable(n);            

            //// APPLY SELECT QUERY TO GRAPH
            //RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

        }

        /// <summary>
        /// Save Query results to a file e.g. "C:\select_results.srq"
        /// </summary>
        public static Boolean ExportRDFQueryResults(this RDFSelectQueryResult selectQueryResult, string filename)
        {
            var result = false;
            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            try
            {
                selectQueryResult.ToSparqlXmlResult(filename); // e.g. "C:\select_results.srq"
                result = true;
            }
            catch (Exception) { throw new Exception("Couldn't write queryresults to file with RDFSHARP API."); }

            return result;
        }

        /// <summary>
        /// Import Query results from a file e.g. "C:\select_results.srq"
        /// </summary>
        public static RDFSelectQueryResult ImportRDFQueryResults(string filename)
        {
            // IMPORT SELECT QUERY RESULTS FROM SPARQL XML FORMAT (FILE)
            var selectQueryResult = RDFSelectQueryResult.FromSparqlXmlResult(filename); // e.g. "C:\select_results.srq"

            return selectQueryResult;
        }
        #endregion
    }
}
