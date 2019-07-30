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

namespace Scan2BIM
{
    public class _60_write_JSON_elements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _52_Cluster_Nodefeature_Wallthickness class.
        /// </summary>
        public _60_write_JSON_elements()
          : base("60_write_JSON_elements", "JSON El",
              "Write a set of building elements (e.g. walls, doors, ...) to a .JSON file",
              "Saiga", "Linked Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes of objects (e.g. walls, floors) that should be published as linked open data", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddTextParameter("File path", "C:/...sjon", "location and filename of the final JSON", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddTextParameter("Guid", "Guids", "GUIDs of the Meshes involved", GH_ParamAccess.list); pManager[2].Optional = false;

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
            List<Mesh> meshes = new List<Mesh>();
            List<GH_Guid> GH_guids = new List<GH_Guid>();
            List<Guid> guids = new List<Guid>();
            string filepath = "";
            string output = "";

            ///import data
            if (!DA.GetDataList("Meshes", meshes)) return;
            if (!DA.GetData("File path", ref filepath)) return;
            if (!DA.GetDataList("Guid", GH_guids)) return;

            /// 0. create proto file
            /// 1.write header
            /// 2. retrieve meshes
            /// 3.

            ///define local parameters


            ///0.
            Element element = new Element();
            ElementCollection element_collection = new ElementCollection();

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
            foreach (GH_Guid guid in GH_guids)
            {
                var a = new Guid(guid.ToString());
                guids.Add(a);
            }


            /// retrieve meshes
            List<RhinoObject> Rhino_meshes = new List<RhinoObject>();
            foreach (Guid Guid in guids)
            {
                Rhino_meshes.Add(Rhino.RhinoDoc.ActiveDoc.Objects.FindId(Guid));
            }

            var rhino_ID = new List<string>();
            
            element_collection.Elements.AddRange(meshes.DuoSelect(Rhino_meshes, (mesh, rhino_mesh) =>
             {
                 return new Element()
                 {
                     VertexCount = mesh.Vertices.Count,
                     FaceCount = mesh.Faces.Count,
                     Centroid = mesh.GetBoundingBox(false).Center.ToString(), /// retrieve centre
                     Min = mesh.GetBoundingBox(false).Min.ToString(),
                     Max = mesh.GetBoundingBox(false).Max.ToString(),

                     ID = rhino_mesh.Attributes.Name,
                     Rdfsquickfixlabel = rhino_mesh.Document.Layers[rhino_mesh.Attributes.LayerIndex].Name,
                     GUID = rhino_mesh.Id.ToString(),
                     BotquickfixElement = rhino_mesh.Attributes.Name,
                     Rdfquickfixtype = "http://www.productontology.org/id/Wall",
                 };
             }));

            //foreach (RhinoObject mesh in Rhino_meshes)
            //{
            //    rhino_ID.Add( mesh.Attributes.Name);
            //    var Rdfs333Label = mesh.Document.Layers[mesh.Attributes.LayerIndex].Name;
            //    var GUID = mesh.Id.ToString();
            //    var Bot333Element = mesh.Attributes.Name;
            //    var Rdf333Type = "http://www.productontology.org/id/Wall";
            //}

            //element_collection.Elements.AddRange(meshes.Select(mesh => new Element()
            //{

            //    VertexCount = mesh.Vertices.Count,
            //    FaceCount = mesh.Faces.Count,
            //    Centroid = mesh.GetBoundingBox(false).Center.ToString(), /// retrieve centre
            //    Min = mesh.GetBoundingBox(false).Min.ToString(),
            //    Max = mesh.GetBoundingBox(false).Max.ToString()
            //}));
            //element_collection.Elements.AddRange(Rhino_meshes.Select(mesh => new Element()
            //{
            //    ID = mesh.Attributes.Name,
            //    Rdfs333Label = mesh.Document.Layers[mesh.Attributes.LayerIndex].Name,
            //    GUID = mesh.Id.ToString(),
            //    Bot333Element = mesh.Attributes.Name,
            //    Rdf333Type = "http://www.productontology.org/id/Wall",
            //}));



            var writer = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(true));
            String json = "";
            using (var file = new StreamWriter(filepath))
            using (StringWriter sw = new StringWriter())
            {
                foreach (var l in header)
                {
                    sw.WriteLine(l);

                }

                writer.WriteValue(sw, element_collection);
                json = sw.ToString();
                json.Replace("333", ":");
                file.Write(json);

            }

            output = "test";

            ///output = file.ToString();

            DA.SetDataList(0, output);

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
            get { return new Guid("acec5f57-028b-452f-99c5-3b921526566d"); }
        }
    }
}