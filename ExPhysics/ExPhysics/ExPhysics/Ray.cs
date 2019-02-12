using ExMath;
using ExMath.Coordinate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExPhysics
{
    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direct;

        /// <summary>
        /// Unit vector of ray
        /// </summary>
        public Vector3 unit { get { return (direct - origin).normalized; } }

        /// <summary>
        /// Distance of ray
        /// </summary>
        public float length { get { return Vector3.Distance(origin, direct); } }

        #region Constructor
        public Ray(Vector3 ori, Vector3 dir)
        {
            origin = ori;
            direct = dir;
        }
        #endregion

        #region Public Methods
        public void SetLength(float l)
        {
            direct = origin + unit * l;
        }
        #endregion

        #region TryCast Methods
        public bool TryCast(Collision collision, out Vector3 point)
        {
            try
            {   
                if (collision.GetType() == typeof(SphereCollision))
                {
                    return TryCast((SphereCollision)collision, out point);
                }
                else if (collision.GetType() == typeof(CubeCollision))
                {
                    return TryCast((CubeCollision)collision, out point);
                }
                else
                {
                    point = default(Vector3);
                }
            }
            catch (Exception e)
            {
                point = default(Vector3);
                Console.WriteLine($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        private bool TryCast(SphereCollision collision, out Vector3 point)
        {
            Vector3 center = collision.Position;
            float radius = collision.Sphere.radius;
            // get unit vector of ray
            Vector3 l = unit;
            float dotValue = Vector3.Dot(l, origin - center);
            var root = dotValue * dotValue - ((origin - center).sqrMagnitude - radius * radius);

            // cases of root
            point = default(Vector3);
            if (root < 0)
            {   
                return false;
            }
            else if(root == 0)
            {
                float dist = -dotValue;
                point = origin + l * dist;
                return true;
            }
            else if(root > 0)
            {
                float dmin = -dotValue - (float)Math.Sqrt(root);
                float dmax = -dotValue + (float)Math.Sqrt(root);
                float d = dmin < 0 ? dmax : dmin;
                point = origin + l * d;
                return true;
            }
            return false;
        }

        private bool TryCast(CubeCollision collision, out Vector3 point)
        {
            Vector3 vector = (direct - origin).normalized;
            Vector3 min = collision.Cube.min;
            Vector3 max = collision.Cube.max;
            Vector3 tMin = new Vector3((min.x - origin.x) / vector.x, (min.y - origin.y) / vector.y, (min.z - origin.z) / vector.z);
            Vector3 tMax = new Vector3((max.x - origin.x) / vector.x, (max.y - origin.y) / vector.y, (max.z - origin.z) / vector.z);

            // compare & swap if min > max
            if (tMin.x > tMax.x) Formula.Swap(ref tMin.x, ref tMax.x);
            if (tMin.y > tMax.y) Formula.Swap(ref tMin.y, ref tMax.y);
            if (tMin.z > tMax.z) Formula.Swap(ref tMin.z, ref tMax.z);

            // find the minimum distance of closest point
            point = default(Vector2);
            if ((tMin.x > tMax.y) || (tMin.y > tMax.x)) return false;

            float tmin = (tMin.x > tMin.y) ? tMin.x : tMin.y;
            float tmax = (tMax.x < tMax.y) ? tMax.x : tMax.y;

            if ((tmin > tMax.z) || (tMin.z > tmax)) return false;

            tmin = (tMin.z > tmin) ? tMin.z : tmin;
            tmax = (tMax.z < tmax) ? tMax.z : tmax;
            float d = tmin < 0 ? tmax : tmin;
            point = origin + vector * d;
            return true;
        }
        #endregion
    }
}
