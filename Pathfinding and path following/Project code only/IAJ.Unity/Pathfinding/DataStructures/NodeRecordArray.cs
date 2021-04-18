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

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeRecords[i] = nodes[i];
            }

            this.Open = new NodePriorityHeap();
        }

        public void GetNodeRecord(NodeRecord node)
        {
            //do not change this method
            //here we have the "special case" node handling
            /*     if (node.NodeIndex == -1)
                 {
                     for (int i = 0; i < this.SpecialCaseNodes.Count; i++)
                     {
                         if (node == this.SpecialCaseNodes[i])
                         {
                             return this.SpecialCaseNodes[i];
                         }
                     }
                     return null;
                 }
                 else
                 {
                     return  this.NodeRecords[node.NodeIndex];
                 }*/
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }
        }

        void IClosedSet.Initialize()
        {
            // Does nothing
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {

            Open.AddToOpen(nodeRecord);
            NodeRecords[nodeRecord.idx] = nodeRecord;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            NodeRecords[nodeRecord.idx] = nodeRecord;
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            NodeRecord inArray = NodeRecords[nodeRecord.idx];
            if (inArray.status == NodeStatus.Open)
            {
                return inArray;
            }
            return null;
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord inArray = NodeRecords[nodeRecord.idx];
            if (inArray.status == NodeStatus.Closed)
                return inArray;
            return null;
        }

        public NodeRecord GetBestAndRemove()
        {
            return Open.GetBestAndRemove();
        }

        public NodeRecord PeekBest()
        {
            return Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            NodeRecords[nodeToReplace.idx] = nodeToReplace;
            Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            Open.RemoveFromOpen(nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            // DO Nothing
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            return Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            return NodeRecords.Where((nr) => nr.status == NodeStatus.Closed).ToList();
        }

        public int CountOpen()
        {
            return Open.CountOpen();
        }
    }
}
