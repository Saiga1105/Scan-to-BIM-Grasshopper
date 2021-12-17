using System;
using System.IO;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


using Rhino.Geometry; // https://developer.rhino3d.com/api/
using Rhino.DocObjects.Custom;
using Rhino.Display;
using Rhino.FileIO;

using Grasshopper; // https://developer.rhino3d.com/api/
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using MathWorks.MATLAB.NET.Arrays;

using Scan2BIM_Matlab; // https://github.com/Saiga1105/Scan-to-BIM-Matlab

using Loyc.Utilities; //https://github.com/qwertie/LoycCore
using Loyc.Math;
using Loyc.Collections;

using MIConvexHull; // https://github.com/DesignEngrLab/MIConvexHull

using Accord.MachineLearning.Geometry; // http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
using Accord.Math;
using Accord.Statistics.Analysis;

using Aardvark.Data.E57; //https://github.com/aardvark-platform/
using Aardvark.Data.Points;
using Aardvark.Data.Points.Import;
using Aardvark.Geometry.Points;
using Uncodium.SimpleStore;

using E57LibCommon; //http://www.libe57.org/#:~:text=The%20E57%20file%20format%20is,in%20the%20ASTM%20E2807%20standard.
using E57LibReader;
using E57LibWriter;

//using g3;// https://github.com/gradientspace/geometry3Sharp
//using gs;

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
        /// Compute principal components of a 3D set (point cloud)
        /// </summary>
        public static List<Vector3d> ToVector3d(this double[][] mydoubles)
        {
            List<Vector3d> vectors = new List<Vector3d>();
            var length = mydoubles.GetLength(0);
            for (int i= 0;i<mydoubles.GetLength(0);i++)
            {
                vectors.Add(new Vector3d(mydoubles[i][0], mydoubles[i][1], mydoubles[i][2])); // this is under the assumption that each row holds a eigenvector
            }
            return vectors;
        }

        /// <summary>
        /// Convert IEnumerable<Accord.Math.Vector3>  to rhinocommon List<Vector3d>
        /// </summary>
        public static List<Vector3d> ToVector3d(this IEnumerable<Accord.Math.Vector3> vector3s)
        {
            List<Vector3d> vectors = new List<Vector3d>();
            for (int i = 0; i < vector3s.Count(); i++)
            {
                vectors.Add(new Vector3d(vector3s.ElementAt(i).X, vector3s.ElementAt(i).Y, vector3s.ElementAt(i).Z)); 
            }
            return vectors;
        }

        /// <summary>
        /// Convert IEnumerable<Aardvark.Base.V3f>  to rhinocommon List<Vector3d>
        /// </summary>
        public static List<Vector3d> ToVector3d(this IEnumerable<Aardvark.Base.V3f> vector3s)
        {
            List<Vector3d> vectors = new List<Vector3d>();

            foreach(Aardvark.Base.V3f v3F in vector3s)
            {
                vectors.Add(new Vector3d(v3F.X, v3F.Y, v3F.Z));
            }
            return vectors;
        }

        /// <summary>
        /// Convert rhinocommon IEnumerable<Vector3d> to List<Accord.Math.Vector3> 
        /// </summary>
        public static List<Accord.Math.Vector3> ToVector3(this IEnumerable<Vector3d> vector3ds)
        {
            List<Accord.Math.Vector3> vectors = new List<Accord.Math.Vector3>();
            for (int i = 0; i < vector3ds.Count(); i++)
            {
                vectors.Add(new Accord.Math.Vector3((float)vector3ds.ElementAt(i).X, (float)vector3ds.ElementAt(i).Y, (float)vector3ds.ElementAt(i).Z));
            }
            return vectors;
        }

        

        /// <summary>
        /// Compute principal components of a 3D set (point cloud)
        /// </summary>
        public static void ComputePrincipalComponents(this GH_PointCloud pc, out List<Vector3d> eigenvectors, out List<Double> eigenvalues)
        {
            var method = PrincipalComponentMethod.Center;
            var pca = new PrincipalComponentAnalysis(method);
            var data = pc.Value.GetPoints().ToArray().ToDouble().ToArray(); // this is unlickely to work
            pca.Learn(data);

            eigenvectors = pca.ComponentVectors.ToVector3d(); ; // direction of the principal components
            eigenvalues = pca.Eigenvalues.ToList(); // magniture of the principal components
        }

        /// <summary>
        /// Create 
        /// </summary>
        public static List<Vector3d>  CreateGaussianSphere(double tolerance)
        {
            List<Vector3d> spherevectors = new List<Vector3d>();

            // !!! these vectors do not result in an even spead near the poles
            // compute vectors based on spherical coordinates
            //double r = 0.0; // distance to origin
            //for (double phi = 0.0; phi < Math.PI; phi = phi + tolerance) // phi is the zenith angle (0 is up)
            //{
            //    for (double theta = 0.0; theta < 2 * Math.PI; theta = theta + tolerance)  // theta is the angle around Z-axis 
            //    {
            //        var x = r * Math.Sin(phi) * Math.Cos(theta);                    
            //        var y = r * Math.Sin(phi) * Math.Sin(theta);
            //        var z = r * Math.Cos(phi); 

            //        spherevectors.Add(new Vector3d(x, y, z)); 
            //    }
            //}

            var sphere = new Sphere(new Point3d(0.0, 0.0, 0.0), 1.0);
            var nrfaces = (int) (4*Math.PI) / (Math.Sqrt(Math.Sin((tolerance)))); 
            var subdivisions = (int) Math.Sqrt(Math.Sqrt(nrfaces / 6));
            if (subdivisions < 0 || subdivisions > 8) subdivisions = 4;
            var sphereMesh=Mesh.CreateQuadSphere(sphere, subdivisions);
            spherevectors = sphereMesh.GetPoints().ToVector3d();
            return spherevectors;
        }

        /// <summary>
        /// Compute the best fit orientation of the Point Cloud based on PCA
        /// </summary>
        public static Transform ComputeOrientation (this GH_PointCloud pc)
        {
            pc.ComputePrincipalComponents(out List<Vector3d> eigenvectors, out List<double> eigenvalues);
            var transform = new Transform();
            transform.M00 = eigenvectors[0].X;
            transform.M01 = eigenvectors[0].Y;
            transform.M02 = eigenvectors[0].Z;
            transform.M03 = 0;
            transform.M10 = eigenvectors[1].X;
            transform.M11 = eigenvectors[1].Y;
            transform.M12 = eigenvectors[1].Z;
            transform.M13 = 0;
            transform.M20 = eigenvectors[2].X;
            transform.M21 = eigenvectors[2].Y;
            transform.M22 = eigenvectors[2].Z;
            transform.M23 = 0;
            transform.M30 = 0;
            transform.M31 = 0;
            transform.M32 = 0;
            transform.M33 = 1;

            return transform;
        }

        /// <summary>
        /// Compute the best fit orientation of the Point Cloud based on PCA
        /// </summary>
        public static int[] FindClosestVector(this IEnumerable<Vector3d> vectors, IEnumerable<Vector3d> referenceVectors, double tolerance = 0.05)
        {
            int[] indices = new int[vectors.Count()];

            for (int i=0;i<indices.Count();i++)
            {
                for (int j=0;j<referenceVectors.Count();j++)
                {
                    if (Vector3d.Multiply(vectors.ElementAt(i),referenceVectors.ElementAt(j)) > tolerance) // this code picks the first reference that is in rance, potentially missing the best fit vector
                    {
                        indices[i] = j; // potential weakness that none of the vectors is assigned
                        break;
                    }
                }
            }
            return indices;
        }

        /// <summary>
        /// Split a point cloud based on its normals [0;2Pi]
        /// </summary>
        public static void SegmentGaussianSphere(this GH_PointCloud pc, out List<GH_PointCloud> pc_outs, double distance=0.1,double tolerance = Math.PI / 36, bool computeZ = false)
        {
            pc_outs = null;
            List<GH_PointCloud> pc_out = new List<GH_PointCloud>();
            if (!pc.Value.ContainsNormals) { pc.ComputeNormals(); }
            var normals = pc.Value.GetNormals();
            // estimate up-direction
            if (computeZ)
            {
                var tform = pc.ComputeOrientation();
                pc.Transform(tform);
            }

            List<Vector3d> spherevectors = CreateGaussianSphere(tolerance); // isn't this code quite stupid?
            var indexList = normals.FindClosestVector(spherevectors);
            GH_PointCloud pc_temp = null;
            
            for (int i = 0; i < spherevectors.Count; i++)
            {
                var orientationIndices = pc.Value
                   .Select((v, index) => index)
                   .Where(index => index == 0)
                   .ToList();
                // perform a size check
                if (orientationIndices.Count < 1) continue;

                pc_temp= pc.GetSubsection(orientationIndices);
                // compute connected components
                //var componentIndices = pc_temp.ComputeConnectedComponents(distance); // are these the correct indices

                // retrieve subsection
                //pc_outs.Add(pc.GetSubsection(componentIndices));
                pc_outs.Add(pc_temp);
            }
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

            // this conversion for some reason failed
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
        /// Read an e.57 Point Cloud using Aardvark library
        /// </summary>
        public static List<GH_PointCloud> ReadPointCloud(this string filename, string temp_storage, double percent)
        {
            List<GH_PointCloud> pc = new List<GH_PointCloud>();
            var info = E57.E57Info(filename, ParseConfig.Default);
            if (info.PointCount < 1) throw new Exception("This point cloud doesn't seem to contain any points.");
            try
            {
                // this out of core storage should be more investigated
                //var store = new SimpleDiskStore(temp_storage).ToPointCloudStore(); // we get an unknown folder exception => there are no .bin files there
                //var cloud = Aardvark.Geometry.Points.PointCloud.Import(filename, ImportConfig.Default.WithStorage(store));
                //pc = cloud.ToGH_PointCloud();
                var chunks = E57.Chunks(filename, ParseConfig.Default);
                pc = chunks.ToGH_PointCloud();
            }
            catch (Exception)
            {

                throw new Exception("Couldn't load point cloud. Check file integrity or writing permissions of temporaty file storage."); ;
            }
            return pc;
        }

        /// <summary>
        /// Check valid filename
        /// </summary>
        public static bool IsValidFileName(this string expression, bool platformIndependent)
        {
            string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            if (platformIndependent)
            {
                sPattern = @"^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?))\\)*[^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?$";
            }
            return (Regex.IsMatch(expression, sPattern, RegexOptions.CultureInvariant));
        }

        /// <summary>
        /// Export a list of GH_PointCloud objects to an e57 file with optional scannames
        /// </summary>
        public static Boolean ExportPointCloud(this IEnumerable<GH_PointCloud> gH_PointClouds , string filename, IEnumerable<string> settings)
        {
            Boolean success = false;
            try
            {
                List<System.Guid> guids = new List<Guid>();

                // Deselect all guids
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(guids);

                // bake geometries to rhino
                foreach (GH_PointCloud gH_PointCloud in gH_PointClouds)
                {
                    var guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(gH_PointCloud.Value);
                    guids.Add(guid);
                }
                
                // Select guids
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(guids);

                // create string command to export geometries from rhino to .obj
                string writeOptions = string.Concat(settings);
                //string cmd = "_-Export " + "\"" + filename + "\"" + " _ENTER" + " _ENTER";
                string cmd = "_-Export " + "\"" + filename + "\"" + writeOptions + " _ENTER" + " _ENTER";
                // _-Export \"D:\\Data\\2018-06 Werfopvolging Academiestraat Gent\\week 22\\MONITORING DATA\\blabla.ply\"
                // "_-Export \"D:\\Data\\2018-06 Werfopvolging Academiestraat Gent\\week 22\\MONITORING DATA\\blabla.ply\" _ENTER _ENTER"
                //"_-Export \"D:\\Data\\2018-06 Werfopvolging Academiestraat Gent\\week 22\\MONITORING DATA\\blabla.obj\" _ENTER _ENTER"
                // Run script
                success = Rhino.RhinoApp.RunScript(cmd, false);

                // Delete geometries from Rhino
                Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guids, true);
            }
            catch (Exception)
            {
                throw new Exception("Exporter crashed"); ;
            }
            return success;
        }

        /// <summary>
        /// Check writing permissions of local store directory
        /// </summary>
        public static bool IsDirectoryWritable(this string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get a Barycentric coordinate grid on a mesh triangle [u,v,w]
        /// </summary>
        public static List<Vector3f> BarycentricCoordinateGrid(this Mesh mesh, int index, double resolution) 
        {
            var a = mesh.Vertices[mesh.Faces[index].A];
            var b = mesh.Vertices[mesh.Faces[index].B];
            var c = mesh.Vertices[mesh.Faces[index].C];

            var ab = a.DistanceTo(b);
            var bc = b.DistanceTo(c);
            var ca = c.DistanceTo(a);

            var stepab = (float) (resolution /ab);
            var stepbc = (float) (resolution /bc);
            var stepca = (float) (resolution /ca);

            var step = (float) (Math.Round((stepab + stepbc + stepca) / 3, 3)); // this average step can cause issues in very slender triangles

            float t0;
            float t1;
            float t2;

            List<Vector3f> vectors = new List<Vector3f>();
            for (t2 = 0; t2 <= 1; t2 = t2 + step)
            {
                for (t0 = 1-t2; t0 >= 0; t0 = t0 - step)
                {
                    t1 = 1 - t0-t2;
                    vectors.Add(new Vector3f(t0, t1, t2));                
                }
            }
            return vectors;
        }
        /// <summary>
        /// Get a random Barycentric coordinate on a mesh triangle vector3d(t0,t1,t2)
        /// </summary>
        public static Vector3d RandomBarycentricCoordinate() 
        {
            Random random = new Random();
            //var t0 = (double)random.NextDouble();
            var t0 = random.Next(0, 100);
            var t1 = random.Next(0, 100-t0);
            //var t1 = 1-t0-(double)random.NextDouble();
            var t2 = 1 - t0 - t1;

            return new Vector3d(t0/100.0, t1 / 100.0, t2 / 100.0);
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
            var indices = new List<int>();

            if (!brep.IsSolid) throw new Exception("Provide a closed Brep");
            if (!brep.IsManifold) throw new Exception("Provide a manifold Brep");

            for (int i = 0; i < pc.Value.Count; i++)
            {
                if (brep.IsPointInside(points[i], 0.0, false)) indices.Add(i); // this is a veeeerrry slow implementation
            }
            pc_out = pc.GetSubsection(indices); 

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
            double[] distances = new double[pc.Value.Count];

            for (int i = 0; i < pc.Value.Count; i++)
            {
                point = brep.ClosestPoint(points[i]);
                if (point.IsValid)
                {
                    distances[i] = point.DistanceTo(points[i]); // this is a veeeerrry slow implementation
                }
                else distances[i] = -1.0;

            }
            pc.Distances.AddRange(distances);

        }

        /// <summary>
        /// Distance between point cloud points and a Mesh (distance overwrites the PointValue)
        /// </summary>
        public static void DistanceTo(this GH_PointCloud pc, Mesh mesh)
        {
            if (!mesh.IsValid) throw new Exception("Provide a valid Mesh");
            var pcPoints = pc.Value.AsReadOnlyListOfPoints();
            Point3d meshPoint;
            double[] distances= new double[pc.Value.Count];
            
            for (int i = 0; i < pc.Value.Count; i++)
            {
                meshPoint = mesh.ClosestPoint(pcPoints[i]);
                if (meshPoint.IsValid)
                {
                    distances[i] = meshPoint.DistanceTo(pcPoints[i]);
                }
                else distances[i] = -1.0;
            }
            pc.Distances.AddRange(distances);
        }
        /// <summary>
        /// Distance between point cloud points and a GH_PointCloud (distance overwrites the PointValue)
        /// </summary>
        public static void DistanceTo(this GH_PointCloud pc, GH_PointCloud pointCloud)
        {
            if (!pointCloud.IsValid) throw new Exception("Provide a valid GH_PointCloud");
            var points = pc.Value.AsReadOnlyListOfPoints();
            Point3d point = Point3d.Unset;
            int index = 0;
            double[] distances = new double[pc.Value.Count];


            for (int i = 0; i < pc.Value.Count; i++)
            {
                index = pointCloud.Value.ClosestPoint(points[i]); // this is a veeeerrry slow implementation
                if (index != -1)
                {
                    distances[i] = pointCloud.Value[index].Location.DistanceTo(points[i]); // this is a veeeerrry slow implementation
                }
                else distances[i] = -1.0;

            }
            pc.Distances.AddRange(distances);

        }
        /// <summary>
        /// Generate samples on a point cloud according to a specific resolution (e.g. a point every 0.05m)
        /// </summary>
        public static GH_PointCloud GenerateRandomCloud(this GH_PointCloud pc, double resolution = 0.05)
        {
            GH_PointCloud pc_out = new GH_PointCloud();

            var pcResolution = pc.ComputeResolution(0.01);
            double ratio = Math.Abs(resolution / pcResolution);
            if (ratio == 0 || ratio > 1) ratio = 0.5;
            var nrSamples = (int)(ratio * pc.Value.Count);
    
            if (nrSamples < 1) throw new Exception("not enough samples.try increasing the ratio");
            else
            {
                var indices = pc.GenerateIndices(nrSamples);
                var temp=pc.GetSubsection(indices);
                if (temp != null) pc_out = new GH_PointCloud(temp);
                else throw new Exception("Random sampling failed. enough points?");
            }
            return pc_out;
        }
        /// <summary>
        /// Generate an array of indices in range of point cloud 
        /// </summary>
        public static int[] GenerateIndices(this GH_PointCloud pc, int nrSamples)
        {
            Random random = new Random();
            int[] array = new int[nrSamples];
            for (int i = 0; i < nrSamples; i++)
            {
                array[i] = random.Next(0, pc.Value.Count);
            }
            return array;
        }
        /// <summary>
        /// Generate a nr of random samples on a mesh. The resolution is the nr of points/m²
        /// </summary>
        public static GH_PointCloud GenerateRandomCloud(this Mesh mesh, double resolution = 0.05)
        {
            GH_PointCloud pc = new GH_PointCloud();
            //int nrSamples = (int)(AreaMassProperties.Compute(mesh).Area * resolution);

            int samplesPerTriangle = 0;
            Vector3d vector = new Vector3d();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                samplesPerTriangle = (int)(mesh.AreaOfTriangle(i) / (resolution*resolution)); // does this give correct result?
                if (samplesPerTriangle < 2) pc.Value.Add(mesh.Faces.GetFaceCenter(i));

                else
                {
                    for (int j = 0; j < samplesPerTriangle; j++) 
                    {
                        vector = RandomBarycentricCoordinate();
                        pc.Value.Add(mesh.PointAt(i, vector[0], vector[1], vector[2], 0.0)); 
                    }
                }                
            }
            return pc;
        }

        /// <summary>
        /// Generate a nr of random samples on a mesh. The resolution is the nr of points/m²
        /// </summary>
        public static GH_PointCloud GenerateSpatialCloud(this Mesh mesh, double resolution = 0.05)
        {
            GH_PointCloud pc = new GH_PointCloud();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {               
                if (mesh.AreaOfTriangle(i) < resolution) pc.Value.Add(mesh.Faces.GetFaceCenter(i));
                else
                {
                    var vectors = mesh.BarycentricCoordinateGrid(i,resolution);
                    foreach (Vector3d vector in vectors) 
                    {                        
                        pc.Value.Add(mesh.PointAt(i, vector[0], vector[1], vector[2], 0.0));
                    }
                }

            }
            return pc;
        }

        /// <summary>
        /// Export a set of meshes to an .obj file. Settings are optional
        /// </summary>
        public static Boolean ExportMesh(this IEnumerable<Mesh> meshes, string filename, List<string> settings)
        {
            Boolean success = false;
            try
            {
                List<System.Guid> guids = new List<Guid>();
                // Deselect all guids
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(guids);

                // bake geometries to rhino
                foreach (Mesh mesh in meshes)
                {
                    var guid = Rhino.RhinoDoc.ActiveDoc.Objects.Add(mesh);
                    guids.Add(guid);

                }

                // Select guids
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(guids);

                // create string command to export geometries from rhino to .obj
                string writeOptions = string.Concat(settings);
                string cmd = "_-Export " + "\"" + filename + "\"" + writeOptions + " _ENTER" + " _ENTER";

                // Run script
                success=Rhino.RhinoApp.RunScript(cmd, false);

                // Delete geometries from Rhino
                Rhino.RhinoDoc.ActiveDoc.Objects.Delete(guids, true);                
            }
            catch (Exception)
            {
                throw new Exception("Exporter crashed"); ;
            }
            return success;
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
                else
                {
                    for (int j = 0; j < samplesPerFace; j++)
                    {
                        vector = RandomUVCoordinate();
                        pc.Value.Add(brep.Faces[i].PointAt(vector.X, vector.Y)); // this might nog be the right answer
                    }
                }
            }
            return pc;
        }

            /// <summary>
            /// IEnumerable<PointCloudItem> pointCloudItems to PointCloud
            /// </summary>
            public static Rhino.Geometry.PointCloud ToPointCloud(this IEnumerable<PointCloudItem> pointCloudItems)
        {
            var pc = new Rhino.Geometry.PointCloud();
            Point3d[] points = new Point3d[pointCloudItems.Count()];
            Color[] colors = new Color[pointCloudItems.Count()];
            Vector3d[] normals = new Vector3d[pointCloudItems.Count()]; // this is so memory intensive!
            Double[] pointvalues = new double[pointCloudItems.Count()];

            points = pointCloudItems.Select(point => point.Location).ToArray();
            if(pc.ContainsNormals) normals= pointCloudItems.Select(point => point.Normal).ToArray();
            if(pc.ContainsColors) colors = pointCloudItems.Select(point => point.Color).ToArray();
            if(pc.ContainsPointValues) pointvalues = pointCloudItems.Select(point => point.PointValue).ToArray();


            for (int i = 0; i < pointCloudItems.Count(); i++)
            {
                pc.Add(points[i], normals[i], colors[i], pointvalues[i]);
            }

            //foreach (PointCloudItem pointCloudItem in pointCloudItems)
            //{
                
            //    pc.Append(pointCloudItem); // addrange is muuuuuch faster!
            //}
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
                pointCloudItems.Add(pc.Value[indices.ElementAt(i)]);                
            }
            return pointCloudItems;
        }

        /// <summary>
        /// Get a subcloud of GH_PointCloud
        /// </summary>
        public static GH_PointCloud GetSubsection(this GH_PointCloud pc, IEnumerable<int> indices) // this is slooooooooooooooooooooooow 9s for 10k points
        {
            GH_PointCloud pc_out = new GH_PointCloud();
            List<PointCloudItem> pointCloudItems = pc.GetPointCloudItems(indices);
            pc_out.Value = pointCloudItems.ToPointCloud();
            
            if (pc.ContainsDistances)
            {
                var distances = pc.Distances.Get(indices.ToArray()); // check this please
                pc_out.Distances.AddRange(distances);
            }
            if (pc.ContainsClassification)
            {
                var classifications = pc.Classification.Get(indices.ToArray()); // check this please
                pc_out.Classification.AddRange(classifications);
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
        /// Convert Aardvark.Geometry.Points.PointSet to List<GH_PointCloud>
        /// </summary>
        public static List<GH_PointCloud> ToGH_PointCloud(this Aardvark.Geometry.Points.PointSet pointCloud_Aardvark)
        {
            var gh_pointCloud = new GH_PointCloud();
            List<GH_PointCloud> GH_pointClouds = new List<GH_PointCloud>();
            var chunks = pointCloud_Aardvark.QueryAllPoints();
            foreach (Chunk chunk in chunks)
            {
                var pointCloud = new Rhino.Geometry.PointCloud();
                var points = chunk.Positions.ToPoint3d();
                List<Vector3d> normals=new List<Vector3d>();
                List<Color> colors = new List<Color>(); // is this the correct grasshooper color?

                if (chunk.HasNormals)
                {
                    normals = chunk.Normals.ToVector3d();
                }
                if (chunk.HasColors)
                {
                    colors = chunk.Colors.ToColor();
                }
                if (chunk.HasNormals && chunk.HasColors)
                {
                    pointCloud.AddRange(points, normals, colors);
                }
                else if (chunk.HasNormals && !chunk.HasColors)
                {
                    pointCloud.AddRange(points, normals);
                }
                else if (!chunk.HasNormals && chunk.HasColors)
                {
                    pointCloud.AddRange(points, colors);
                }
                GH_pointClouds.Add(new GH_PointCloud(pointCloud));
            }
            return GH_pointClouds;
        }

        /// <summary>
        /// Convert Aardvark.Geometry.Points.Chunks to List<GH_PointCloud>
        /// </summary>
        public static List<GH_PointCloud> ToGH_PointCloud(this IEnumerable<Aardvark.Data.Points.Chunk> chunks)
        {
            var gh_pointCloud = new GH_PointCloud();
            List<GH_PointCloud> GH_pointClouds = new List<GH_PointCloud>();
            foreach (Chunk chunk in chunks)
            {
                var pointCloud = new Rhino.Geometry.PointCloud();
                var points = chunk.Positions.ToPoint3d();
                List<Vector3d> normals = new List<Vector3d>();
                List<Color> colors = new List<Color>(); // is this the correct grasshooper color?

                if (chunk.HasNormals)
                {
                    normals = chunk.Normals.ToVector3d();
                }
                if (chunk.HasColors)
                {
                    colors = chunk.Colors.ToColor();
                }
                if (chunk.HasNormals && chunk.HasColors)
                {
                    pointCloud.AddRange(points, normals, colors);
                }
                else if (chunk.HasNormals && !chunk.HasColors)
                {
                    pointCloud.AddRange(points, normals);
                }
                else if (!chunk.HasNormals && chunk.HasColors)
                {
                    pointCloud.AddRange(points, colors);
                }
                GH_pointClouds.Add(new GH_PointCloud(pointCloud));
            }
            return GH_pointClouds;
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
        /// Convert IEnumerable<Point3d> to List<Vector3d>
        /// </summary>
        public static List<Vector3d> ToVector3d(this IEnumerable<Point3d> points3D)
        {
            var vectors = new List<Vector3d>(points3D.Count());
            for (int i = 0; i < points3D.Count(); i++) // this is rather inneficient because you do calculations of the plane twice
            {
                vectors[i] = new Vector3d(points3D.ElementAt(i).X, points3D.ElementAt(i).Y, points3D.ElementAt(i).Z); 
            }
            return vectors;
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
        /// Convert IEnumerable<Aardvark.Base.V3d> to List<Point3d> https://github.com/aardvark-platform
        /// </summary>
        public static List<Point3d> ToPoint3d(this IEnumerable<Aardvark.Base.V3d> verticesV3D)
        {
            var points3D = new List<Point3d>();
            foreach (Aardvark.Base.V3d vertex in verticesV3D)
            {
                points3D.Add(new Point3d(vertex.X, vertex.Y, vertex.Z));
            }
            return points3D;
        }

        /// <summary>
        /// Convert IEnumerable<Aardvark.Base.C4b> to List<System.Drawing.Color> https://github.com/aardvark-platform
        /// </summary>
        public static List<System.Drawing.Color> ToColor(this IEnumerable<Aardvark.Base.C4b> colorsC4B)
        {
            var colors = new List<System.Drawing.Color>();

            foreach (Aardvark.Base.C4b c4B in colorsC4B)
            {
                colors.Add(System.Drawing.Color.FromArgb(c4B.A, c4B.R, c4B.G, c4B.B));
            }
            return colors;
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
        public static void FitPlanarSurface(this GH_PointCloud pc, out Boolean success, out Brep brep, out int[] inliers, out double rmse, double tolerance = 0.05) // this yields invalid breps
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

            while (i < nrPlanes && success) 
            {
                i++;
                pc.FitPlanarSurface(out success, out Brep brep, out int[] inliers, out double rmse, tolerance); // this yields an invalid brep
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


