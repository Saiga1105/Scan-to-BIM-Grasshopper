using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Rhino.Geometry;
using Rhino.DocObjects.Custom;
using Rhino.Display;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MathWorks.MATLAB.NET.Arrays;
using Scan2BIM_Matlab;
using Loyc.Utilities;
using Loyc.Math;
using Loyc.Collections;
using MIConvexHull; // https://github.com/DesignEngrLab/MIConvexHull
using Accord.MachineLearning.Geometry; // http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
using Accord.Math;

namespace Scan2BIM
{
    /// <summary>
    /// Spatial operations  
    /// </summary>
    public static class SpatialTools
    {

        /// <summary>
        /// Split a datatree of vectors along each of the cardinal axes {X,Y,Z}
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

                foreach (GH_Vector thisGHVector in branch) // this seems an optional code. put in iff statement
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
        /// Create [4x4] transformation matrix from translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]
        /// </summary>
        public static Transform CreateTransformationMatrix(Vector3d R, Vector3d t)
        {
            // gather data 
            double a = R.Z;
            double b = R.Y;
            double c = R.X;
            double x = t.X;
            double y = t.Y;
            double z = t.Z;

            /// transformation
            Transform transform = new Transform();

            transform.M00 = Math.Cos(a) * Math.Cos(b);
            transform.M01 = (Math.Cos(a) * Math.Sin(b) * Math.Sin(c) - Math.Sin(a) * Math.Cos(c));
            transform.M02 = (Math.Cos(a) * Math.Sin(b) * Math.Cos(c) + Math.Sin(a) * Math.Sin(c));
            transform.M03 = x;

            transform.M10 = Math.Sin(a) * Math.Cos(b);
            transform.M11 = (Math.Sin(a) * Math.Sin(b) * Math.Sin(c) + Math.Cos(a) * Math.Cos(c));
            transform.M12 = (Math.Sin(a) * Math.Sin(b) * Math.Cos(c) - Math.Cos(a) * Math.Sin(c));
            transform.M13 = y;

            transform.M20 = -Math.Sin(b);
            transform.M21 = Math.Cos(b) * Math.Sin(c);
            transform.M22 = Math.Cos(b) * Math.Cos(c);
            transform.M23 = z;

            transform.M30 = 0.0;
            transform.M31 = 0.0;
            transform.M32 = 0.0;
            transform.M33 = 1.0;

            return transform;
        }

        /// <summary>
        /// Convert [xx4] transformation matrix to Array
        /// </summary>
        public static Double[] ToDoubleArray(this Transform transform)
        {
            List<Double> transformList = new List<double>();
            transformList.Add(transform.M00);
            transformList.Add(transform.M01);
            transformList.Add(transform.M02);
            transformList.Add(transform.M03);
            transformList.Add(transform.M10);
            transformList.Add(transform.M11);
            transformList.Add(transform.M12);
            transformList.Add(transform.M13);
            transformList.Add(transform.M20);
            transformList.Add(transform.M21);
            transformList.Add(transform.M22);
            transformList.Add(transform.M23);
            transformList.Add(transform.M30);
            transformList.Add(transform.M31);
            transformList.Add(transform.M32);
            transformList.Add(transform.M33);

            return transformList.ToArray();
        }

