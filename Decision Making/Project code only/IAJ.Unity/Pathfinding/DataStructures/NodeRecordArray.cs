using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        private NodeRecord[] NodeRecords { get; set; }
        private NodePriorityHeap Open { get; set; }

        public NodeRecordArray(List<NodeRecord> nodes)
        {
            //this method creates and initializes the NodeRecordArray for all nodes in the Navigation Graph
            this.NodeRecords = new NodeRecord[nodes.Count];

            foreach (var n in nodes)
                NodeRecords[n.index] = n;
            
            this.Open = new NodePriorityHeap();
        }

        public NodeRecord GetNodeRecord(NodeRecord node)
        {

            return NodeRecords[node.index];
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                if(NodeRecords[i].isWalkable)
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

        }

        void IClosedSet.Initialize()
        {
         
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            this.Open.AddToOpen(nodeRecord);
            nodeRecord.status = NodeStatus.Open;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Closed;
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            var storedNode = this.GetNodeRecord(nodeRecord);

            if (storedNode != null && storedNode.status == NodeStatus.Open) return storedNode;

            else return null;
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            var storedNode = this.GetNodeRecord(nodeRecord);

            if (storedNode != null && storedNode.status == NodeStatus.Closed) return storedNode;
            else return null;
        }

        public NodeRecord GetBestAndRemove()
        {
            //TODO implement
            return this.Open.GetBestAndRemove();
        }

        public NodeRecord PeekBest()
        {
            //TODO implement
            return this.Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            //TODO implement
            this.Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            //TODO implement
            this.Open.RemoveFromOpen(nodeRecord);
            nodeRecord.status = NodeStatus.Unvisited;
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            //TODO implement
            nodeRecord.status = NodeStatus.Unvisited;
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            //TODO implement
            return this.Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            //TODO implement
            return this.NodeRecords.Where(node => node.status == NodeStatus.Closed).ToList();
        }

        public int CountOpen()
        {
            //TODO implement
            return this.Open.CountOpen();
        }
    }
}
