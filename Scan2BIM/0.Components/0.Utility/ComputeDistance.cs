using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using Rhino.Display;


namespace Scan2BIM
{
    public class ComputeDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeDistance class.
        /// </summary>
        public ComputeDistance()
          : base("Distance to Mesh/Brep", "Distance to Mesh/Brep",
              "Compute the distance between a Point Cloud and a Mesh/Brep. optionally colorize and cull outlier points",
              "Saiga", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); pManager[0].Optional = false;
            pManager.AddGenericParameter("Geometry", "Geometry", "reference geometry for the distance calculation i.e. Brep, Mesh or Point Cloud", GH_ParamAccess.item); pManager[1].Optional = false;
            pManager.AddIntervalParameter("Interval", "Interval", "optional interval to cull distane outliers of point cloud", GH_ParamAccess.item, new Interval(0.0, 1.0)); pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_PointCloudParam(), "Point Cloud", "PCD", "Point Cloud data", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //define i/o parameters
            GH_PointCloud pc = new GH_PointCloud();
            Object geometry = null;
            Interval interval = new Interval(0.0, 1.0);

            // read inputs
            if (!DA.GetData(0, ref pc)) return;
            if (!DA.GetData(1, ref geometry)) return;
            if (!DA.GetData(2, ref interval)) return;
                       
            // Exceptions
            if (!pc.IsValid) throw new Exception("Invalid Point Cloud");
            if (pc.ContainsDistances) pc.ClearDistances();

            //typecasting => maybe some more types can be included?
            var type = geometry.GetType();

            GH_PointCloud gH_PointCloud = null;
            GH_Mesh mesh = null;
            GH_Brep brep = null;

            if (type == typeof(GH_PointCloud))
            {
                gH_PointCloud = (GH_PointCloud)geometry;
                pc.DistanceTo(gH_PointCloud);
            }
            else if (type == typeof(PointCloud))
            {
                gH_PointCloud.Value = (PointCloud)geometry;
                pc.DistanceTo(gH_PointCloud);
            } 
            else if (type == typeof(Brep) )
            {
                pc.DistanceTo((Brep)geometry);
            }
            else if (type == typeof(GH_Brep))
            {
                brep = (GH_Brep)geometry;
                pc.DistanceTo(brep.Value);
            }
            else if (type == typeof(Mesh))
            {
                pc.DistanceTo((Mesh)geometry);
            }
            else if (type == typeof(GH_Mesh))
            {
                mesh = (GH_Mesh)geometry;
                pc.DistanceTo(mesh.Value);
            }
            else throw new Exception("Only submit GH_PointCloud, PointCloud, Brep or Mesh geometry. \n If it's another type of point cloud (e.g. Volvox, Tarsier, etc.) \n just pass it through a normal Grasshopper cloud parameter first.");

            // filter point cloud based on distance
            GH_PointCloud pc_out = null;
            if (!pc.ContainsDistances) throw new Exception("Something went wrong with the distances");

            var indices = pc.Distances
                .Select((distance, index) => index)
                .Where(index => interval.IncludesParameter(pc.Distances[index], false)); // this is superfast

            pc_out = pc.GetSubsection(indices);
            
            // Output
            DA.SetData(0, pc_out);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icon_DistanceToPointCloud;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d3b73e04-c6a3-4df5-a0c8-05c745ce4dc4"); }
        }
    }
}