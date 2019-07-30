using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Text;

namespace Scan2BIM
{
    public class Write_features1 : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Write_features1()
          : base("Write Matlab Predictors", "I/O",
              "Save Rhino Predictor values to a file or add them to an existing file",
              "Saiga", "Classification")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("write?", "B", "Label of mesh", GH_ParamAccess.item, false); pManager[0].Optional = false;
            pManager.AddTextParameter("Label", "L", "Label of mesh", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddNumberParameter("Area", "A", "Area of mesh", GH_ParamAccess.list); pManager[2].Optional = true;
            pManager.AddNumberParameter("NormalSimilarity", "N", "Normal of mesh", GH_ParamAccess.list); pManager[3].Optional = true;
            pManager.AddNumberParameter("NormalZ", "Nz", "NormalZ of mesh", GH_ParamAccess.list); pManager[4].Optional = true;
            pManager.AddNumberParameter("DiagonalXY", "D", "Horizontal diagonal of mesh", GH_ParamAccess.list); pManager[5].Optional = true;
            pManager.AddNumberParameter("Height", "H", "height in Z-direction of mesh", GH_ParamAccess.list); pManager[6].Optional = true;
            pManager.AddNumberParameter("Coplanarity", "C", "Normal distance to 5 closest large meshes (filtered by coplanarity)", GH_ParamAccess.list); pManager[7].Optional = true;
            pManager.AddNumberParameter("Proximity", "P", "Edge to Edge distance to closest large mesh", GH_ParamAccess.list); pManager[8].Optional = true;
            pManager.AddNumberParameter("Connections", "Con", "# connections with neighbourhing meshes", GH_ParamAccess.list); pManager[9].Optional = true;
            pManager.AddNumberParameter("Wallinlier", "I", "Inlier of bounding box of potential walls (large vertical meshes)", GH_ParamAccess.list); pManager[10].Optional = true;
            pManager.AddNumberParameter("DvBottom", "DvB", "Distance to closest underlying large mesh", GH_ParamAccess.list); pManager[11].Optional = true;
            pManager.AddNumberParameter("DvTop", "DvT", "Distance to closest overlying large mesh", GH_ParamAccess.list); pManager[12].Optional = true;
            //pManager.AddNumberParameter("ProxXY", "Pxy", "Proximity of closest large vertical mesh", GH_ParamAccess.list);
            pManager.AddNumberParameter("ColAbove", "ColA", "Boolean collision 1.5m with large surfave above (0 for no hit, floors & roofs)", GH_ParamAccess.list); pManager[13].Optional = true;
            pManager.AddNumberParameter("ColBelow", "ColB", "Boolean collision 1.5m with large surfave below (0 for no hit, ceilings)", GH_ParamAccess.list); pManager[14].Optional = true;
            pManager.AddNumberParameter("ColFarAbove", "ColFA", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[15].Optional = true;
            pManager.AddNumberParameter("Vbot", "Vbot", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[16].Optional = true;
            pManager.AddNumberParameter("Vtop", "Vtop", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[17].Optional = true;
            pManager.AddNumberParameter("Raytrace", "Ray", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[18].Optional = true;
            //pManager.AddNumberParameter("Z", "Z", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list);
            pManager.AddTextParameter("Path","Path","Path+name to where the folder should be saved", GH_ParamAccess.item); pManager[19].Optional = false;
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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //string GHname=null;
            bool boolean = false;
            List<string> GHLabel = new List<string>();
            List<double> GHArea = new List<double>();
            List<double> GHNormalsimilarity = new List<double>();
            List<double> GHNormalZ = new List<double>();
            List<double> GHDiagonalXY = new List<double>();
            List<double> GHHeight = new List<double>();
            List<double> GHCoplanarity = new List<double>();
            List<double> GHProximity = new List<double>();
            List<double> GHConnections = new List<double>();
            List<double> GHWallinlier = new List<double>();
            List<double> GHDvBottom = new List<double>();
            List<double> GHDvTop = new List<double>();
            //List<double> GHProxXY = new List<double>();
            List<double> GHColAbove = new List<double>();
            List<double> GHColBelow = new List<double>();
            List<double> GHColFarAbove = new List<double>();
            List<double> GHVbot = new List<double>();
            List<double> GHVtop = new List<double>();
            List<double> GHRaytrace = new List<double>();
            string name = "";
            //List<double> GHZ = new List<double>();
            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            if (!DA.GetData(0, ref boolean)) return;
            if (!DA.GetDataList(1, GHLabel)) return;
            if (!DA.GetDataList(2, GHArea)) return;
            if (!DA.GetDataList(3, GHNormalsimilarity)) return;
            if (!DA.GetDataList(4, GHNormalZ)) return;
            if (!DA.GetDataList(5, GHDiagonalXY)) return;
            if (!DA.GetDataList(6, GHHeight)) return;
            if (!DA.GetDataList(7, GHCoplanarity)) return;
            if (!DA.GetDataList(8, GHProximity)) return;
            if (!DA.GetDataList(9, GHConnections)) return;
            if (!DA.GetDataList(10, GHWallinlier)) return;
            if (!DA.GetDataList(11, GHDvBottom)) return;
            if (!DA.GetDataList(12, GHDvTop)) return;
            //if (!DA.GetDataList(13, GHProxXY)) return;
            if (!DA.GetDataList(13, GHColAbove)) return;
            if (!DA.GetDataList(14, GHColBelow)) return;
            if (!DA.GetDataList(15, GHColFarAbove)) return;
            if (!DA.GetDataList(16, GHVbot)) return;
            if (!DA.GetDataList(17, GHVtop)) return;
            if (!DA.GetDataList(18, GHRaytrace)) return;
            if (!DA.GetData(19, ref name)) return;
            //if (!DA.GetDataList(20, GHZ)) return;


            ///string test = @"D:\Google Drive\Research\Grasshopper Plugin Scan-to-BIM\Classification\Predictors\test.csv";
            if (boolean == true)
            {
                if (!File.Exists(name))
                {
                    File.Create(name).Close();
                }

                var csv = new StringBuilder();

                for (int i = 0; i < GHArea.Count; i++)
                {
                    var v1 = GHLabel[i].ToString();
                    var v2 = GHArea[i].ToString();
                    var v3 = GHNormalsimilarity[i].ToString();
                    var v4 = GHNormalZ[i].ToString();
                    var v5 = GHDiagonalXY[i].ToString();
                    var v6 = GHHeight[i].ToString();
                    var v7 = GHCoplanarity[i].ToString();
                    var v8 = GHProximity[i].ToString();
                    var v9 = GHConnections[i].ToString();
                    var v10 = GHWallinlier[i].ToString();
                    var v11 = GHDvBottom[i].ToString();
                    var v12 = GHDvTop[i].ToString();
                    //var v13 = GHProxXY[i].ToString();
                    var v13 = GHColAbove[i].ToString();
                    var v14 = GHColBelow[i].ToString();
                    var v15 = GHColFarAbove[i].ToString();
                    var v16 = GHVbot[i].ToString();
                    var v17 = GHVtop[i].ToString();
                    var v18 = GHRaytrace[i].ToString();
                    //var v20 = GHZ[i].ToString();
                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}", v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18);
                    csv.AppendLine(newLine);
                }

                File.AppendAllText(name, csv.ToString());
            }

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{4210bccb-7f6f-4125-a23e-4303fbafc35d}"); }
        }
    }
}
