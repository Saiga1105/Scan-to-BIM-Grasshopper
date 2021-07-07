using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Rhino;
using Rhino.DocObjects;
using Grasshopper.Kernel.Data;
using MathWorks.MATLAB.NET.Arrays;
using Scan2BIM_Matlab;


namespace Scan2BIM
{
    public class GH_PointCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        /// <summary>
        /// Fields of the GH_PointCloud class
        /// </summary>
        #region fields 
        private List<int> m_classifications; //test
        private List<double> m_distances; //test

        // there currently is only the PointCloud field
        /// <summary>
        /// Gets or sets the distance results of this GH_PointCloud.
        /// </summary>
        public List<double> Distances
        {
            get
            {       
                return m_distances;
            }
            set
            {
                if (value.Count() != m_distances.Count()) return; // test: i'm uncertain if the set works like this
                m_distances = value;
            }
        }
        /// <summary>
        /// Gets or sets the classification results of this GH_PointCloud.
        /// </summary>
        public List<int> Classification
        {
            get
            {
                return m_classifications;
            }
            set
            {
                if (value.Count() != m_classifications.Count()) return; // test: i'm uncertain if the set works like this
                m_classifications = value;
            }
        }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GH_PointCloud()
        {
            this.m_value = new PointCloud();
            this.m_classifications = new List<int>(); //test
            this.m_distances = new List<double>(); //test

        }

        /// <summary>
        /// Create GH_PointCloud from a Rhino.Geometry PointCloud
        /// </summary>
        public GH_PointCloud(PointCloud cloud)
        {
            this.m_value = cloud;
            this.m_classifications = new List<int>(cloud.Count); //test
            this.m_distances = new List<double>(cloud.Count); //test
        }

        /// <summary>
        /// Create GH_PointCloud from another GH_PointCloud
        /// </summary>  
        public GH_PointCloud(GH_PointCloud other)
        {
            m_value = (PointCloud) other.m_value.Duplicate();
            m_classifications = other.m_classifications; //test
            m_distances = other.m_distances; //test
        }
        #endregion