        /// <summary>
        /// Decompose [16,1] transformation matrix to translation vector {Tx,Ty,Tz} and Euler rotation vector {Rx,Ry,Rz} [radians]
        /// </summary>
        public static void DecomposeTransformationMatrix(this Transform transform, out Vector3d R, out Vector3d t)
        {
            /// decompose tform into translation 
            t = new Vector3d(transform.M03, transform.M13, transform.M23);

            /// decompose tform in euler XYZ rotations 
            var param = new MWNumericArray(16, 1, transform.ToDoubleArray());

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
        public static Transform ToTransform(this List<double> transformList)
        {
            if (transformList.Count != 16) throw new Exception("Only submit [16,1] lists");
            Transform transform = new Transform();
            transform.M00 = transformList[0];
            transform.M01 = transformList[1];
            transform.M02 = transformList[2];
            transform.M03 = transformList[3];
            transform.M10 = transformList[4];
            transform.M11 = transformList[5];
            transform.M12 = transformList[6];
            transform.M13 = transformList[7];
            transform.M20 = transformList[8];
            transform.M21 = transformList[9];
            transform.M22 = transformList[10];
            transform.M23 = transformList[11];
            transform.M30 = transformList[12];
            transform.M31 = transformList[13];
            transform.M32 = transformList[14];
            transform.M33 = transformList[15];
            return transform;
        }

        /// <summary>
        /// Compute the Rigid body transformation [Rt] from point cloud A to B
        /// </summary>
        public static void RegisterPointClouds(out Transform transform, out Double rmse, ref GH_PointCloud pcA, ref GH_PointCloud pcB, Double metric = 0, Double inlierRatio = 0.6, Double maxIterations = 20, Double downsamplingA = 0.1, Double downsamplingB = 0.5)
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
            var transformList = new List<double>(dc0);

            transform = transformList.ToTransform();

            MWNumericArray na1 = (MWNumericArray)mwca[2];
            rmse = (double)na1;
        }

        /// <summary>
        /// Random generator
        /// </summary>
        public static IEnumerable<Double> RandomInt(int minValue = 0, int maxValue = 100, int nrSamples = 100)
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
        /// Gather all the points of a mesh
        /// </summary>
        public static IEnumerable<Point3d> GetPoints(this Mesh mesh)
        {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < mesh.Faces.Count; i++) /// retrieve best fit plane
            {
                points.Add(mesh.Faces.GetFaceCenter(i));

            }
            points.AddRange(mesh.Vertices.ToPoint3dArray());
            return points;
        }

