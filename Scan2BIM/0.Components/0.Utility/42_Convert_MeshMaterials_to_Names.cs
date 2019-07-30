using System;
using System.Collections.Generic;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Commands;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class _42_Convert_MeshMaterials_to_Names : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _42_Convert_MeshMaterials_to_Names class.
        /// </summary>
        public _42_Convert_MeshMaterials_to_Names()
          : base("_42_Convert_MeshMaterials_to_Names", "Materials to Names",
              "Convert all the selected meshes their materials to their object names",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Run", GH_ParamAccess.item); pManager[0].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /// i/o param
            /// 
            var run = false;

            
            /// input
            if (!DA.GetData("Run", ref run)) return;

            if (run == true)
            {
                ///internal params
                const ObjectType filter = Rhino.DocObjects.ObjectType.Mesh;
                Rhino.DocObjects.ObjRef[] objref;
                var new_object_name = "";

                /// select all meshes in doc            
                Rhino.Input.RhinoGet.GetMultipleObjects("", true, filter, out objref);

                /// access their materials
                /// 
                for (int i = 0; i < objref.Length; i++)
                {
                    var rhino_object = objref[i].Object();
                    new_object_name = rhino_object.GetMaterial(true).ToString();

                    /// set their names
                    /// 
                    rhino_object.Attributes.Name = new_object_name;
                    rhino_object.CommitChanges();
                }

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
            get { return new Guid("bf8e1586-f9e3-4a83-8cdd-bb1a29b8c811"); }
        }
    }
}