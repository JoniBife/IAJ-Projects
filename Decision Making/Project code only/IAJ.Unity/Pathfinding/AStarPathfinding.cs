using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding
    {
        protected const int MOVE_STRAIGHT_COST = 10;
        protected const int MOVE_DIAGONAL_COST = 14;
        public Grid<NodeRecord> grid { get; set; }
        public uint NodesPerSearch { get; set; }
        public uint TotalProcessedNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; set; }
        public bool InProgress { get; set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        //heuristic function
        public IHeuristic Heuristic { get; protected set; }

        public int StartPositionX { get; set; }
        public int StartPositionY { get; set; }
        public int GoalPositionX { get; set; }
        public int GoalPositionY { get; set; }
        public NodeRecord GoalNode { get; set; }
        public NodeRecord StartNode { get; set; }

        public AStarPathfinding(int width, int height, float cellSize, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            grid = new Grid<NodeRecord>(width, height, cellSize, (Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y));
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = 50; //by default we process all nodes in a single request

        }
        public virtual void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
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
            var initialNode = new NodeRecord(StartNode.x, StartNode.y)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode)
            };

            initialNode.CalculateFCost();

            this.Open.Initialize();
            this.Closed.Initialize();
            this.Open.AddToOpen(initialNode);
           
        }
        public virtual bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false)
        {

            var processedNodes = 0;
            int count = 0;
            while (processedNodes < this.NodesPerSearch)
            {
                count = Open.CountOpen();
                if (count == 0)
                {
                    solution = null;
                    this.InProgress = false;
                    return false;
                }

                if (count > this.MaxOpenNodes)
                {
                    this.MaxOpenNodes = count;
                }

                NodeRecord currentNode = Open.GetBestAndRemove();
                TotalProcessedNodes += 1;
                if (currentNode.x == GoalNode.x && currentNode.y == GoalNode.y)
                {
                    // Reached final node
                    grid.SetGridObject(currentNode.x, currentNode.y, currentNode);
                    this.InProgress = false;

                    solution = CalculatePath(currentNode);

                    return true;
                }

                Closed.AddToClosed(currentNode);
                processedNodes += 1;
                this.TotalProcessedNodes++;
                currentNode.status = NodeStatus.Closed;
                grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

                foreach (var neighbourNode in GetNeighbourList(currentNode))
                {

                    if (!neighbourNode.isWalkable)
                    {
                        continue;
                    }
                  
                 ProcessChildNode(currentNode, neighbourNode);

                }
            }
            //this is very unlikely but it might happen that we process all nodes alowed in this cycle but there are no more nodes to process
            if (this.Open.CountOpen() == 0)
            {
                solution = null;
                this.InProgress = false;
                return true;
            }

            //if the caller wants create a partial Path to reach the current best node so far
            if (returnPartialSolution)
            {
                var bestNodeSoFar = this.Open.PeekBest();
                solution = this.CalculatePath(bestNodeSoFar);
            }
            else
            {
                solution = null;
            }
            return false;


        }

        protected int CalculateDistanceCost(NodeRecord a, NodeRecord b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        protected virtual void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            // Check if we found a better path than the one we had before

            var childNode = GenerateChildNodeRecord(parentNode, neighbourNode);
            var closedSearch = this.Closed.SearchInClosed(childNode);
            if (closedSearch != null)
            {
                if (childNode.fCost <= closedSearch.fCost)
                {
                    childNode.status = NodeStatus.Open;
                    this.Closed.RemoveFromClosed(closedSearch);
                    grid.SetGridObject(childNode.x, childNode.y, childNode);
                    this.Open.AddToOpen(childNode);

                }

                return;
            }

            var openSearch = this.Open.SearchInOpen(childNode);
            if (openSearch != null)
            {
                if (childNode.fCost <= openSearch.fCost)
                {
                    this.Open.Replace(openSearch, childNode);
                    grid.SetGridObject(neighbourNode.x, neighbourNode.y, childNode);
                }
            }
            else
            {
                this.Open.AddToOpen(childNode);
                childNode.status = NodeStatus.Open;
                grid.SetGridObject(neighbourNode.x, neighbourNode.y, childNode);
            }

        }


        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NodeRecord neighbour)
        {
            var childNodeRecord = new NodeRecord(neighbour.x, neighbour.y)
            {
                parent = parent,
                gCost = parent.gCost + CalculateDistanceCost(parent, neighbour),
                hCost = 10 * this.Heuristic.H(neighbour, this.GoalNode)
            };

            childNodeRecord.CalculateFCost();

            return childNodeRecord;
        }
        private List<NodeRecord> GetNeighbourList(NodeRecord currentNode)
        {
            List<NodeRecord> neighbourList = new List<NodeRecord>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
                //Left down
                if (currentNode.y - 1 >= 0)
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


        public NodeRecord GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y);
        }



        public List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);
            NodeRecord currentNode = endNode;
            while (currentNode.parent != null)
            {
                path.Add(currentNode.parent);
                currentNode = currentNode.parent;

            }
            path.Reverse();
            return path;
        }


    }
}
