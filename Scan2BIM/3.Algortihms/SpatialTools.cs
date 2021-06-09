using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;
using Scan2BIM_Matlab;



namespace Scan2BIM
{
    /// <summary>
    /// Spatial operations  
    /// </summary>
    public static class SpatialTools
    {

        /// <summary>
        /// Convert data to GH_PointCloud
        /// </summary>
        public static DataTree<int> SplitVectors(this GH_Structure<GH_Vector> vectorTree, bool strict)
        {
            List<int> X_i = new List<int>();
            List<int> Y_i = new List<int>();
            List<int> Z_i = new List<int>();
            GH_Path pth = null;
            Vector3d vector3D = new Vector3d();
            DataTree<int> indices = new DataTree<int>();

            /// 1. decompose datatree
            for (int i = 0; i < vectorTree.PathCount; i++)
            {

                /// 2. convert GH_GOO structure to rhinocommon Vector3D
                pth = vectorTree.get_Path(i);
                var branch = vectorTree.Branches[i];
                List<Vector3d> V = new List<Vector3d>();

                foreach (GH_Vector thisGHVector in branch)
                {
                    GH_Convert.ToVector3d(thisGHVector, ref vector3D, 0);
                    V.Add(vector3D);
                }

                /// 3. querry list
                if (strict == false)
                {
                    X_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].X) >= Math.Abs(V[index].Y) && Math.Abs(V[index].X) >= Math.Abs(V[index].Z))
                    .ToList();

