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
using Rhino;

namespace Scan2BIM
{
    public class _61_write_JSON_buildings : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public _61_write_JSON_buildings()
          : base("61_write_JSON_buildings", "JSON Building",
              "Write a set of buildings (e.g. building1, ...) consisting of a set of objects (e.g. wall) to a .JSON file",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "C:/...sjon", "location and filename of the final JSON", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddTextParameter("Building Names", "Buildings", "List of building names to publish as described in the layers", GH_ParamAccess.list); pManager[2].Optional = false;
            pManager.AddBooleanParameter("Run", "R", "Publish JSON", GH_ParamAccess.item); pManager[3].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "JSON", "Representation of output JSON as text", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /// input / output parameters
            ///List<GH_Guid> GH_guids = new List<GH_Guid>();
            ////List<Guid> guids = new List<Guid>();
            List<string> Rhino_buildings = new List<string>();
            Boolean run = false;
            string filepath = "";
            string output = "";

            ///import data
            if (!DA.GetData("File path", ref filepath)) return;
            if (!DA.GetDataList("Guid", Rhino_buildings)) return;
            if (!DA.GetData("Run", ref run)) return;
            /// 0. create proto file
            /// 1.write header
            /// 2. retrieve meshes
            /// 3.

            ///define local parameters

            if (run == true)
            {

            ///0.
            Building building = new Building();
            BuildingCollection building_collection = new BuildingCollection();

            ///1.
            string[] header =
            {
                "@prefix rdf: < http://www.w3.org/1999/02/22-rdf-syntax-ns#>.",
                "@prefix rdfs: < http://www.w3.org/2000/01/rdf-schema#> .",
                "@prefix owl: < http://www.w3.org/2002/07/owl#> .",
                "@prefix dc: < http://purl.org/dc/elements/1.1/> .",
                "@prefix xsd: < http://www.w3.org/2001/XMLSchema#> .",
                "@prefix dcterms: < http://purl.org/dc/terms/> .  ",
                "@prefix vann: < http://purl.org/vocab/vann/> .",
                "@prefix voaf: < http://purl.org/vocommons/voaf#> .",
                "@prefix foaf: < http://xmlns.com/foaf/0.1/>.",
                "@prefix bot: < https://w3id.org/bot#> .",
                "@prefix ifc: < http://www.buildingsmart-tech.org/ifcOWL/IFC4_ADD2#> .",
                "@prefix ifcowl: < https://w3id.org/bot/IFC4_ADD2Alignment#> .",
                "@prefix gr: http://purl.org/goodrelations/v1#.",
                "@prefix pto: < http://www.productontology.org/id/>."
            };
                //const ObjectType filter = Rhino.DocObjects.ObjectType.Mesh;
                //Rhino.DocObjects.ObjRef[] objref;
                //var new_object_name = "";

                ///// select all meshes in doc            
                //Rhino.Input.RhinoGet.GetMultipleObjects("", true, filter, out objref);
                

                ////find layer
                /// find all meshes on that layer
                /// compute json information
                foreach (string cluster in Rhino_buildings)
                {
                    var layer = Rhino.RhinoDoc.ActiveDoc.Layers.FindName(cluster);
                    Rhino.DocObjects.RhinoObject[] rhobjs = RhinoDoc.ActiveDoc.Objects.FindByLayer(layer);
                    List<RhinoObject> meshes = new List<RhinoObject>();
                    List<Guid> mesh_guids = new List<Guid>();
                    List<Point3d> mesh_centroids = new List<Point3d>();
                    List<Point3d> meshes_min = new List<Point3d>();
                    List<Point3d> meshes_max = new List<Point3d>();
                    foreach (RhinoObject mesh in rhobjs)
                    {
                        meshes.Add(mesh);
                        mesh_guids.Add(mesh.Id);
                        
                        meshes_min.Add(mesh.Geometry.GetBoundingBox(false).Min);
                        meshes_max.Add(mesh.Geometry.GetBoundingBox(false).Max);

                    }
                
            

                    building_collection.Buildings.Add( new Building()
                     {
                         GUID = layer.Id.ToString(),
                         Inst = b,
                         ProductquickfixBuilding = b,
                         FogquickfixasObj = "",
                         Rdfsquickfixlabel = b,
                         objectCount = meshes.Count,
                         building_max = meshes_max.Max().ToString(),
                         building_min = meshes_min.Min().ToString(),
                         BotquickfixBuilding = b,
                         Rdfquickfixtype = "http://www.productontology.org/id/Building",
                         botquickfixcontainsElement = mesh_guids.ToString(),
                     };
                    );
                }


            var writer = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(true));
            String json = "";
            using (var file = new StreamWriter(filepath))
            using (StringWriter sw = new StringWriter())
            {
                foreach (var l in header)
                {
                    sw.WriteLine(l);

                }

                writer.WriteValue(sw, building_collection);
                json = sw.ToString();
                json.Replace("quickfix", ":");
                file.Write(json);

            }

            output = json;

            ///output = file.ToString();

            DA.SetDataList(0, output);

            }
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
            get { return new Guid("acec5f57-028b-452f-99c5-3b921526566e"); }
        }
    }
}