        /// <summary>
        /// Compute the standarddeviation of List<Double>
        /// </summary>
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        /// <summary>
        /// Area of a MeshFace
        /// </summary>
        public static double AreaOfTriangle(this Mesh mesh, int Faceindex)
        {
            double a = mesh.Vertices[mesh.Faces[Faceindex].A].DistanceTo(mesh.Vertices[mesh.Faces[Faceindex].B]);
            double b = mesh.Vertices[mesh.Faces[Faceindex].B].DistanceTo(mesh.Vertices[mesh.Faces[Faceindex].C]);
            double c = mesh.Vertices[mesh.Faces[Faceindex].C].DistanceTo(mesh.Vertices[mesh.Faces[Faceindex].A]);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        /// <summary>
        /// Get the plane from a MeshFace index
        /// </summary>
        public static Rhino.Geometry.Plane GetPlane(this Mesh mesh, int indexFace)
        {
            return new Rhino.Geometry.Plane(mesh.Vertices[mesh.Faces[indexFace].A], mesh.Vertices[mesh.Faces[indexFace].B], mesh.Vertices[mesh.Faces[indexFace].C]);
        }

        /// <summary>
        /// Get a random Barycentric coordinate on a mesh triangle [u,v,w]
        /// </summary>
        public static Vector3d RandomBarycentricCoordinate()
        {
            Random random = new Random();
            var vector = new Vector3d(random.Next(100), random.Next(100), random.Next(100));
            if (vector.Unitize()) return vector;
            else return new Vector3d(1 / 3, 1 / 3, 1 / 3);
        }

        /// <summary>
        /// Get a random U,V coordinate on a BrepFace [u,v]
        /// </summary>
        public static Vector2d RandomUVCoordinate()
        {
            Random random = new Random();
            var vector = new Vector2d(random.Next(100), random.Next(100));
            if (vector.Unitize()) return vector;
            else return new Vector2d(1 / 2, 1 / 2);
        }



        /// <summary>
        /// Isolate the Point Cloud points that lie within a Brep
        /// </summary>
        public static GH_PointCloud CropPointCloud(this GH_PointCloud pc, Brep brep)
        {
            GH_PointCloud pc_out = new GH_PointCloud();
            var points = pc.Value.AsReadOnlyListOfPoints();

            if (!brep.IsSolid) throw new Exception("Provide a closed Brep");
            if (!brep.IsManifold) throw new Exception("Provide a manifold Brep");

            for (int i = 0; i < pc.Value.Count; i++)
            {
                if (brep.IsPointInside(points[i], 0.0, false)) pc_out.Value.Append(pc.Value[i]); // this is a veeeerrry slow implementation
            }
            return pc_out;
        }

        /// <summary>
        /// Isolate the Point Cloud points that lie within a mesh
        /// </summary>
        public static GH_PointCloud CropPointCloud(this GH_PointCloud pc, Mesh mesh)
        {
            GH_PointCloud pc_out = new GH_PointCloud();
            var points = pc.Value.AsReadOnlyListOfPoints();

            if (!mesh.IsSolid) throw new Exception("Provide a closed Mesh");
            if (!mesh.IsManifold()) throw new Exception("Provide a manifold Mesh");

            for (int i = 0; i < pc.Value.Count; i++)
            {
                if (mesh.IsPointInside(points[i], 0.0, false)) pc_out.Value.Append(pc.Value[i]); // this is a veeeerrry slow implementation
            }
            return pc_out;
        }

        /// <summary>
        /// Distance between point cloud points and a Brep (distance overwrites the PointValue)
        /// </summary>
        public static void DistanceTo(this GH_PointCloud pc, Brep brep)
        {
            if (!brep.IsManifold) throw new Exception("Provide a manifold Mesh");
            var points = pc.Value.AsReadOnlyListOfPoints();
            Point3d point = Point3d.Unset;

            for (int i = 0; i < pc.Value.Count; i++)
            {
                point = brep.ClosestPoint(points[i]);
                if (point.IsValid)
                {
                    pc.Distances[i] = point.DistanceTo(points[i]); // this is a veeeerrry slow implementation
                }
            }
        }

        /// <summary>
        /// Distance between point cloud points and a Mesh (distance overwrites the PointValue)
        /// </summary>
        public static void DistanceTo(this GH_PointCloud pc, Mesh mesh)
        {
            if (!mesh.IsValid) throw new Exception("Provide a valid Mesh");
            var pcPoints = pc.Value.AsReadOnlyListOfPoints();
            Point3d meshPoint;
            
            for (int i = 0; i < pc.Value.Count; i++)
            {
                meshPoint = mesh.ClosestPoint(pcPoints[i]);
                if (meshPoint.IsValid)
                {
                    var temp= meshPoint.DistanceTo(pcPoints[i]); // this is not giving the proper results
                    pc.Distances[i] = temp; // this is a veeeerrry slow implementation
                }
            }
        }
        /// <summary>
        /// Distance between point cloud points and a GH_PointCloud (distance overwrites the PointValue)
        /// </summary>
        public static void DistanceTo(this GH_PointCloud pc, PointCloud pointCloud)
        {
            if (!pointCloud.IsValid) throw new Exception("Provide a valid GH_PointCloud");
            var points = pc.Value.AsReadOnlyListOfPoints();
            Point3d point = Point3d.Unset;
            int index = 0;

            for (int i = 0; i < pc.Value.Count; i++)
            {
                index = pointCloud.ClosestPoint(points[i]); // this is a veeeerrry slow implementation
                if (index != -1)
                {
                    pc.Distances[i] = pointCloud[index].Location.DistanceTo(points[i]); // this is a veeeerrry slow implementation
                }
            }
        }
        /// <summary>
        /// Generate samples on a point cloud according to a specific resolution (e.g. a point every 0.05m)
        /// </summary>
        public static GH_PointCloud GenerateSpatialCloud(this GH_PointCloud pc, double resolution = 0.05)
        {
            GH_PointCloud pc_out = new GH_PointCloud();

            var pcResolution = pc.ComputeResolution(0.01);
            double ratio = Math.Abs(resolution / pcResolution);
            if (ratio == 0 || ratio > 1) ratio = 0.5;

            var nrSamples = (uint)(ratio * pc.Value.Count);

            pc_out.Value = pc.Value.GetRandomSubsample(nrSamples);

            return pc_out;
        }
        /// <summary>
        /// Generate a nr of random samples on a mesh. The resolution is the nr of points/m²
        /// </summary>
        public static GH_PointCloud GenerateSpatialCloud(this Mesh mesh, double resolution = 100)
        {
            GH_PointCloud pc = new GH_PointCloud();
            //int nrSamples = (int)(AreaMassProperties.Compute(mesh).Area * resolution);

            int samplesPerTriangle = 0;
            Vector3d vector = new Vector3d();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                samplesPerTriangle = (int)(mesh.AreaOfTriangle(i) * resolution);
                if (samplesPerTriangle < 2) pc.Value.Add(mesh.Faces.GetFaceCenter(i));

                for (int j = 0; j < samplesPerTriangle; j++) // this is uncessery
                {
                    vector = RandomBarycentricCoordinate();
                    pc.Value.Add(mesh.PointAt(i, vector[0], vector[1], vector[2], 0.0));
                }
            }
            return pc;
        }
        /// <summary>
        /// Generate a nr of random samples on a Brep. The resolution is the nr of points/m²
        /// </summary>
        public static GH_PointCloud GenerateSpatialCloud(this Brep brep, double resolution = 100)
        {
            GH_PointCloud pc = new GH_PointCloud();
            //int nrSamples = (int)(AreaMassProperties.Compute(mesh).Area * resolution);

            int samplesPerFace = 0;
            Vector2d vector = new Vector2d();

            for (int i = 0; i < brep.Faces.Count; i++)
            {
                samplesPerFace = (int)(AreaMassProperties.Compute(brep.Faces[i]).Area * resolution);
                if (samplesPerFace < 2) pc.Value.Add(brep.Faces[i].PointAt(0.5, 0.5)); // this might nog be the right answer
                for (int j = 0; j < samplesPerFace; j++) // this is uncessery
                {
                    vector = RandomUVCoordinate();
                    pc.Value.Add(brep.Faces[i].PointAt(vector.X, vector.Y));
                }
            }
            return pc;
        }

