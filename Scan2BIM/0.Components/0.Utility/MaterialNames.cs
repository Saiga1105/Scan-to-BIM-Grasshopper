using System;
using System.Collections.Generic;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Commands;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Scan2BIM
{
    public class MaterialNames : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MaterialNames class.
        /// </summary>
        public MaterialNames()
          : base("Mesh materials names", "Names",
              "Convert mesh materials to their object names",
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
            // Define i/o parameters
            var run = false;

            // Read inputs
            if (!DA.GetData(0, ref run)) return;

            if (run == true)
            {
                ///internal params
                const ObjectType filter = ObjectType.Mesh;
                /// select all meshes in doc            
                RhinoGet.GetMultipleObjects("", true, filter, out ObjRef[] objref);

                /// access their materials
                /// 
                for (int i = 0; i < objref.Length; i++)
                {
                    var rhino_object = objref[i].Object();
                    string new_object_name = rhino_object.GetMaterial(true).ToString();

                    /// set their names
                    /// 
                    Name = new_object_name;
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
                return Properties.Resources.Icon_MatExtraction;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C07-412D-8FF8-B76439197730"); }
        }
    }
}