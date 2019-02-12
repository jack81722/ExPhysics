using ExMath.Coordinate;

namespace ExPhysics
{
    /// <summary>
    /// Collision base
    /// </summary>
    public abstract class Collision
    {
        #region Properties
        public bool active { get; private set; }
        private CollisionManager manager;
        public delegate void CollisionHandler(Collision other);
        public CollisionHandler OnCollision;

        /// <summary>
        /// Position of collision
        /// </summary>
        public abstract Vector3 Position { get; }

        protected const string DEFAULT_TAG = "Default";
        /// <summary>
        /// Tag of collision
        /// </summary>
        private string _tag;
        public string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                if (manager != null)
                {
                    manager.Unregister(this);
                    _tag = value;
                    manager.Register(this);
                }
                else
                {
                    _tag = value;
                }
            }
        }

        public object CustomData;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor with tag
        /// </summary>
        /// <param name="tag">collision tag</param>
        /// <param name="cdata">custom data</param>
        public Collision(string tag = DEFAULT_TAG, object cdata = null)
        {
            this._tag = tag;
            active = true;
            CustomData = cdata;
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Detect collision event between other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool IsCollision(Collision other);

        /// <summary>
        /// Set collision position
        /// </summary>
        /// <param name="pos"></param>
        public abstract void SetPos(params Vector3[] pos);

        /// <summary>
        /// Check if position in bound
        /// </summary>
        /// <param name="pos">position</param>
        public abstract bool InBound(Vector3 pos);

        /// <summary>
        /// Get shape of collision
        /// </summary>
        /// <returns>Geomatric shape</returns>
        public abstract object GetShape();
        #endregion

        #region Methods
        /// <summary>
        /// Set collision manager
        /// </summary>
        public void SetManager(CollisionManager manager)
        {
            if (this.manager == manager)
                return;
            // Keep the collision uniqu for all manager
            if (this.manager != null)
                this.manager.Unregister(this);
            this.manager = manager;
            if(this.manager != null)
                this.manager.Register(this);
        }

        /// <summary>
        /// Set this collision turn on/off collision detect
        /// </summary>
        /// <param name="active">active or not</param>
        public void SetActive(bool active)
        {
            this.active = active;
        }
        #endregion
    }
}
