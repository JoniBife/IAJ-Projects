using UnityEngine;
using UnityEditor;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public abstract class Pathfinding
    {
        public abstract Grid<NodeRecord> grid { get; set; }
        public abstract uint NodesPerSearch { get; set; }
        public abstract uint TotalProcessedNodes { get; protected set; }
        public abstract int MaxOpenNodes { get; protected set; }
        public abstract float TotalProcessingTime { get; set; }
        public abstract uint MaxExploredNodesPerFrame { get; set; }
        public abstract bool InProgress { get; set; }
        public abstract IOpenSet Open { get; protected set; }
        public abstract IClosedSet Closed { get; protected set; }

        public abstract IHeuristic Heuristic { get; protected set; }

        public abstract int StartPositionX { get; set; }
        public abstract int StartPositionY { get; set; }
        public abstract int GoalPositionX { get; set; }
        public abstract int GoalPositionY { get; set; }
        public abstract NodeRecord GoalNode { get; set; }
        public abstract NodeRecord StartNode { get; set; }

        public abstract void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY);

        public abstract bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false);

        public abstract NodeRecord GetNode(int x, int y);

        public abstract List<NodeRecord> CalculatePath(NodeRecord endNode);
    }
}