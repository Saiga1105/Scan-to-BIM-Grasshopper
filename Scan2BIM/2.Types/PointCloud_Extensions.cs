using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;
using Scan2BIM_Matlab;

namespace Scan2BIM
{
    /// <summary>
    /// PointCloud class extension methods for Scan2BIM.
    /// </summary>
    public static class PointCloud_Extensions
    {
        /// <summary>
        /// Compute normals for a point cloud
        /// </summary>
        public static void ComputeNormals( this PointCloud pointCloud, int k)
        {
            // check type
            if (!pointCloud.IsValid) { return; }
            // check dimensions
            if (pointCloud.Count <= k) { return; }

            /// interal parameters 
            MWNumericArray mWNumericArray = new MWNumericArray();

            if (pointCloud.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArray))
            {
                Scan2BIM_Matlab.General general = new Scan2BIM_Matlab.General();
                MWNumericArray matlabNormals = new MWNumericArray();
                matlabNormals = (MWNumericArray)general.S2B_ComputeNormals(mWNumericArray, k);

                Vector3d[] rhinoNormals = new Vector3d[matlabNormals.Dimensions[0]];
                if (matlabNormals.ToVector3d(ref rhinoNormals))
                {
                    for (int i = 0; i < rhinoNormals.Length; i++)
                    {
                        pointCloud[i].Normal = rhinoNormals[i];
                    }
                }
            }
            return;
        }

        public static void AddColors(this PointCloud pointCloud, List<GH_Colour> colours)
        {
            // check type
            if (!pointCloud.IsValid) { return; }
            // check dimensions
            if (pointCloud.Count != colours.Count) { return; }

            for (int i = 0; i < colours.Count; i++)
            {
                //pointCloud[i].Color = new System.Drawing.Color.FromArgb(colours[i].Value.A, colours[i].Value.R, colours[i].Value.G, colours[i].Value.B);
            }
        }
    }
}
