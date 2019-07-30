using System;
using System.Collections.Generic;
using Rhino.DocObjects;
using Rhino;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects.Tables;
using Rhino.Input;
using Rhino.Commands;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace Scan2BIM
{
    public class Name_Extraction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Name_Extraction()
          : base("Name_Extraction", "Name Extraction",
              "Isolate indices of textfragments that contain a target string e.g. wall",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to search e.g. mesh names", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddTextParameter("String", "S", "target text fragments ", GH_ParamAccess.list); pManager[0].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("indices", "i", "indices of text fragments ", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            List<string> Rhino_text = new List<string>() ;
            List<string> Rhino_key = new List<string>();

            ///output parameters
            List<double> Rhino_indices = new List<double>() ;
            DataTree<double> Rhino_index = new DataTree<double>();


            ///import data
            if (!DA.GetDataList<string>("Text", Rhino_text)) return;
            if (!DA.GetDataList<string>("String", Rhino_key)) return;

            ///1.access rhino object name given the GH mesh
            ///2.compare mesh name to target string
            ///3.return meshes that partially comply

            ///local parameters
            ///1.

            ///2.
            for (int i = 0; i < Rhino_key.Count; i++)
            {
                GH_Path gH_Path = new GH_Path(i);

                for (int j = 0; j < Rhino_text.Count; j++)
                        {
                            if (Rhino_text[j].Contains(Rhino_key[i]))
                                            {
                                                ///Rhino_indices.Add(j);
                                                Rhino_index.Add(j, gH_Path);
                                                
                                            }
                        }
            }
            
                

            ///3.
            DA.SetDataTree(0, Rhino_index);

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
            get { return new Guid("bbdf5be5-f8c2-45f3-83fc-417b542a45cc"); }
        }
    }
}