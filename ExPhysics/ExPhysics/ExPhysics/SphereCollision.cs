using ExMath.Coordinate;
using ExMath.Geometry;
using System;

namespace ExPhysics
{
    /// <summary>
    /// Sphere collision
    /// </summary>
    /// <remarks>
    /// The most efficient collision
    /// </remarks>
    public class SphereCollision : Collision
    {
        #region Properties
        /// <summary>
        /// Sphere math data structure
        /// </summary>
        private Sphere sphere;
        public Sphere Sphere { get { return sphere; } }

        /// <summary>
        /// Sphere center
        /// </summary>
        public override Vector3 Position { get { return sphere.center; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sphere">sphere</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public SphereCollision(Sphere sphere, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            this.sphere = sphere;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x-axis posistion</param>
        /// <param name="y">y-axis posistion</param>
        /// <param name="z">z-axis posistion</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public SphereCollision(float x, float y, float z, float radius, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            sphere = new Sphere(x, y, z, radius);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">sphere center position</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public SphereCollision(Vector3 pos, float radius, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            sphere = new Sphere(pos, radius);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Detect sphere collision event
        /// </summary>
        public override bool IsCollision(Collision other)
        {
            Type type = other.GetType();
            if (type == typeof(CubeCollision))
            {
                return Sphere.isIntersect(sphere, (other as CubeCollision).Cube);
            }
            else if (type == typeof(SphereCollision))
            {
                return Sphere.isIntersect(sphere, (other as SphereCollision).Sphere);
            }
            else if (type == typeof(CompositeCollision))
            {
                return (other as CompositeCollision).IsCollision(this);
            }
            return false;
        }

        /// <summary>
        /// Set sphere center
        /// </summary>
        public override void SetPos(params Vector3[] pos)
        {
            if (pos.Length >= 1)
            {
                sphere.center = new Vector3(pos[0].x, pos[0].y, pos[0].z);
            }
        }

        public override bool InBound(Vector3 pos)
        {
            return sphere.InBound(pos);
        }

        public override object GetShape()
        {
            return sphere;
        }
        #endregion

        public void SetRadius(float radius)
        {
            sphere.radius = radius;
        }
    }
}
