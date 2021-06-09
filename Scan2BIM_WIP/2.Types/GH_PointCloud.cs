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


namespace Scan2BIM_WIP
{
    public class GH_PointCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        /// <summary>
        /// Fields of the GH_PointCloud class
        /// </summary>
        #region fields 

        // there currently is only the PointCloud field

        #endregion

        #region "Constructors"
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GH_PointCloud()
        {
            this.m_value = new PointCloud();
            
        }

        /// <summary>
        /// Create GH_PointCloud from a Rhino.Geometry PointCloud
        /// </summary>
        public GH_PointCloud(PointCloud cloud)
        {
            this.m_value = cloud;
        }

        /// <summary>
        /// Create GH_PointCloud from another GH_PointCloud
        /// </summary>  
        public GH_PointCloud(GH_PointCloud other)
        {
            m_value = (PointCloud) other.m_value.Duplicate();
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

            var asOtherPC = (GH_GeometricGoo<PointCloud>)source;
            if (asOtherPC != null)
            {
                this.m_value = asOtherPC.Value;
                return true;
            }

            if (source.GetType() == typeof(IEnumerable<Point3d>))
            {
                this.m_value = new PointCloud((IEnumerable<Point3d>)source);
                return true;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(out Q target)
        {

            if (typeof(Q) == typeof(PointCloud))
            {
                target = (Q)(object)this.m_value;
                return true;
            }

            if (typeof(Q) == typeof(GH_PointCloud))
            {
                target = (Q)(object)this;
                return true;
            }

            if (typeof(Q) == typeof(IEnumerable<Point3d>))
            {
                target = (Q)(object)this.m_value.GetPoints();
                return true;
            }

            //return base.CastTo<Q>(out target);
            target = default(Q);
            return false;
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
        /// Compute normals if there aren't any 
        /// </summary>
        public static void ComputeNormals(this PointCloud pointCloud, int k)
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
        #endregion
    }
}
