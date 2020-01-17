using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
//using GH_IO;
using Rhino.Geometry;

namespace Scan2BIM.Components.Topology
{
    public class delete_branch_occurences : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public delete_branch_occurences()
          : base("delete_branch_occurences", "branch occurences",
              "Given a dataTree (point3D), only keep the first branch that contains unique members of the dataTree ",
              "Saiga", "Topology")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// <summary>
            /// Registers all the input parameters for this component.
            /// Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
            /// </summary>

             pManager.AddPointParameter("dataTree", "Tree", "DataTree to operate on", GH_ParamAccess.tree);
             //pManager.AddNumberParameter("Number", "x", "Descr. of Number", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// <summary>
            /// Registers all the output parameters for this component.
            /// Use the pManager object to register your output parameters.
            /// Output parameters do not have default values, but they too must have the correct access type.
            /// </summary>
 
            pManager.AddPointParameter("uniquedataTree", "UTree", "Tree containing only branches ith unique elements", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Out", "O", "Descr. of Out", GH_ParamAccess.list);
        }

             protected override void SolveInstance(IGH_DataAccess DA)
        {
            /// <summary>
            /// This is the method that actually does the work.
            /// </summary>
            /// 
            /// First, we need to retrieve all data from the input parameters.
            /// We'll start by declaring variables and assigning them starting values.  
            /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
              
            // datatree should be accessed through GH_structure (grasshopper.kernal.data namespace)
            GH_Structure<GH_Point> winnerTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Point> dataTree = new GH_Structure<GH_Point>(); ;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetDataTree(0, out dataTree)) return;
  
            // body of code

            while (dataTree.PathCount != 0)
            {
                var pointlist = dataTree.Branches[0].GetRange(0, dataTree.Branches[0].Count);
                // copy data from first branch to winner
                winnerTree.EnsurePath(dataTree.Paths[0]);
                winnerTree.AppendRange(pointlist);
                dataTree.RemovePath(dataTree.Paths[0]);
                // test other branches for inliers with winner(0), if so delete them
                for (int i = 0; i < pointlist.Count; i++)
                {
                    var pointtest = pointlist[i];
                    for (int branchj = 0; branchj < dataTree.PathCount; branchj++)
                    {
                        for (int listj = 0; listj < dataTree.Branches[branchj].Count; listj++)
                        {
                            if (((pointtest.Value.X - dataTree.get_DataItem(dataTree.Paths[branchj], listj).Value.X) < 0.001) && ((pointtest.Value.Y - dataTree.get_DataItem(dataTree.Paths[branchj], listj).Value.Y) < 0.001) && ((pointtest.Value.Z - dataTree.get_DataItem(dataTree.Paths[branchj], listj).Value.Z) < 0.001))
                            {
                                dataTree.RemovePath(dataTree.Paths[branchj]);
                            }

                        }
                    }

                }
            }

            /// run some function on the data

            //// Finally assign the output to the output parameter.
            DA.SetDataTree(0, winnerTree);
            //DA.SetDataList(1, output);

        }


        protected override System.Drawing.Bitmap Icon
        {
            /// <summary>
            /// Provides an Icon for the component.
            /// </summary>
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
            get { return new Guid("cc930c14-203f-418a-b2cb-7b73603dda18"); }
        }
    }
}