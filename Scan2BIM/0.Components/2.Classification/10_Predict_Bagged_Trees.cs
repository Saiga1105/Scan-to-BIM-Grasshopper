using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Scan2BIM_Matlab;

namespace Scan2BIM
{
    public class Predict_Bagged_Trees : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Predict_Bagged_Trees()
          : base("Predict_Bagged_Trees", "Predict",
              "Classifies mesh geometry into floors, ceilings, roofs, walls, beams and clutter based on a pretrained Bagged Trees model and a set of double predictor values",
              "Saiga", "Classification")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Area", "A", "Area of mesh", GH_ParamAccess.list); pManager[0].Optional = false;
            pManager.AddNumberParameter("NormalSimilarity", "N", "Normal of mesh", GH_ParamAccess.list); pManager[1].Optional = false;
            pManager.AddNumberParameter("NormalZ", "Nz", "NormalZ of mesh", GH_ParamAccess.list); pManager[2].Optional = false;
            pManager.AddNumberParameter("DiagonalXY", "D", "Horizontal diagonal of mesh", GH_ParamAccess.list); pManager[3].Optional = false;
            pManager.AddNumberParameter("Height", "H", "height in Z-direction of mesh", GH_ParamAccess.list); pManager[4].Optional = false;
            pManager.AddNumberParameter("Coplanarity", "C", "Normal distance to 5 closest large meshes (filtered by coplanarity)", GH_ParamAccess.list); pManager[5].Optional = false;
            pManager.AddNumberParameter("Proximity", "P", "Edge to Edge distance to closest large mesh", GH_ParamAccess.list); pManager[6].Optional = false;
            pManager.AddNumberParameter("Connections", "Con", "# connections with neighbourhing meshes", GH_ParamAccess.list); pManager[7].Optional = false;
            pManager.AddNumberParameter("Wallinlier", "I", "Inlier of bounding box of potential walls (large vertical meshes)", GH_ParamAccess.list); pManager[8].Optional = false;
            pManager.AddNumberParameter("DvBottom", "DvB", "Distance to closest underlying large mesh", GH_ParamAccess.list); pManager[9].Optional = false;
            pManager.AddNumberParameter("DvTop", "DvT", "Distance to closest overlying large mesh", GH_ParamAccess.list); pManager[10].Optional = false;
            //pManager.AddNumberParameter("ProxXY", "Pxy", "Proximity of closest large vertical mesh", GH_ParamAccess.list);
            pManager.AddNumberParameter("ColAbove", "ColA", "Boolean collision 1.5m with large surfave above (0 for no hit, floors & roofs)", GH_ParamAccess.list); pManager[11].Optional = false;
            pManager.AddNumberParameter("ColBelow", "ColB", "Boolean collision 1.5m with large surfave below (0 for no hit, ceilings)", GH_ParamAccess.list); pManager[12].Optional = false;
            pManager.AddNumberParameter("ColFarAbove", "ColFA", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[13].Optional = false;
            pManager.AddNumberParameter("Vbot", "Vbot", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[14].Optional = false;
            pManager.AddNumberParameter("Vtop", "Vtop", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[15].Optional = false;
            pManager.AddNumberParameter("Raytrace", "Ray", "Boolean collision 15m with large surfave above (0 for no hit, roofs & outside)", GH_ParamAccess.list); pManager[16].Optional = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("labels", "L", "0=Floors,1=Ceilings,2=Roofs,3=Walls,4=Beams and 5= Clutter", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///define i/o parameters
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
            
            if (!DA.GetDataList<double>("Area", GHArea)) return;
            if (!DA.GetDataList<double>("NormalSimilarity", GHNormalsimilarity)) return;
            if (!DA.GetDataList<double>("NormalZ", GHNormalZ)) return;
            if (!DA.GetDataList<double>("DiagonalXY", GHDiagonalXY)) return;
            if (!DA.GetDataList<double>("Height", GHHeight)) return;
            if (!DA.GetDataList<double>("Coplanarity", GHCoplanarity)) return;
            if (!DA.GetDataList<double>("Proximity", GHProximity)) return;
            if (!DA.GetDataList<double>("Connections", GHConnections)) return;
            if (!DA.GetDataList<double>("Wallinlier", GHWallinlier)) return;
            if (!DA.GetDataList<double>("DvBottom", GHDvBottom)) return;
            if (!DA.GetDataList<double>("DvTop", GHDvTop)) return;
            if (!DA.GetDataList<double>("ColAbove", GHColAbove)) return;
            if (!DA.GetDataList<double>("ColBelow", GHColBelow)) return;
            if (!DA.GetDataList<double>("ColFarAbove", GHColFarAbove)) return;
            if (!DA.GetDataList<double>("Vbot", GHVbot)) return;
            if (!DA.GetDataList<double>("Vtop", GHVtop)) return;
            if (!DA.GetDataList<double>("Raytrace", GHRaytrace)) return;

            /// initialise internal parameters
            //convert C#List to C#Array and directly to MatlabArray
            var Area = new MWNumericArray(GHArea.Count, 1, GHArea.ToArray());
            var Normalsimilarity = new MWNumericArray(GHNormalsimilarity.Count, 1, GHNormalsimilarity.ToArray());
            var NormalZ = new MWNumericArray(GHNormalZ.Count, 1, GHNormalZ.ToArray());
            var DiagonalXY = new MWNumericArray(GHDiagonalXY.Count, 1, GHDiagonalXY.ToArray());
            var Height = new MWNumericArray(GHHeight.Count, 1, GHHeight.ToArray());

            var Coplanarity = new MWNumericArray(GHCoplanarity.Count, 1, GHCoplanarity.ToArray());
            var Proximity = new MWNumericArray(GHProximity.Count, 1, GHProximity.ToArray());
            var Connections = new MWNumericArray(GHConnections.Count, 1, GHConnections.ToArray());
            var Wallinlier = new MWNumericArray(GHWallinlier.Count, 1, GHWallinlier.ToArray());
            var DvBottom = new MWNumericArray(GHDvBottom.Count, 1, GHDvBottom.ToArray());

            var DvTop = new MWNumericArray(GHDvTop.Count, 1, GHDvTop.ToArray());
            var ColAbove = new MWNumericArray(GHColAbove.Count, 1, GHColAbove.ToArray());
            var ColBelow = new MWNumericArray(GHColBelow.Count, 1, GHColBelow.ToArray());
            var ColFarAbove = new MWNumericArray(GHColFarAbove.Count, 1, GHColFarAbove.ToArray());
            var Vbot = new MWNumericArray(GHVbot.Count, 1, GHVbot.ToArray());

            var Vtop = new MWNumericArray(GHVtop.Count, 1, GHVtop.ToArray());
            var Raytrace = new MWNumericArray(GHRaytrace.Count, 1, GHRaytrace.ToArray());

            Scan2BIM_Matlab.Classification classification = new Scan2BIM_Matlab.Classification();

            MWArray c = new MWNumericArray();
            c = classification.S2B_Predict_1(Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot, Vtop, Raytrace);

            //convert MatlabArray to C#Array to C#List
            MWNumericArray na = (MWNumericArray)c;
            double[] dc = (double[])na.ToVector(0);

            List<double> output = new List<double>(dc);

            //// Finally assign the output to the output parameter.

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
            get { return new Guid("d10063e8-a182-4289-8119-cc2c1525b112"); }
        }
    }
}