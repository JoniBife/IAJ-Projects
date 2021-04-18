using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class ClosedSetDictionary : IClosedSet
    {
        private Dictionary<int, NodeRecord> NodeRecords;
        private int height;

        public ClosedSetDictionary(int height)
        {
            this.height = height;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            NodeRecords.Add(CalculateKey(nodeRecord), nodeRecord);
        }

        public ICollection<NodeRecord> All()
        {
            return NodeRecords.Values;
        }

        public void Initialize()
        {
            NodeRecords = new Dictionary<int, NodeRecord>();
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            NodeRecords.Remove(CalculateKey(nodeRecord));
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord found;
            if (NodeRecords.TryGetValue(CalculateKey(nodeRecord), out found))
            {
                return found;
            }
            return null;
        }

        private int CalculateKey(NodeRecord nodeRecord)
        {
            return height * nodeRecord.x + nodeRecord.y;
        }
    }
}