        #region "Properties"

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                return this.m_value.GetBoundingBox(true);
            }
        }
        /// <summary>
        /// Return whether GH_PointCloud is a valid entity
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Return whether GH_PointCloud has classification results
        /// </summary>
        public bool ContainsClassification
        {
            get
            {
                if (!this.m_classifications.Any()) { return false; } // this is a test
                return true;
            }
 
        }

        /// <summary>
        /// Return whether GH_PointCloud contains distances
        /// </summary>
        public bool ContainsDistances
        {
            get
            {
                if (!this.m_distances.Any()) { return false; } //  test
                return true;
            }
        }
        /// <summary>
        /// Clears any classification results on GH_PointCloud
        /// </summary>
        public void ClearClassification()
        {
            if (!ContainsClassification) return;
            this.m_classifications = new List<int>();// new int[Value.Count]; //test
        }

        /// <summary>
        /// Clears any distance results on GH_PointCloud
        /// </summary>
        public void ClearDistances()
        {
            if (!ContainsDistances) return;
            this.m_distances = new List<double>(); //test 
        }
               
        /// <summary>
        /// Add multiple distances to GH_PointCloud according to a set of indices
        /// </summary>
        public void SetDistances(IEnumerable<double> distances, IEnumerable<int> indices)
        {
            if (distances.Count() != indices.Count()) throw new Exception("indices and distances should be of equal length"); //Test
            for (int i = 0; i < indices.Count(); i++)
            {
                this.m_distances[indices.ElementAt(i)] = distances.ElementAt(i);
            }
        }

        /// <summary>
        /// Add multiple classifications to GH_PointCloud according to a set of indices
        /// </summary>
        public void SetClassification(IEnumerable<int> classifications, IEnumerable<int> indices)
        {
            if (classifications.Count() != indices.Count()) throw new Exception("indices and classifications should be of equal length"); //Test
            for (int i = 0; i < indices.Count(); i++)
            {
                this.m_distances[indices.ElementAt(i)] = classifications.ElementAt(i);
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "A point cloud with optional colors and normals.";
            }
        }

        public override string TypeName
        {
            get
            {
                return "PointCloud";
            }
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return this.m_value.GetBoundingBox(true);
            }
        }

        public bool IsBakeCapable
        {
            get
            {
                return this.m_value.Count > 0;
            }
        }

        public static explicit operator GH_PointCloud(GeometryBase v)
        {
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_PointCloud((PointCloud) this.m_value.Duplicate());
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            return this.m_value.GetBoundingBox(xform);
        }

        
        #endregion

        #region "Casting"
        // Return the value we use to represent the type in unsafe code i.e. C# scripts.
        public override object ScriptVariable()
        {
            return Value;
        }

        public override string ToString()
        {
            if (this.IsValid)
            {
                return String.Format("PointCloud: {0}", m_value.Count);
            }
            else
                return ("Invalid Point cloud");
        }


        public override bool CastFrom(object source)
        {
            if (source.GetType() == typeof(PointCloud))
            {
                this.m_value = (PointCloud)source;
                return true;
            }

            if (source.GetType() == typeof(GH_PointCloud))
            {
                this.m_value = ((GH_PointCloud)source).Value;
                return true;
            }

            if (source.GetType() == typeof(GH_Mesh))
            {
                var mesh = (GH_Mesh)source;
                this.Value = mesh.Value.GenerateSpatialCloud().Value;
                return true;
            }
            if (source.GetType() == typeof(Mesh))
            {
                var mesh = (Mesh)source;
                this.Value = mesh.GenerateSpatialCloud().Value;
                return true;
            }
            if (source.GetType() == typeof(GH_Brep))
            {
                var brep = (GH_Brep)source;
                this.Value = brep.Value.GenerateSpatialCloud().Value;
                return true;
            }
            if (source.GetType() == typeof(Brep))
            {
                var brep = new GH_Brep((Brep)source);
                this.Value = brep.Value.GenerateSpatialCloud().Value;
                return true;
            }
            if (source.GetType() == typeof(IEnumerable<Point3d>))
            {
                this.m_value = new PointCloud((IEnumerable<Point3d>)source);
                return true;
            }
            try
            {
                var asOtherPC = (GH_GeometricGoo<PointCloud>)source; // cast als het iets anders is dat wel can casten naar een GH_GeometricGoo<PointCloud>
                if (asOtherPC != null)
                {
                    this.m_value = asOtherPC.Value;
                    return true;
                }
            }
            catch
            {
                throw new Exception("cast didn't succeed from another Point Cloud to GH_PointCloud");
            }
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(out Q target)
        {
            //target = default; // this is new
            if (typeof(Q) == typeof(PointCloud)) // this should be made obsolute by IsAssignableFrom(typeof(PointCloud))
            {
                target = (Q)(object)this.m_value;
                return true;
            }

            if (typeof(Q) == typeof(GH_PointCloud)) // this should be made obsolute by IsAssignableFrom(typeof(PointCloud))
            {
                target = (Q)(object)this;
                return true;
            }

            if (typeof(Q) == typeof(IEnumerable<Point3d>))
            {
                target = (Q)(object)this.m_value.GetPoints();
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(PointCloud))) // this is the new code
            {
                target = (Q)(object)this.Value;
                return true;
            }
            target = default;
            return false;
            
            /// default code
            //return base.CastTo<Q>(out target);
            //target = default;
            //return false;
        }
        
        #endregion

        #region Transformations
        /// <summary>
        /// Transform the point cloud
        /// </summary>
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            var dup = this.m_value.Duplicate();
            dup.Transform(xform);
            return new GH_PointCloud((PointCloud)dup);
        }
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            var dup = this.m_value.Duplicate();
            xmorph.Morph(dup);
            return new GH_PointCloud((PointCloud)dup);
        }
        #endregion

        #region Drawing
        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, new ObjectAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            obj_ids.Add(doc.Objects.AddPointCloud(m_value, att));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            // No meshes to draw
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPointCloud(this.m_value, args.Thickness);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compute normals of the point cloud
        /// </summary>
        public void ComputeNormals(int k=6)
        {
            // check type
            if (!this.m_value.IsValid) { return; }
            // check dimensions
            if (this.m_value.Count <= k) { return; }

            /// interal parameters 
            MWNumericArray mWNumericArray = new MWNumericArray();

            if (this.m_value.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArray))
            {
                Scan2BIM_Matlab.General general = new Scan2BIM_Matlab.General();
                MWNumericArray matlabNormals = (MWNumericArray)general.S2B_ComputeNormals(mWNumericArray, k);

                Vector3d[] rhinoNormals = new Vector3d[matlabNormals.Dimensions[0]];
                if (matlabNormals.ToVector3d(ref rhinoNormals))
                {
                    for (int i = 0; i < rhinoNormals.Length; i++)
                    {
                        this.m_value[i].Normal = rhinoNormals[i];
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Compute the resolution of the point cloud based on a percentage of the points
        /// </summary>
        public double ComputeResolution(double percentage)
        {
            // check type
            if (!this.m_value.IsValid) { return 0.0; }

            // convert % to actual samplenr
            var nrSamples = (int) (percentage * this.m_value.Count);

            /// interal parameters 
            MWNumericArray mWNumericArray = new MWNumericArray();

            if (this.m_value.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArray))
            {
                Scan2BIM_Matlab.General general = new Scan2BIM_Matlab.General();
                MWNumericArray resolution = (MWNumericArray)general.S2B_Resolution_PCD(mWNumericArray, nrSamples);
                return resolution.ToScalarDouble();
            }
            return 0.0;
        }

        /// <summary>
        /// Allign this point cloud to a target point cloud. return the tform [16,1] and the rmse value
        /// </summary>
        public void AllignToCloud(out Transform transform, out Double rmse, GH_PointCloud target, Double metric =0, Double inlierRatio=0.6, Double maxIterations=20, Double downsamplingA=0.1, Double downsamplingB=0.5)
        {
            // Exceptions
            if (this.Value.Count <= 1 || target.Value.Count <= 1) throw new Exception("Use point clouds with more than 1 point");
            if (downsamplingA == 0 || downsamplingB == 0) throw new Exception("Do not use 0 as downsampling");

            uint nrSamplesA = (uint)(downsamplingA * this.Value.Count);
            uint nrSamplesB = (uint)(downsamplingB * target.Value.Count);

            var sampleA = this.Value.GetRandomSubsample(nrSamplesA);
            var sampleB = target.Value.GetRandomSubsample(nrSamplesB);

            MWNumericArray mWNumericArrayA = new MWNumericArray();
            MWNumericArray mWNumericArrayB = new MWNumericArray();

            if (!this.Value.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArrayA)) throw new Exception("ToMWNumericArray conversion error. Matlab runtime ok?");
            if (!target.Value.AsReadOnlyListOfPoints().ToArray().ToMWNumericArray(ref mWNumericArrayB)) throw new Exception("ToMWNumericArray conversion error. Matlab runtime ok?");

            Scan2BIM_Matlab.General general = new General();
            var mwca = (MWCellArray)general.S2B_ICP2(mWNumericArrayA, mWNumericArrayB, metric, inlierRatio, maxIterations);

            MWNumericArray na0 = (MWNumericArray)mwca[1];
            double[] dc0 = (double[])na0.ToVector(0);
            var transformList = new List<double>(dc0);

            transform = transformList.ToTransform();

            MWNumericArray na1 = (MWNumericArray)mwca[2];
            rmse = (double)na1;
            
            if(!this.Value.Transform(transform)) throw new Exception("transformation failed. check tform");
        }





        #endregion
    }
}
