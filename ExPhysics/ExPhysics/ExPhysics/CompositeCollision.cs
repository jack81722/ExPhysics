using ExMath.Coordinate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExPhysics
{
    /// <summary>
    /// Composite of multiple geometry collisionss
    /// </summary>
    public class CompositeCollision : Collision
    {
        #region Properties
        /// <summary>
        /// Multiple collision
        /// </summary>
        private List<Collision> collisions;
        private object collision;

        /// <summary>
        /// Center of gravity
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                if (collisions.Count <= 0)
                    return Vector3.zero;
                Vector3 center = Vector3.zero;
                for (int i = 0; i < collisions.Count; i++)
                {
                    center += collisions[i].Position;
                }
                center /= collisions.Count;
                return center;
            }
        }
        #endregion

        #region Constructor
        public CompositeCollision(string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            this.collisions = new List<Collision>();
        }

        public CompositeCollision(Collision[] collisions, string tag = DEFAULT_TAG, object cdata = null) : base(tag, cdata)
        {
            this.collisions = new List<Collision>(collisions);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Detect collision event with other
        /// </summary>
        public override bool IsCollision(Collision other)
        {
            try
            {
                lock (collisions)
                {
                    return collisions.Exists(c => c.IsCollision(other));
                }
            }
            catch (Exception e)
            {
                //Printer.WriteWarning(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Set all positions of collisions
        /// </summary>
        public override void SetPos(params Vector3[] pos)
        {
            int count = Math.Min(pos.Length, collisions.Count);
            //Printer.WriteWarning("pos length = " + pos.Length + ", collision length = " + collisions.Count);
            for (int i = 0; i < count; i++)
            {
                collisions[i].SetPos(pos[i]);
            }
        }

        public override bool InBound(Vector3 pos)
        {
            int count = collisions.Count;
            return collisions.Exists(col => col.InBound(pos));
        }

        public override object GetShape()
        {
            lock (collisions)
            {
                object[] shapes = collisions.Select(col => col.GetShape()).ToArray();
                return shapes;
            }
        }

        public void SetAllSize(float radius)
        {
            for(int i = 0; i < collisions.Count; i++)
            {
                if(collisions[i].GetType() == typeof(SphereCollision))
                {
                    ((SphereCollision)collisions[i]).SetRadius(radius);
                }
            }
        }

        /// <summary>
        /// Add collision to this
        /// </summary>
        /// <param name="collision"></param>
        public void AddCollision(Collision collision)
        {
            collisions.Add(collision);
        }

        /// <summary>
        /// Add multiple collisions to this
        /// </summary>
        /// <param name="collisions"></param>
        public void AddCollisions(IEnumerable<Collision> collisions)
        {
            this.collisions.AddRange(collisions);
        }

        /// <summary>
        /// Get all collision datas
        /// </summary>
        /// <returns></returns>
        public Collision[] GetCollisions()
        {
            return collisions.ToArray();
        }

        /// <summary>
        /// Get ranged collision datas
        /// </summary>
        /// <returns></returns>
        public Collision[] GetCollisions(int index, int count)
        {
            try
            {
                return collisions.GetRange(index, count).ToArray();
            }
            catch (Exception e)
            {
                //Printer.WriteWarning(e.Message + ", " + e.StackTrace);
                return collisions.GetRange(0, System.Math.Min(count, collisions.Count)).ToArray();
            }
        }

        /// <summary>
        /// Remove specific collision from this
        /// </summary>
        /// <param name="collision"></param>
        public void RemoveCollision(Collision collision)
        {
            collisions.Remove(collision);
        }

        /// <summary>
        /// Remove multiple collision from this
        /// </summary>
        /// <param name="collision"></param>
        public void RemoveCollisions(Collision[] collisions)
        {
            for (int i = 0; i < collisions.Length; i++)
            {
                if (this.collisions.Contains(collisions[i]))
                    this.collisions.Remove(collisions[i]);
            }
        }

        public void RemoveCollisionRange(int index, int count)
        {
            lock (collisions)
            {
                collisions.RemoveRange(index, count);
            }
        }

        /// <summary>
        /// Clear all collisions
        /// </summary>
        public void ClearCollisions()
        {
            collisions.Clear();
        }
        #endregion
    }
}
