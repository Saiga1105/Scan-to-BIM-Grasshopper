using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;


namespace Scan2BIM
{
    /// <summary>
    /// GH_Convert extension methods
    /// </summary>
    public static class GH_Convert_Extensions
    {
        /// <summary>
        /// Convert data to GH_PointCloud
        /// </summary>
        public static bool ToGH_PointCloud(this object data, ref GH_PointCloud target, GH_Conversion conversion_level) // this is correct
        {
            switch(conversion_level) 
            {
                // directcast attempt
                case GH_Conversion.Primary:
                return  ToGH_PointCloud_Primary(RuntimeHelpers.GetObjectValue(data),ref target);
                // other implied cast attempts
                case GH_Conversion.Secondary:
                return  ToGH_PointCloud_Secondary(RuntimeHelpers.GetObjectValue(data), ref target);
                // Try primary first, followed by secondary attempts
                case GH_Conversion.Both:
                if (!ToGH_PointCloud_Primary(RuntimeHelpers.GetObjectValue(data), ref target))
                {
                    return ToGH_PointCloud_Secondary(RuntimeHelpers.GetObjectValue(data), ref target);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Direct cast to GH_PointCloud
        /// </summary>
        public static bool ToGH_PointCloud_Primary(object data, ref GH_PointCloud target)
        {
            //Abort immediately on bogus data.
            if (data == null) { return false; }
            //only attempt direct cast from PointCloud
            if (data.GetType() == typeof(PointCloud))
            {
                target = (GH_PointCloud)data;
            }
            return false;
        }
        
        /// <summary>
        /// Implied casts to GH_PointCloud
        /// </summary>
        public static bool ToGH_PointCloud_Secondary(object data, ref GH_PointCloud target)
        {
            return false;
        }

        /// <summary>
        /// Convert Point3d[] to MWNumericArray
        /// </summary>
        /// <param name="data">Point3d[] </param>
        /// <returns>Returns Mx3 MWNumericArray</returns>
        public static bool ToMWNumericArray(this Point3d[] data, ref MWNumericArray target) // this is correct
        {
            // check type
            if (data.GetType()!= typeof(Point3d[])){return false;}

            // don't worry about internal mtehod variables, as they are automatically flagged for Garbage Collection (GC)
            double[] points = new double[data.Length*3];
            for (int i=0;i<data.Length;i++)
            {
                points[i*3] = data[i].X;
                points[i*3+1] = data[i].Y;
                points[i*3+2] = data[i].Z;
            }
            target = new MWNumericArray(data.Length, 3, points);
            return true;
        }
        /// <summary>
        /// Convert Point3d[] to MWNumericArray
        /// </summary>
        /// <param name="data">Point3d[] </param>
        /// <returns>Returns Mx3 MWNumericArray</returns>
        public static bool ToMWNumericArray(this List<Point3d> data, ref MWNumericArray target) // this is correct
        {
            // check type
            if (data.GetType() != typeof(List<Point3d>)) { return false; }

            // don't worry about internal mtehod variables, as they are automatically flagged for Garbage Collection (GC)
            double[] points = new double[data.Count * 3];
            for (int i = 0; i < data.Count; i++)
            {
                points[i * 3] = data[i].X;
                points[i * 3 + 1] = data[i].Y;
                points[i * 3 + 2] = data[i].Z;
            }
            target = new MWNumericArray(data.Count, 3, points);
            return true;
        }

        /// <summary>
        /// Convert Point3f[] to MWNumericArray
        /// </summary>
        /// <param name="data">Point3f[] </param>
        /// <returns>Returns Mx3 MWNumericArray</returns>
        public static bool ToMWNumericArray(this Point3f[] data, ref MWNumericArray target) // this is correct
        {
            // check type
            if (data.GetType() != typeof(Point3f[])) { return false; }

            float[] points = new float[data.Length*3];
            for (int i = 0; i < data.Length; i++)
            {
                points[i * 3] = data[i].X;
                points[i * 3 + 1] = data[i].Y;
                points[i * 3 + 2] = data[i].Z;
            }
            target = new MWNumericArray(data.Length, 3, points);
            return true;
        }

        public static bool ToPoint3d(this MWNumericArray data, ref Point3d[] target) // this is correct
        {
            // check type
            if (data.GetType() != typeof(MWNumericArray)) { return false; }
            // check dimensions
            if (data.Dimensions[0] != target.Length) { return false; }
            if (data.Dimensions[1] != 3) { return false; }

            //Point3d[] target = new Point3d[data.Dimensions[0]];
            //double[,] array2D = new double[1,1];
            for (int i = 0; i < target.Length; i++)
            {
                target[i].X = data[i+1,1].ToScalarDouble();
                target[i].Y = data[i+1,2].ToScalarDouble();
                target[i].Z = data[i+1,3].ToScalarDouble();

            }
            return true;
        }

        public static bool ToVector3d(this MWNumericArray data, ref Vector3d[] target) // this is correct
        {
            // check type
            if (data.GetType() != typeof(MWNumericArray)) { return false; }
            // check dimensions
            if (data.Dimensions[0] != target.Length) { return false; }
            if (data.Dimensions[1] != 3) { return false; }

            //Point3d[] target = new Point3d[data.Dimensions[0]];
            //double[,] array2D = new double[1,1];
            for (int i = 0; i < target.Length; i++)
            {
                target[i].X = data[i + 1, 1].ToScalarDouble();
                target[i].Y = data[i + 1, 2].ToScalarDouble();
                target[i].Z = data[i + 1, 3].ToScalarDouble();

            }
            return true;
        }
    }
}
