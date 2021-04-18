using System;

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

        public NodeRecord parent;
        public float gCost;
        public float hCost;
        public float fCost;
        public int index;
        public NodeStatus status;
        public bool isWalkable;

       
        public bool U, D, L, R;

        public int[,] distanceArray = new int[3, 3];
       

        public override string ToString()
        {
            return x + "," + y;
        }

        public int CompareTo(NodeRecord other)
        {
            return   this.fCost.CompareTo(other.fCost);
        }

        public NodeRecord(int x, int y)
        {
            
            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            index = 0;
            isWalkable = true;
            status = NodeStatus.Unvisited;
        }

        public NodeRecord(int x, int y, int _index)
        {

            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            index = _index;
            isWalkable = true;
            status = NodeStatus.Unvisited;
        }
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        //two node records are equal if they refer to the same node
        public override bool Equals(object obj)
        {
            var target = obj as NodeRecord;
            if (target != null) return this.x == target.x && this.y == target.y;
            else return false;
        }
    }
}