        /// <summary>
        /// IEnumerable<PointCloudItem> pointCloudItems to PointCloud
        /// </summary>
        public static PointCloud ToPointCloud(this IEnumerable<PointCloudItem> pointCloudItems)
        {
            PointCloud pc = new PointCloud();
            foreach (PointCloudItem pointCloudItem in pointCloudItems)
            {
                pc.Append(pointCloudItem); // addrange is muuuuuch faster!
            }
            return pc;
        }

        /// <summary>
        /// Get point cloud from a mesh including the vertices, centroids and edge mids
        /// </summary>
        public static GH_PointCloud GetVertexCloud(this Mesh mesh)
        {
            GH_PointCloud gH_PointCloud = new GH_PointCloud();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                gH_PointCloud.Value.Add(mesh.Faces.GetFaceCenter(i)); // add centre
                gH_PointCloud.Value.Add(mesh.PointAt(i, 0, 0.5, 0.5, 0.0)); // add edgepoints
                gH_PointCloud.Value.Add(mesh.PointAt(i, 0.5, 0, 0.5, 0.0));
                gH_PointCloud.Value.Add(mesh.PointAt(i, 0.5, 0.5, 0, 0.0));
            }
            gH_PointCloud.Value.AddRange(mesh.Vertices.ToPoint3dArray()); // this makes it unstructured
            return gH_PointCloud;
        }

        /// <summary>
        /// Get a list of PointClouditems from a point cloud
        /// </summary>
        public static List<PointCloudItem> GetPointCloudItems(this GH_PointCloud pc, IEnumerable<int> indices)
        {
            List<PointCloudItem> pointCloudItems = new List<PointCloudItem>();

            for (int i = 0; i < indices.Count(); i++)
            {
                pointCloudItems.Add(pc.Value[i]);
            }
            return pointCloudItems;
        }

        /// <summary>
        /// Get a subcloud of GH_PointCloud
        /// </summary>
        public static GH_PointCloud GetSubsection(this GH_PointCloud pc, IEnumerable<int> indices)
        {
            GH_PointCloud pc_out = new GH_PointCloud();
            for (int i = 0; i < indices.Count(); i++)
            {
                pc_out.Value.Append(pc.Value[indices.ElementAt(i)]);
            }
            return pc_out;
        }

        /// <summary>
        /// Compute the contourpoints [GH_PointCloud] of a PointCloud
        /// </summary>
        public static Mesh ComputeConvexHull3D(this GH_PointCloud pc)
        {
            //pc_out = new GH_PointCloud();           

            var vertices3D = pc.Value.AsReadOnlyListOfPoints().ToVertex3D();
            var convexHullCreationResult3D = ConvexHull.Create(vertices3D);
            if (convexHullCreationResult3D.Outcome != 0) throw new Exception("3D Convex hull failed (are all the points on a plane?)");

            //var points3D = convexHullCreationResult3D.Result.Points.ToPoint3D();
            //pc_out.Value.AddRange(points3D);

            return convexHullCreationResult3D.Result.ToMesh();

            //var nurbsCurve3D = planeSurface.InterpolatedCurveOnSurface(contour3D, tolerance);
            ////if (!nurbsCurve3D.IsValid) throw new Exception("Couldn't create curve from 3D contourpoints. Maybe there are to few.");

        }

        /// <summary>
        /// Convert ConvexHull to a rhinocommon mesh
        /// </summary>
        public static Mesh ToMesh(this ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> convexhull)
        {
            Mesh mesh = new Mesh();

            //var points =  new List<Point3d>();
            var points3D = convexhull.Points.ToPoint3D();
            mesh.Vertices.AddVertices(points3D);
            //var meshFaces = new List<MeshFace>();
            var normals = new List<Vector3d>();

            foreach (DefaultConvexFace<DefaultVertex> convexface in convexhull.Faces)
            {
                //normals.Add(new Vector3d(convexface.Normal[0], convexface.Normal[1], convexface.Normal[2]));
                var facePoints3D = convexface.Vertices.ToPoint3D();

                var index0 = points3D.FirstIndexWhere(point => point.Equals(facePoints3D[0])).Value;
                var index1 = points3D.FirstIndexWhere(point => point.Equals(facePoints3D[1])).Value;
                var index2 = points3D.FirstIndexWhere(point => point.Equals(facePoints3D[2])).Value;

                mesh.Faces.AddFace(index0, index1, index2);
                mesh.FaceNormals.AddFaceNormal(convexface.Normal[0], convexface.Normal[1], convexface.Normal[2]);
                //var index0 = points3D
                //    .Select((v, index) => index)
                //    .Where(index => points3D[index].Equals(facePoints3D[0]));
                //var index1 = points3D
                //    .Select((v, index) => index)
                //    .Where(index => points3D[index].Equals(facePoints3D[1]));
                //var index2 = points3D
                //    .Select((v, index) => index)
                //    .Where(index => points3D[index].Equals(facePoints3D[2]));

                //meshFaces.Add(new MeshFace(index0.First(), index1.First(), index2.First(), index2.First()));

            }
            return mesh;
        }

        /// <summary>
        /// Convert List<Point2d> to  List<DefaultVertex2D>
        /// </summary>
        public static List<DefaultVertex2D> ToVertex2D(this IEnumerable<Point2d> points2D)
        {
            var vertices = new List<DefaultVertex2D>();
            foreach (Point2d point in points2D)
            {
                vertices.Add(new DefaultVertex2D(point.X, point.Y));
            }
            return vertices;
        }

        /// <summary>
        /// Convert List<Point3d> to List<DefaultVertex>
        /// </summary>
        public static List<DefaultVertex> ToVertex3D(this IEnumerable<Point3d> points3D)
        {
            var vertices = new List<DefaultVertex>();
            foreach (Point3d point in points3D)
            {
                vertices.Add(new DefaultVertex() { Position = new double[] { point.X, point.Y, point.Z } });
            }
            return vertices;
        }


        /// <summary>
        /// Convert IEnumerable<Point3d> to Accord.Math.Point3[] 
        /// </summary>
        public static Point3[] ToPoint3(this IEnumerable<Point3d> points3D)
        {
            var point3s = new Point3[points3D.Count()];
            for (int i = 0; i < points3D.Count(); i++) // this is rather inneficient because you do calculations of the plane twice
            {
                point3s[i] = new Point3((float)points3D.ElementAt(i).X, (float)points3D.ElementAt(i).Y, (float)points3D.ElementAt(i).Z); // this switch to float might cause accuracy problems
            }
            return point3s;
        }

        /// <summary>
        /// Convert Accord.Math.Plane to Rhino.Geometry.Plane
        /// </summary>
        public static Rhino.Geometry.Plane ToRhinoPlane(this Accord.Math.Plane accordPlane)
        {
            return new Rhino.Geometry.Plane(accordPlane.A, accordPlane.B, accordPlane.C, accordPlane.Offset); // the offest might not be normalized
        }

        /// <summary>
        /// Convert List<DefaultVertex2D> to List<Point2d> 
        /// </summary>
        public static List<Point2d> ToPoint2D(this IEnumerable<DefaultVertex2D> vertices2D)
        {
            var points2D = new List<Point2d>();
            foreach (DefaultVertex2D vertex in vertices2D)
            {
                points2D.Add(new Point2d(vertex.X, vertex.Y));
            }
            return points2D;
        }

        /// <summary>
        /// Convert List<DefaultVertex> to List<Point3d> 
        /// </summary>
        public static List<Point3d> ToPoint3D(this IEnumerable<DefaultVertex> vertices3D)
        {
            var points3D = new List<Point3d>();
            foreach (DefaultVertex vertex in vertices3D)
            {
                points3D.Add(new Point3d(vertex.Position[0], vertex.Position[1], vertex.Position[2]));
            }
            return points3D;
        }

        /// <summary>
        /// Convert List<DefaultVertex> to List<Point3d> 
        /// </summary>
        public static List<Point3d> GetPoints3D(this PlaneSurface surface, IEnumerable<Point2d> points2D)
        {
            var points3D = new List<Point3d>();
            foreach (Point2d point in points2D)
            {
                points3D.Add(surface.PointAt(point.X, point.Y));
            }
            return points3D;
        }

        /// <summary>
        /// Compute the best fit (least squares) planar surface through a mesh
        /// </summary>
        public static void FitPlanarSurface(this Mesh mesh, out Brep brep, out List<Point3d> contour3D, out Double rmse, Double tolerance = 0.05, Double resolution = 0.05)
        {
            //List<Point3d> points3D = mesh.GetPoints(); // replace this with a point cloud module
            GH_PointCloud gH_pointCloud = mesh.GenerateSpatialCloud(resolution);

            List<Point3d> points3D = new List<Point3d>();    // replace this with only the indices
            List<int> inliersint = new List<int>();    // replace this with only the indices

            List<Double> distances = new List<Double>();
            List<Point2d> points2D = new List<Point2d>();
            Double distance = 0.0;

            Rhino.Geometry.Plane.FitPlaneToPoints(gH_pointCloud.Value.AsReadOnlyListOfPoints(), out Rhino.Geometry.Plane plane, out double maximumDeviation);

            for (int i = 0; i < gH_pointCloud.Value.Count; i++) // this is rather inneficient because you do calculations of the plane twice
            {
                distance = Math.Abs(plane.DistanceTo(gH_pointCloud.Value[i].Location));
                if (distance <= tolerance)
                {
                    distances.Add(distance);
                    plane.ClosestParameter(gH_pointCloud.Value[i].Location, out Double s, out Double t); // does this really do what you think?
                    inliersint.Add(i);
                    points3D.Add(gH_pointCloud.Value[i].Location);
                    points2D.Add(new Point2d(s, t));
                }
            }
            rmse = distances.StandardDeviation();

            Interval xExtents = new Interval(gH_pointCloud.Boundingbox.Min.X, gH_pointCloud.Boundingbox.Max.X);
            Interval yExtents = new Interval(gH_pointCloud.Boundingbox.Min.Y, gH_pointCloud.Boundingbox.Max.Y);
            PlaneSurface planeSurface = new PlaneSurface(plane, xExtents, yExtents);

            var vertices2D = points2D.ToVertex2D();
            var convexHullCreationResult2D = ConvexHull.Create2D(vertices2D, tolerance);
            if (convexHullCreationResult2D.Outcome != 0) throw new Exception("2D Convex hull failed");
            var contour2D = convexHullCreationResult2D.Result.ToPoint2D();

            contour3D = planeSurface.GetPoints3D(contour2D);

            NurbsCurve nurbsCurve2D = NurbsSurface.CreateCurveOnSurface(planeSurface, contour2D, tolerance, true); // cant we make a polygon of this?
            if (!nurbsCurve2D.IsValid)
            {
                brep = planeSurface.ToBrep();
            }

            brep = Brep.CreateTrimmedPlane(plane, nurbsCurve2D);
        }

        /// <summary>
        /// Compute the best fit (least squares) planar surface through a mesh
        /// </summary>
        public static void FitPlanarSurface(this GH_PointCloud pc, out Boolean success, out Brep brep, out int[] inliers, out double rmse, double tolerance = 0.05)
        {

            //List<Point3d> points3D = new List<Point3d>();    // replace this with only the indices
            //inliers = new List<int>();    // replace this with only the indices

            List<Double> distances = new List<Double>();
            List<Point2d> points2D = new List<Point2d>();
            Double distance = 0.0;

            RansacPlane ransacPlane = new RansacPlane(tolerance, 0.1);
            Accord.Math.Plane accordPlane = ransacPlane.Estimate(pc.Value.AsReadOnlyListOfPoints().ToPoint3());
            Rhino.Geometry.Plane rhinoPlane = accordPlane.ToRhinoPlane();
            inliers = ransacPlane.Inliers;
            var pc_out = pc.GetSubsection(inliers);


            for (int i = 0; i < inliers.Length; i++) // this is rather inneficient because you do calculations of the plane twice
            {
                distance = Math.Abs(rhinoPlane.DistanceTo(pc_out.Value[i].Location));
                if (distance <= tolerance)
                {
                    distances.Add(distance);
                    rhinoPlane.ClosestParameter(pc_out.Value[i].Location, out Double s, out Double t); // does this really do what you think?
                    points2D.Add(new Point2d(s, t));
                }
            }
            rmse = distances.StandardDeviation();

            Interval xExtents = new Interval(pc_out.Boundingbox.Min.X, pc_out.Boundingbox.Max.X);
            Interval yExtents = new Interval(pc_out.Boundingbox.Min.Y, pc_out.Boundingbox.Max.Y);
            PlaneSurface planeSurface = new PlaneSurface(rhinoPlane, xExtents, yExtents);

            var vertices2D = points2D.ToVertex2D();
            var convexHullCreationResult2D = ConvexHull.Create2D(vertices2D, tolerance);
            if (convexHullCreationResult2D.Outcome != 0) throw new Exception("2D Convex hull failed");
            var contour2D = convexHullCreationResult2D.Result.ToPoint2D();

            //contour3D = planeSurface.GetPoints3D(contour2D);
            NurbsCurve nurbsCurve2D = NurbsSurface.CreateCurveOnSurface(planeSurface, contour2D, tolerance, true); // you shouldn't use all the points 
            if (!nurbsCurve2D.IsValid)
            {
                brep = planeSurface.ToBrep(); // this is just cautionary code
            }

            brep = Brep.CreateTrimmedPlane(rhinoPlane, nurbsCurve2D);
            if (!brep.IsValid) success = false;
            success = true;
        }
        /// <summary>
        /// Compute a nr of best fit (least squares) planar surfaces through a point cloud
        /// </summary>
        public static void FitPlanarSurfaces(this GH_PointCloud pc, out List<Brep> breps, out List<double> rmseList, int nrPlanes = 1, Double tolerance = 0.05)
        {
            if (!pc.Value.ContainsNormals) pc.ComputeNormals();
            breps = new List<Brep>();
            rmseList = new List<double>();
            int i = 0;
            Boolean success = true;

            while (nrPlanes < i && success)
            {
                i++;
                pc.FitPlanarSurface(out success, out Brep brep, out int[] inliers, out double rmse, tolerance);
                if (success)
                {
                    pc.Value.RemoveRange(inliers);
                    breps.Add(brep);
                    rmseList.Add(rmse);
                }
            }
        }
    }


}


