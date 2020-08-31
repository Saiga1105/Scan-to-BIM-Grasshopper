using System;
using System.Collections.Generic;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Segmentation;
using System.Linq;
using Volvox_Instr;
using Volvox_Cloud;

namespace Scan2BIM
{
    public class Decompose_transformation_matrix : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_component class.
        /// </summary>
        /// 
        public Decompose_transformation_matrix()
          : base("Decompose transformation matrix", "Decomposition",
              "Decompose [16,1] transformation matrix to translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]",
              "Saiga", "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// input parameters
            pManager.AddNumberParameter("tform", "T", " Rigid Transformation matrix [16,1] ", GH_ParamAccess.list); pManager[0].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /// ouput indices vertices mesh conform segmentation
            pManager.AddVectorParameter("R", "R", "rotation {Rx,Ry,Rz}  ", GH_ParamAccess.item);
            pManager.AddVectorParameter("t", "t", "translation {X,Y,Z}", GH_ParamAccess.item);
        }
        public class Size_Exception : Exception
        {
            public Size_Exception(string message) : base(message) { }
           
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ///input parameters
            List<double> tform = new List<double>();

            /// initialise parameters

            ///import 
            if (!DA.GetDataList("tform",  tform)) return;

            /// 0. error catching 
            /// 1. decompose tform into translation 
            /// 2. decompose tform in euler XYZ rotations 
            /// 4. convert function output (list with indices) to .Net array (Matlab => .Net)
            /// 5. convert array to rhinocommon list. (.Net => Rhinocommon)
            /// 6. output data

            ///0.
            if (tform.Count !=16)
            {
                throw new Size_Exception(string.Format("Need[16, 1] parameters"));
            }
            /// 1. 
            Vector3d T = new Vector3d(tform[3], tform[7], tform[11]);

            /// 2.
            var param = new MWNumericArray(16, 1, tform.ToArray());

            Segmentation.segment segment_mesh = new Segmentation.segment();
            MWArray rotXYZ = new MWNumericArray();
            rotXYZ = segment_mesh.G_rotm2eul(param);

            /// 4.
            MWNumericArray na = (MWNumericArray)rotXYZ;
            double[] dc = (double[])na.ToVector(0);

            /// 5.
            var Rhino_rotXYZ = new List<double>(dc);
            Vector3d R = new Vector3d(Rhino_rotXYZ[0], Rhino_rotXYZ[1], Rhino_rotXYZ[2]);

            /// 6.
            DA.SetData("R", R);
            DA.SetData("t", T);


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
            get { return new Guid("8a667810-210d-41b7-b78e-19adc0485f7e"); }
        }
    }
}