                    Y_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Y) >= Math.Abs(V[index].X) && Math.Abs(V[index].Y) >= Math.Abs(V[index].Z))
                    .ToList();

                    Z_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Z) >= Math.Abs(V[index].X) && Math.Abs(V[index].Z) >= Math.Abs(V[index].Y))
                    .ToList();
                }
                else
                {
                    var a = 1 / Math.Sqrt(3);
                    X_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].X) >= a)
                    .ToList();

                    Y_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Y) >= a)
                    .ToList();

                    Z_i = V
                    .Select((v, index) => index)
                    .Where(index => Math.Abs(V[index].Z) >= a)
                    .ToList();
                }


                /// 4. create new datatree
                var path = pth.Indices.ToList();
                path.Add(0);
                var p = new GH_Path(path.ToArray());
                indices.AddRange(X_i, p);

                path = pth.Indices.ToList();
                path.Add(1);
                p = new GH_Path(path.ToArray());
                indices.AddRange(Y_i, p);

                path = pth.Indices.ToList();
                path.Add(2);
                p = new GH_Path(path.ToArray());
                indices.AddRange(Z_i, p);
            }

            return indices;

        }

        /// <summary>
        /// Create [16,1] transformation matrix from translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]
        /// </summary>
        public static List<double> CreateTransformationMatrix(Vector3d R, Vector3d t)
        {
            // gather data 
            double a = R.Z;
            double b = R.Y;
            double c = R.X;
            double x = t.X;
            double y = t.Y;
            double z = t.Z;

            /// transformation
            List<double> tform = new List<double>();

            tform.Add(Math.Cos(a) * Math.Cos(b));
            tform.Add((Math.Cos(a) * Math.Sin(b) * Math.Sin(c) - Math.Sin(a) * Math.Cos(c)));
            tform.Add((Math.Cos(a) * Math.Sin(b) * Math.Cos(c) + Math.Sin(a) * Math.Sin(c)));
            tform.Add(x);

            tform.Add(Math.Sin(a) * Math.Cos(b));
            tform.Add((Math.Sin(a) * Math.Sin(b) * Math.Sin(c) + Math.Cos(a) * Math.Cos(c)));
            tform.Add((Math.Sin(a) * Math.Sin(b) * Math.Cos(c) - Math.Cos(a) * Math.Sin(c)));
            tform.Add(y);

            tform.Add(-Math.Sin(b));
            tform.Add(Math.Cos(b) * Math.Sin(c));
            tform.Add(Math.Cos(b) * Math.Cos(c));
            tform.Add(z);

            tform.Add(0.0);
            tform.Add(0.0);
            tform.Add(0.0);
            tform.Add(1.0);

            return tform;
        }

        /// <summary>
        /// Decompose [16,1] transformation matrix to translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]
        /// </summary>
        public static void DecomposeTransformationMatrix(this List<double> tform, out Vector3d R, out Vector3d t)
        {
            /// decompose tform into translation 
            t = new Vector3d(tform[3], tform[7], tform[11]);

            /// decompose tform in euler XYZ rotations 
            var param = new MWNumericArray(16, 1, tform.ToArray());

            Scan2BIM_Matlab.General general = new General();
            MWArray rotXYZ = general.S2B_rotm2eul(param);

            /// convert function output (list with indices) to .Net array (Matlab => .Net)
            MWNumericArray na = (MWNumericArray)rotXYZ;
            double[] dc = (double[])na.ToVector(0);

            /// convert array to rhinocommon list. (.Net => Rhinocommon)
            var Rhino_rotXYZ = new List<double>(dc);
            R = new Vector3d(Rhino_rotXYZ[0], Rhino_rotXYZ[1], Rhino_rotXYZ[2]);
        }

        /// <summary>
        /// Compute the Rigid body transformation [Rt] from point cloud A to B
        /// </summary>
        public static void RegisterPointClouds(out List<Double> tform, out Double rmse, ref GH_PointCloud pcA, ref GH_PointCloud pcB, Double metric = 0, Double inlierRatio = 0.6, Double maxIterations = 20, Double downsamplingA = 0.1, Double downsamplingB = 0.5)
        {
            uint nrSamplesA = (uint)(downsamplingA * pcA.Value.Count);
            uint nrSamplesB = (uint)(downsamplingB * pcB.Value.Count);

            var sampleA = pcA.Value.GetRandomSubsample(nrSamplesA);
            var sampleB = pcB.Value.GetRandomSubsample(nrSamplesB);


            MWNumericArray mWNumericArrayA = new MWNumericArray();
            MWNumericArray mWNumericArrayB = new MWNumericArray();

            if (!sampleA.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArrayA)) throw new Exception("ToMWNumericArray conversion error. Matlab runtime ok?");
            if (!sampleB.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArrayB)) throw new Exception("ToMWNumericArray conversion error. Matlab runtime ok?");

            Scan2BIM_Matlab.General general = new General();
            var mwca = (MWCellArray)general.S2B_ICP2(mWNumericArrayA, mWNumericArrayB, metric, inlierRatio, maxIterations);

            MWNumericArray na0 = (MWNumericArray)mwca[1];
            double[] dc0 = (double[])na0.ToVector(0);
            tform = new List<double>(dc0);

            MWNumericArray na1 = (MWNumericArray)mwca[2];
            rmse = (double)na1;
        }

        /// <summary>
        /// Random generator
        /// </summary>
        public static List<Double> RandomInt(int minValue = 0, int maxValue = 100, int nrSamples = 100)
        {
            var rand = new Random();
            var rtnlist = new List<double>();

            for (int i = 0; i < nrSamples; i++)
            {
                rtnlist.Add(rand.Next(minValue, maxValue));
            }
            return rtnlist;
        }

        /// <summary>
        /// Compute the best fit (least squares) planar surface through a mesh
        /// </summary>
        public static void FitPlanarSurface(this Mesh mesh, out Surface surface, out Double rmse, Double range=0.05, Double downsampling = 1.0)
        {
            List<Point3d> points = mesh.GetPoints();
            List<Point2d> inliers = new List<Point2d>();
            List<Double> inliers_s = new List<Double>();
            List<Double> inliers_t = new List<Double>();
            //GH_Brep gH_Brep = new GH_Brep();


            Plane.FitPlaneToPoints(points, out Plane plane, out double maximumDeviation);

            for (int i = 0; i < points.Count; i++) 
            {                
                if (Math.Abs(plane.DistanceTo(points[i]))<=range)
                {
                    plane.ClosestParameter(points[i], out Double s, out Double t);
                    Point2d point2D = new Point2d(s, t);
                    inliers.Add(point2D);
                }
            }
            Interval xExtents = new Interval(inliers_s.Min(), inliers_s.Max());
            Interval yExtents = new Interval(inliers_t.Min(), inliers_t.Max());

            PlaneSurface planeSurface = new PlaneSurface(plane, xExtents, yExtents);
            Rhino.Geometry.NurbsSurface.CreateCurveOnSurface(planeSurface, point2D,tolerance,true)
var nurbsCurve =
            BrepFace brepFace = new BrepFace(planeSurface,BrepTrim brepTrim);

        }

        /// <summary>
        /// Gather all the points of a mesh
        /// </summary>
        public static List<Point3d> GetPoints(this Mesh mesh)
        {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < mesh.Faces.Count; i++) /// retrieve best fit plane
            {
                points.Add(mesh.Faces.GetFaceCenter(i));

            }
            points.AddRange(mesh.Vertices.ToPoint3dArray());
            return points;
        }
    }
}


