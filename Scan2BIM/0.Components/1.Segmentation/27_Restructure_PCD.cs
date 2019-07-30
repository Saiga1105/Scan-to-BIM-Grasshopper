using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Display;
using Volvox_Cloud;

namespace Scan2BIM
{
    public class Restructure_PCD : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Restructure_PCD()
          : base("Restructure_PCD1", "Restructure",
              "Restructure a Point Cloud according to the computed indices of the regions (E.g. Region_Growing_PCD) ",
              "Saiga", "Segmentation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Point_Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddNumberParameter("Indices", "indices", "Indices equal to Point_Cloud.Count (output E.g. Region_Growing_PCD) ", GH_ParamAccess.list); pManager[1].Optional = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Sub_Clouds", "sPCD", "Segmented Point Cloud Data", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
            PointCloud Rhino_Cloud = new PointCloud();
            List<double> Rhino_indices = new List<double>();
            List<PointCloud> Sub_Clouds = new List<PointCloud>();
            List<GH_Cloud> GH_subclouds = new List<GH_Cloud>();

            /// read inputs
            if (!DA.GetData("Point_Cloud", ref Rhino_Cloud)) return;
            if (!DA.GetDataList<double>("Indices", Rhino_indices)) return;

            /// define internal parameters
            int cluster_count = (int)Rhino_indices.Max();

            ///1. maak lijsten van idexen per value van Rhino-indices
            ///2. Retrieve het punt van iedere index
            ///3. Add punt aan nieuwe cloud
            ///7. output

            var Rhino_xyz = Rhino_Cloud.GetPoints(); 
            var Rhino_n = Rhino_Cloud.GetNormals();
            var Rhino_c = Rhino_Cloud.GetColors();

            for (int i = 0; i < cluster_count; i++)
            {
                PointCloud Rhino_subcloud = new PointCloud();

                ///1.
                var C_indexlist = Enumerable.Range(0, Rhino_indices.Count).Where(flap => Rhino_indices[flap] == i + 1).ToList();
                ///2.


                if (Rhino_Cloud.ContainsNormals == true && Rhino_Cloud.ContainsColors == true)
                {
                    for (int j = 0; j < C_indexlist.Count; j++)
                    {
                        ///3.
                        Rhino_subcloud.Add(Rhino_xyz[C_indexlist[j]], Rhino_n[C_indexlist[j]], Rhino_c[C_indexlist[j]]);


                    }
                }
                if (Rhino_Cloud.ContainsNormals == true && Rhino_Cloud.ContainsColors == false)
                {
                    for (int j = 0; j < C_indexlist.Count; j++)
                    {
                        ///3.
                        Rhino_subcloud.Add(Rhino_xyz[C_indexlist[j]], Rhino_n[C_indexlist[j]]);


                    }
                }
                if (Rhino_Cloud.ContainsNormals == false && Rhino_Cloud.ContainsColors == true)
                {
                    for (int j = 0; j < C_indexlist.Count; j++)
                    {
                        ///3.
                        Rhino_subcloud.Add(Rhino_xyz[C_indexlist[j]], Rhino_c[C_indexlist[j]]);


                    }
                }


                ///7.
                GH_Cloud GH_subcloud = new GH_Cloud(Rhino_subcloud);
                GH_subclouds.Add(GH_subcloud);


            }

            ///output
            DA.SetDataList(0, GH_subclouds);
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
            get { return new Guid("3d844523-2b8e-41ae-b289-360bcb65ac68"); }
        }
    }
}