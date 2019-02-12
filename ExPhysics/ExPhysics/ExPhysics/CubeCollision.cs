using ExMath.Coordinate;
using ExMath.Geometry;
using System;

namespace ExPhysics
{
    /// <summary>
    /// The cube collision
    /// </summary>
    public class CubeCollision : Collision
    {
        #region Properties
        private Cube cube;
        public Cube Cube { get { return cube; } }

        public override Vector3 Position { get { return cube.center; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cube">cube</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public CubeCollision(Cube cube, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            this.cube = cube;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">cube position</param>
        /// <param name="size">cube size</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public CubeCollision(Vector3 center, Vector3 size, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            cube = new Cube(center, size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">x-axis posistion</param>
        /// <param name="y">y-axis posistion</param>
        /// <param name="z">z-axis posistion</param>
        /// <param name="xSize">x-axis size</param>
        /// <param name="ySize">y-axis size</param>
        /// <param name="zSize">z-axis size</param>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public CubeCollision(float x, float y, float z, float xSize, float ySize, float zSize, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            cube = new Cube(new Vector3(x, y, z), new Vector3(xSize, ySize, zSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center">cube center position</param>
        /// <param name="xSize">x-axis size</param>
        /// <param name="ySize">y-axis size</param>
        /// <param name="zSize">z-axis size</param>
        /// <param name="tag">collision tag</param>
        public CubeCollision(Vector3 center, float xSize, float ySize, float zSize, string tag = DEFAULT_TAG) : base(tag)
        {
            cube = new Cube(center, new Vector3(xSize, ySize, zSize));
        }
        #endregion

        #region Override Methods
        public override bool IsCollision(Collision other)
        {
            Type type = other.GetType();
            if(type == typeof(CubeCollision))
            {
                return Cube.isIntersect(cube, (other as CubeCollision).Cube);
            }
            else if(type == typeof(SphereCollision))
            {
                return Cube.isIntersect(cube, (other as SphereCollision).Sphere);
            }
            else if(type == typeof(CompositeCollision))
            {
                return (other as CompositeCollision).IsCollision(this);
            }
            return false;
        }

        public override void SetPos(params Vector3[] pos)
        {
            cube.center = pos[0];
        }

        public override bool InBound(Vector3 pos)
        {
            return cube.InBound(pos);
        }

        public override object GetShape()
        {
            return cube;
        }
        #endregion
    }
}
