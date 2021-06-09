using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;

namespace Scan2BIM_WIP
{
    public class GH_PointCloud : GH_GeometricGoo<PointCloud>
    {
        // Warning: create a child of GH_GeometricGoo requires the implementation of all geometry classes including Boundingbox, etc.
        
        /// <summary>
        /// Fields of the GH_PointCloud class
        /// </summary>
        #region fields 
 
        /// <summary>
        /// GH_PointCloud centerpoint 
        /// </summary>
        public Point3d MyPoint { get; set; }

        /// <summary>
        /// GH_PointCloud number of points
        /// </summary>
        public Double MyDouble { get; set; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GH_PointCloud()
        {
            Value = new PointCloud(); //  this is the  inherited property from the parent PointCloud. can we add extra stuff? we should build our octree around this class
            MyDouble = Value.Count; 
            MyPoint = Point3d.Unset; //base point           
        }

        /// <summary>
        /// Create GH_PointCloud from a Rhino.Geometry PointCloud
        /// </summary>
        public GH_PointCloud(PointCloud pointCloud)
        {            
            Value = pointCloud;
            MyDouble = Value.Count; // MyDouble takes on the count of the point cloud
            MyPoint = Value.PointAt(0); // MyPoint takes on the first point in the point cloud
        }

        /// <summary>
        /// Create GH_PointCloud from some more inputs
        /// </summary>
        public GH_PointCloud(PointCloud pointCloud, Double myDouble, Point3d myPoint) 
        {
            Value = pointCloud;
            MyDouble = myDouble;
            MyPoint = myPoint;
        }

        /// <summary>
        /// Create GH_PointCloud from another GH_PointCloud
        /// </summary>        
        public GH_PointCloud(GH_PointCloud pointcloud) 
        {
            Value = pointcloud.Value;
            MyDouble = pointcloud.MyDouble;
            MyPoint = pointcloud.MyPoint;
        }

        /// <summary>
        /// Duplicate a GH_PointCloud (technically not a constructor)
        /// </summary>
        public override IGH_Goo Duplicate()
        {
            return new GH_PointCloud(this);
        }

        /// <summary>
        /// Duplicate the geometry of a GH_PointCloud entity
        /// </summary>
        public override IGH_GeometricGoo DuplicateGeometry()
        {
            if (!Value.IsValid) { new GH_PointCloud(); }
            return new GH_PointCloud(this);
            //    return new GH_PointCloud(Cloud == null ? new GH_PointCloud() : new GH_PointCloud(Value)); // this is a ternary conditional operator ( condition ? command(true): command (false))

        }

        #endregion

        #region "Properties"
        /// <summary>
        /// Return whether GH_PointCloud is a valid entity
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (MyDouble <= 0.0) { return false; }
                if (Value == null) { return false; }
                if (MyPoint == null) { return false; }

                // check validity pointCloud, center
                if (!Rhino.RhinoMath.IsValidDouble(MyDouble)) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Return why GH_PointCloud is not a valid entity
        /// </summary>
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) { return "No internal GH_PointCloud instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid GH_PointCloud instance"; //Todo: beef this up to be more informative.
            }
        }
        /// <summary>
        /// Return a string with the name of this Type.
        /// </summary>
        public override string TypeName
        {
            get { return "GH_PointCloud"; }
        }

        /// <summary>
        /// Return a string describing what this Type is about.
        /// </summary>
        public override string TypeDescription
        {
            get { return "Defines a GH_PointCloud which is an extension of Rhino.Geometry.PointCloud, used for geometry and machine learning applications."; }
        }

        /// <summary>
        /// Return the Rhino.Geometry.BoundingBox of the point cloud
        /// </summary>
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                if (MyPoint == null ) { return BoundingBox.Empty; }
                return Value.GetBoundingBox(true);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            if (MyPoint == null) { return BoundingBox.Empty; }
            return Value.GetBoundingBox(xform);

        }
        #endregion

        #region "Casting"
        // Return the value we use to represent the type in unsafe code i.e. C# scripts.
        public override object ScriptVariable()
        {
            return Value;
        }

        /// <summary>
        /// Return a string representation of the state (value) of this instance.
        /// </summary>
        public override string ToString()
        {
            if (this.IsValid)
            {
                return ("Point cloud with " + Value.Count + " points");
            }
            else
                return ("Invalid Point cloud");
        }

        /// <summary>
        /// Convert GH_PointCloud to PointCloud
        /// </summary>
        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to GH_PointCloud
            if (typeof(Q).IsAssignableFrom(typeof(GH_PointCloud)))
            {
                if (Value == null)
                    target = default(Q);
                else
                    target = (Q)(object)Value;
                return true;
            }

            //Cast to PointCloud
            if (typeof(Q).IsAssignableFrom(typeof(PointCloud)))
            {
                object ptr = new PointCloud(Value);
                target = (Q)ptr;
                return true;
            }

            target = default(Q);
            return false;
        }
 


    /// <summary>
    /// Convert a source data type to GH_PointCloud
    /// </summary>
    public override bool CastFrom(object source)
        {
            //Abort immediately on bogus data.
            if (source == null) { return false; }

            //First, we try the PointCloud class. By specifying GH_Conversion.Both 
            //we will get both exact and fuzzy results. You should always try to use the
            //methods available through GH_Convert as they are extensive and consistent.

            // we should accomodate multiple datatypes: PointCloud, KdTree, Mesh
            GH_PointCloud target = new GH_PointCloud();
            if (GH_Convert_Extensions.ToGH_PointCloud(source, ref target, GH_Conversion.Both))
            {
                return true;
            }

            //Cast from GH_PointCloud
            if (typeof(GH_PointCloud).IsAssignableFrom(source.GetType()))
            {
                Value = (PointCloud)source;
                return true;
            }

            return false;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Morph the point cloud
        /// </summary>
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            //Abort immediately on bogus data.
            if (!Value.IsValid) { return null; }
            xmorph.Morph(Value);
            return this;
        }


        /// <summary>
        /// Transform the point cloud
        /// </summary>
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            //Abort immediately on bogus data.
            if (!Value.IsValid) { return null; }
            Value.Transform(xform);
            return this;
        }

        #endregion

    }
}
