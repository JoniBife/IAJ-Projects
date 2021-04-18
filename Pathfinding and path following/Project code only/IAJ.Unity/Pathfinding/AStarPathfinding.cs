using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding : Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        public override Grid<NodeRecord> grid { get; set; }
        public override uint NodesPerSearch { get; set; }
        public override uint TotalProcessedNodes { get; protected set; }
        public override int MaxOpenNodes { get; protected set; }
        public override float TotalProcessingTime { get; set; }
        public override uint MaxExploredNodesPerFrame { get; set; }
        public override bool InProgress { get; set; }
        public override IOpenSet Open { get; protected set; }
        public override IClosedSet Closed { get; protected set; }
 
        //heuristic function
        public override IHeuristic Heuristic { get; protected set; }

        public override int StartPositionX { get; set; }
        public override int StartPositionY { get; set; }
        public override int GoalPositionX { get; set; }
        public override int GoalPositionY { get; set; }
        public override NodeRecord GoalNode { get; set; }
        public override NodeRecord StartNode { get; set; }

        private int width;
        private int height;

        public AStarPathfinding(int width, int height, float cellSize, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            grid = new Grid<NodeRecord>(width, height, cellSize, (Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y));
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = uint.MaxValue;
            this.width = width;
            this.height = height;
        }
        public override void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            this.StartPositionX = startX;
            this.StartPositionY = startY;
            this.GoalPositionX = goalX;
            this.GoalPositionY = goalY;
            this.StartNode = grid.GetGridObject(StartPositionX, StartPositionY);
            this.GoalNode = grid.GetGridObject(GoalPositionX, GoalPositionY);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord(StartNode.x, StartNode.y, height*StartNode.x + StartNode.y)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode)
            };

            initialNode.CalculateFCost();

            this.Open.Initialize();
            this.Open.AddToOpen(initialNode);
            this.Closed.Initialize();
        }
        public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false) {

            for (int exploredNodes = 0; exploredNodes < MaxExploredNodesPerFrame; ++exploredNodes)
            {

                int numberOfOpenNodes = Open.CountOpen();

                if (numberOfOpenNodes > MaxOpenNodes)
                    MaxOpenNodes = numberOfOpenNodes;

                // Are there no more nodes to search?
                if (numberOfOpenNodes == 0)
                {
                    solution = null;
                    return true;
                }

                // CurrentNode is the best one from the Open set, start with that
                NodeRecord currentNode = Open.GetBestAndRemove();

                // Is this the Goal node?
                if (currentNode.x == GoalNode.x && currentNode.y == GoalNode.y)
                {
                    solution = CalculatePath(currentNode);
                    return true;
                }

                currentNode.status = NodeStatus.Closed;
                Closed.AddToClosed(currentNode);
                grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

                ++TotalProcessedNodes;

                //Handle the neighbours/children with something like this
                foreach (NodeRecord neighbourNode in GetNeighbourList(currentNode))
                {
                    if (neighbourNode.isWalkable)
                    {
                        this.ProcessChildNode(currentNode, neighbourNode);
                    }
                }
            }

            TotalProcessingTime += Time.deltaTime;

            if (returnPartialSolution && TotalProcessedNodes >= MaxExploredNodesPerFrame)
            {
                NodeRecord partialBest = Open.PeekBest();
                solution = CalculatePath(partialBest);
                return true;
            }

            //Out of nodes on the openList
            solution = null;
            return false;
        }
      
        protected virtual void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            //this is where you process a child node 
            NodeRecord child = this.GenerateChildNodeRecord(parentNode, neighbourNode);

            NodeRecord childInOpen = Open.SearchInOpen(child);
            NodeRecord childInClosed = Closed.SearchInClosed(child);

            if (childInOpen == null && childInClosed == null)
            {
                Open.AddToOpen(child);
                child.status = NodeStatus.Open;
                grid.SetGridObject(child.x, child.y, child);
            } else if (childInOpen != null && child.CompareTo(childInOpen) < 0)
            {
                Open.Replace(childInOpen, child);
                child.status = NodeStatus.Open;
                grid.SetGridObject(child.x, child.y, child);
            } else if (childInClosed != null && child.CompareTo(childInClosed) < 0)
            {
                Closed.RemoveFromClosed(childInClosed);
                Open.AddToOpen(child);
                child.status = NodeStatus.Open;
                grid.SetGridObject(child.x, child.y, child);
            }
        }

        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NodeRecord neighbour)
        {
            var childNodeRecord = new NodeRecord(neighbour.x, neighbour.y, height * neighbour.x + neighbour.y)
            {
                parent = parent,
                gCost = parent.gCost + CalculateDistanceCost(parent, neighbour),
                hCost = this.Heuristic.H(neighbour, this.GoalNode)
            };

            childNodeRecord.CalculateFCost();

            return childNodeRecord;
        }

        // Not filtering the path already taken
        //Retrieve all the neighbours possible optimization here
        private List<NodeRecord> GetNeighbourList(NodeRecord currentNode)
        {
            List<NodeRecord> neighbourList = new List<NodeRecord>();

            if(currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
                //Left down
                if(currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                //Left up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.x + 1 < grid.getWidth())
            {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
                //Right down
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                //Right up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            // Down
            if (currentNode.y - 1 >= 0)
                neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
            //Up
            if (currentNode.y + 1 < grid.getHeight())
                neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

            return neighbourList;
        }


        public override NodeRecord GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y);
        }

        private int CalculateDistanceCost(NodeRecord a, NodeRecord b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }


        public override List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);
            NodeRecord currentNode = endNode;
            //Go through the list of nodes from the end to the beggining
            while(currentNode.parent != null)
            {
                path.Add(currentNode.parent);
                currentNode = currentNode.parent;
                
            }
            //the list is reversed
            path.Reverse();
            return path;
        }

    
    }
}
