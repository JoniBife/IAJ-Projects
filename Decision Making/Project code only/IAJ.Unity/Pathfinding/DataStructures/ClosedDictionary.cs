using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    class ClosedDictionary : IClosedSet
    {

        private Dictionary<Vector2, NodeRecord> Closed { get; set; }

        public ClosedDictionary()
        {
            this.Closed = new Dictionary<Vector2, NodeRecord>();
        }

        public void Initialize()
        {
            this.Closed.Clear();
        }


        public void AddToClosed(NodeRecord nodeRecord)
        {
            var vec = new Vector2(nodeRecord.x, nodeRecord.y);
            if (!Closed.ContainsKey(vec))
                Closed.Add(vec, nodeRecord);
            else Closed[vec] = nodeRecord;
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.Closed.Remove(new Vector2(nodeRecord.x, nodeRecord.y));
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            //here I cannot use the == comparer because the nodeRecord will likely be a different computational object
            //and therefore pointer comparison will not work, we need to use Equals
            //LINQ with a lambda expression

            var key = new Vector2(nodeRecord.x, nodeRecord.y);

            if (Closed.ContainsKey(key))
            {
                return this.Closed[key];

            }
            else return null;
        }



        public ICollection<NodeRecord> All()
        {
            return this.Closed.Values;
        }


    }

}

