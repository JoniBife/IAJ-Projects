using System;
using System.Diagnostics;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public enum NodeStatus
    {
        Unvisited,
        Open,
        Closed
    }

    public class NodeRecord  : IComparable<NodeRecord>
    {
        //Coordinates
        public int x;
        public int y;

        public int idx;

        public NodeRecord parent;
        public float gCost;
        public float hCost;
        public float fCost;
        public NodeStatus status;
        public bool isWalkable;
        public bool isTargetJumpPoint;

        public override string ToString()
        {
            return x + "," + y;
        }

        public int CompareTo(NodeRecord other)
        {
            int fComparison = this.fCost.CompareTo(other.fCost);
            if (fComparison == 0)
            {
                return this.hCost.CompareTo(other.hCost);
            }
            return fComparison;
        }

        public NodeRecord(int x, int y)
        {
            
            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            isWalkable = true;
            status = NodeStatus.Unvisited;
        }

        public NodeRecord(int x, int y, int idx)
        {
            this.idx = idx;
            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            isWalkable = true;
            status = NodeStatus.Unvisited;
        }
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
