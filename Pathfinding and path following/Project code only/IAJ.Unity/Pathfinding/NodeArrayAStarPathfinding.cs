using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;
namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathfinding : AStarPathfinding
    {
        public static NodeArrayAStarPathfinding Create(int width, int height, float cellSize, IHeuristic heuristic)
        {
            List<NodeRecord> nodeRecords = new List<NodeRecord>(width*height);

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    nodeRecords.Add(new NodeRecord(i, j, height*i + j));
                }
            }

            return new NodeArrayAStarPathfinding(width, height, cellSize, new NodeRecordArray(nodeRecords), heuristic);
        }

        private NodeArrayAStarPathfinding(int width, int height, float cellSize, NodeRecordArray nodeArray, IHeuristic heuristic)
            : base(width, height, cellSize, nodeArray, nodeArray, heuristic) { }
    }
}