using ExMath.Coordinate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExPhysics
{
    /// <summary>
    /// A manager of all ExPhysics collisions
    /// </summary>
    /// <remarks>
    /// Register collision of Exphysics into manager, and call Update method somewhere. It will automatically detect collision event.
    /// Also you can set tags of collisions easily and switch collision trigger on/off tag-by-tag.
    /// </remarks>
    public class CollisionManager
    {
        #region Properties
        /// <summary>
        /// Restered and tagged collision dictionary
        /// </summary>
        public Dictionary<string, List<Collision>> TagCollisions;

        /// <summary>
        /// Tag list for all registered tags
        /// </summary>
        public List<string> TagList;

        /// <summary>
        /// Tag trigger table
        /// </summary>
        public bool[,] TagTriggerTable;

        /// <summary>
        /// Active of collision manager
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Detect if collision manager executing updating loop
        /// </summary>
        public bool updating { get; private set; }

        /// <summary>
        /// Add temp if register at updating
        /// </summary>
        private List<Collision> added;

        /// <summary>
        /// Remove temp if unregister at updating
        /// </summary>
        private List<Collision> removed;

        /// <summary>
        /// Threads of detecting collisions
        /// </summary>
        private List<Task> detectTasks;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of collision manager
        /// </summary>
        public CollisionManager()
        {
            TagCollisions = new Dictionary<string, List<Collision>>();
            TagList = new List<string>();
            TagTriggerTable = new bool[TagList.Count, TagList.Count];
            added = new List<Collision>();
            removed = new List<Collision>();
            detectTasks = new List<Task>();
        }
        #endregion

        #region CastInfo
        /// <summary>
        /// Raycast result
        /// </summary>
        public struct HitInfo
        {
            /// <summary>
            /// Collision on hit
            /// </summary>
            public Collision collision;

            /// <summary>
            /// Point on hit
            /// </summary>
            public Vector3 point;

            /// <summary>
            /// Vector between ray origin and point
            /// </summary>
            public Vector3 vector;

            public HitInfo(Collision col, Vector3 p, Vector3 v)
            {
                collision = col;
                point = p;
                vector = v;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add collision into manager
        /// </summary>
        /// <param name="collision">Added collision</param>
        private void AddCollision(Collision collision)
        {
            lock (TagCollisions)
            {
                if (!TagCollisions.ContainsKey(collision.Tag))
                    AddTag(collision.Tag);
                if (!TagCollisions[collision.Tag].Contains(collision))
                {
                    TagCollisions[collision.Tag].Add(collision);
                    collision.SetManager(this);
                }
            }
        }

        /// <summary>
        /// Remove collision from manager
        /// </summary>
        /// <param name="collisions">Removed collision</param>
        private void RemoveCollision(Collision collision)
        {
            lock (TagCollisions)
            {
                if (TagCollisions.ContainsKey(collision.Tag))
                {
                    if (TagCollisions[collision.Tag].Contains(collision))
                    {
                        TagCollisions[collision.Tag].Remove(collision);
                    }
                }
            }
        }

        /// <summary>
        /// Add new tag to manager
        /// </summary>
        private void AddTag(string tag)
        {
            lock (TagList)
            {
                TagList.Add(tag);
                TagCollisions.Add(tag, new List<Collision>());
                UpdateTagBoolTable();
            }
        }

        /// <summary>
        /// Resize tag trigger table after new tag saved
        /// </summary>
        private void UpdateTagBoolTable()
        {
            lock (TagTriggerTable)
            {
                var oldTable = TagTriggerTable;
                // create new 2D bool array
                TagTriggerTable = new bool[TagList.Count, TagList.Count];
                for (int i = 0; i < TagList.Count; i++)
                {
                    for (int j = 0; j < TagList.Count; j++)
                    {
                        if (i < oldTable.GetLength(0) && j < oldTable.GetLength(1))
                        {
                            TagTriggerTable[i, j] = oldTable[i, j];
                        }
                        else
                        {
                            // default true
                            TagTriggerTable[i, j] = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear datas of collisions safely
        /// </summary>
        private void ClearSafely()
        {
            while (updating)
                Task.Delay(10);
            active = false;
            lock(TagCollisions)
                TagCollisions.Clear();
            lock(TagList)
                TagList.Clear();
            lock(TagTriggerTable)
                TagTriggerTable = new bool[0, 0];
            removed.Clear();
        }

        /// <summary>
        /// Detect tag collisions
        /// </summary>
        /// <param name="tag1"></param>
        /// <param name="tag2"></param>
        private void Detect(string tag1, string tag2)
        {
            // check if tagged collision is existed and tag trigger is true
            if (TagCollisions[tag1].Count > 0 && TagCollisions[tag2].Count > 0)
            {
                // if same tag
                if (tag1 == tag2)
                {
                    var list = TagCollisions[tag1];
                    for (int i = 0; i < list.Count - 1; i++)
                    {
                        // if collision not null and active
                        if (list[i] != null && list[i].active)
                        {
                            for (int j = i + 1; j < list.Count; j++)
                            {
                                // if collision not null and active
                                if (list[j] != null && list[j].active && list[i].IsCollision(list[j]))
                                {
                                    try
                                    {
                                        if (list[i].OnCollision != null)
                                            list[i].OnCollision.Invoke(list[j]);
                                        if (list[j].OnCollision != null)
                                            list[j].OnCollision.Invoke(list[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        //Printer.WriteError("Collision Manager update failed : " + e.Message + "\n" + e.StackTrace);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // not same tag
                    var xList = TagCollisions[tag1];
                    var yList = TagCollisions[tag2];
                    for (int i = 0; i < xList.Count; i++)
                    {
                        // if collision is not null and active
                        if (xList[i] != null && xList[i].active)
                        {
                            for (int j = 0; j < yList.Count; j++)
                            {
                                // if collision is not null, active, and collide
                                if (yList[j] != null && yList[j].active && xList[i].IsCollision(yList[j]))
                                {
                                    try
                                    {
                                        if (xList[i].OnCollision != null)
                                            xList[i].OnCollision.Invoke(yList[j]);
                                        if (yList[j].OnCollision != null)
                                            yList[j].OnCollision.Invoke(xList[i]);
                                    }
                                    catch (Exception e)
                                    {
                                        //Printer.WriteError("Collision Manager update failed : " + e.Message + "\n" + e.StackTrace);
                                    }
                                }
                            }
                        }
                    }
                }// end of same tag
            }
        }

        /// <summary>
        /// Refresh added/removed collisions
        /// </summary>
        private void Refresh()
        {
            // add all collisions in added list
            lock (added)
            {
                for (int i = 0; i < added.Count; i++)
                {
                    AddCollision(added[i]);
                }
                added.Clear();
            }
            // remove all collisions in removed list
            lock (removed)
            {
                for (int i = 0; i < removed.Count; i++)
                {
                    RemoveCollision(removed[i]);
                }
                removed.Clear();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Detect collisions event
        /// </summary>
        /// <param name="multithread">Execute with multi-thread</param>
        public void Update(bool multithread = true)
        {
            if (!active)
                return;

            if (!updating)
            {
                updating = true;
                lock (TagList)
                {
                    // execute detect with multi-thread
                    if (multithread)
                    {
                        // all of tags loop
                        for (int x = 0; x < TagList.Count; x++)
                        {
                            // opponent tags loop
                            for (int y = x; y < TagList.Count; y++)
                            {
                                string tag1 = TagList[x];
                                string tag2 = TagList[y];
                                // check if tag trigger is true
                                if (GetTagTrigger(tag1, tag2))
                                {
                                    // create detect task
                                    Task task = Task.Factory.StartNew(() =>
                                    {
                                        try
                                        {
                                            Detect(tag1, tag2);
                                        }
                                        catch (Exception e)
                                        {
                                            //Printer.WriteWarning($"Detect failed : {e.Message}\n{e.StackTrace}");
                                        }
                                    });
                                    // add task 
                                    detectTasks.Add(task);
                                }
                            }
                        }
                        // wait all detecting task
                        for (int i = 0; i < detectTasks.Count; i++)
                        {
                            detectTasks[i].Wait();
                        }
                        detectTasks.Clear();
                    }
                    else
                    {
                        // all of tags loop
                        for (int x = 0; x < TagList.Count; x++)
                        {
                            // opponent tags loop
                            for (int y = x; y < TagList.Count; y++)
                            {
                                string tag1 = TagList[x];
                                string tag2 = TagList[y];
                                if (GetTagTrigger(tag1, tag2))
                                {
                                    Detect(tag1, tag2);
                                }
                            }
                        }
                    }
                }
            }// end of updating
            updating = false;
            // refresh added and removed collisions
            Refresh();
        }

        #region Raycast Methods
        /// <summary>
        /// Detect what ray intersect and return hit infos
        /// </summary>
        /// <param name="ray">ray</param>
        /// <param name="hits">hit info ray intersected</param>
        public void Raycast(Ray ray, out List<HitInfo> hits)
        {
            hits = new List<HitInfo>();
            Vector3 point;
            HitInfo hit;
            foreach (var colList in TagCollisions.Values)
            {
                for(int i = 0; i < colList.Count; i++)
                {
                    if(ray.TryCast(colList[i], out point))
                    {
                        hit = new HitInfo(colList[i], point, point - ray.origin);
                        hits.Add(hit);
                    }
                }
            }
            // sort hit infos by distance
            hits.Sort(delegate (HitInfo h1, HitInfo h2) { return (int)Math.Round((h1.vector.sqrMagnitude - h2.vector.sqrMagnitude) * 1000); });
        }

        /// <summary>
        /// Detect what ray intersect and return hit infos
        /// </summary>
        /// <param name="ray">ray</param>
        /// <param name="distance">specific distance</param>
        /// <param name="hits">hit info ray intersected</param>
        public void Raycast(Ray ray, float distance, out List<HitInfo> hits)
        {
            ray.SetLength(distance);
            hits = new List<HitInfo>();
            Vector3 point;
            HitInfo hit;
            foreach (var colList in TagCollisions.Values)
            {
                for (int i = 0; i < colList.Count; i++)
                {
                    if (ray.TryCast(colList[i], out point))
                    {
                        hit = new HitInfo(colList[i], point, point - ray.origin);
                        hits.Add(hit);
                    }
                }
            }
            // sort hit infos by distance
            hits.Sort(delegate (HitInfo h1, HitInfo h2) { return (int)Math.Round((h1.vector.sqrMagnitude - h2.vector.sqrMagnitude) * 1000); });
        }

        /// <summary>
        /// Detect what ray intersect and return hit infos by specific tags
        /// </summary>
        /// <param name="ray">ray</param>
        /// <param name="hits">hit info ray intersected</param>
        /// <param name="tags">specific tags</param>
        public void Raycast(Ray ray, out List<HitInfo> hits, params string[] tags)
        {
            hits = new List<HitInfo>();
            Vector3 point;
            HitInfo hit;
            List<Collision> colList;
            // onle execute with specific tags
            for(int t = 0; t < tags.Length; t++)
            {
                if (TagCollisions.TryGetValue(tags[t], out colList))
                {
                    // try for all collisions in lists
                    for (int i = 0; i < colList.Count; i++)
                    {
                        if (ray.TryCast(colList[i], out point))
                        {
                            hit = new HitInfo(colList[i], point, point - ray.origin);
                            hits.Add(hit);
                        }
                    }
                }
            }
            // sort hit infos by distance
            hits.Sort(delegate (HitInfo h1, HitInfo h2) { return (int)Math.Round((h1.vector.sqrMagnitude - h2.vector.sqrMagnitude) * 1000); });
        }

        /// <summary>
        /// Detect what ray intersect and return hit infos by specific tags
        /// </summary>
        /// <param name="ray">ray</param>
        /// <param name="hits">hit info ray intersected</param>
        /// <param name="tags">specific tags</param>
        public void Raycast(Ray ray, float distance, out List<HitInfo> hits, params string[] tags)
        {
            ray.SetLength(distance);
            hits = new List<HitInfo>();
            Vector3 point;
            HitInfo hit;
            List<Collision> colList;
            for (int t = 0; t < tags.Length; t++)
            {
                if (TagCollisions.TryGetValue(tags[t], out colList))
                {
                    for (int i = 0; i < colList.Count; i++)
                    {
                        if (ray.TryCast(colList[i], out point))
                        {
                            hit = new HitInfo(colList[i], point, point - ray.origin);
                            hits.Add(hit);
                        }
                    }
                }
            }
            // sort hit infos by distance
            hits.Sort(delegate (HitInfo h1, HitInfo h2) { return (int)Math.Round((h1.vector.sqrMagnitude - h2.vector.sqrMagnitude) * 1000); });
        }
        #endregion

        /// <summary>
        /// Register collision into manager
        /// </summary>
        public void Register(Collision collision)
        {
            if (updating)
            {
                added.Add(collision);
                return;
            }
            AddCollision(collision);
        }
         
        /// <summary>
        /// Register multiple collisions into manager
        /// </summary>
        public void Register(Collision[] collisions)
        {
            for(int i = 0; i < collisions.Length; i++)
            {
                Register(collisions[i]);
            }
        }

        /// <summary>
        /// Unregister collision from manager
        /// </summary>
        public void Unregister(Collision collision)
        {
            if (updating)
            {
                //collision.SetActive(false);
                removed.Add(collision);
                return;
            }

            RemoveCollision(collision);
        }

        /// <summary>
        /// Unregister collision from manager
        /// </summary>
        public void Unregister(Collision[] collisions)
        {
            if (updating)
            {
                removed.AddRange(collisions);
                return;
            }

            for(int i = 0; i < collisions.Length;i ++)
                RemoveCollision(collisions[i]);
        }

        /// <summary>
        /// Switch on/off trigger between tag1 and tag2
        /// </summary>
        public void SetTagTrigger(string tag1, string tag2, bool b)
        {
            if (!TagList.Contains(tag1))
            {
                AddTag(tag1);
            }
            if (!TagList.Contains(tag2))
            {
                AddTag(tag2);
            }
            int i = TagList.IndexOf(tag1);
            int j = TagList.IndexOf(tag2);
            TagTriggerTable[i, j] = TagTriggerTable[j, i] = b;
        }

        /// <summary>
        /// Check trigger boolean between tag1 and tag2
        /// </summary>
        public bool GetTagTrigger(string tag1, string tag2)
        {
            if (TagList.Contains(tag1) && TagList.Contains(tag2))
            {
                int i = TagList.IndexOf(tag1);
                int j = TagList.IndexOf(tag2);
                return TagTriggerTable[i, j];
            }
            return false;
        }

        /// <summary>
        ///  Clear datas of collisions
        /// </summary>
        public void Clear()
        {
            Task.Factory.StartNew(() => ClearSafely());
        }
        #endregion
    }
}
