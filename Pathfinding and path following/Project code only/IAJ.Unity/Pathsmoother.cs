using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Collections.Generic;
using Assets.Scripts.Grid;
using Unity.Collections;
using System;

namespace Assets.Scripts.IAJ.Unity
{
    public static class Pathsmoother
    {
        public static List<NodeRecord> Smooth(this List<NodeRecord> path, Grid<NodeRecord> grid)
        {
            for(int i = 0; i < path.Count; ++i)
            {
                int next1I = i + 1;
                int next2I = i + 2;

                if (next1I > path.Count - 1 || next2I > path.Count - 1)
                    break;

                NodeRecord curr = path[i];
                NodeRecord next1 = path[next1I];
                NodeRecord next2 = path[next2I];

                while (!ContainsObstacleInBetween(curr, next2, grid))
                {
                    path.Remove(next1);

                    // Since we removed an element this condition might fail
                    if (next1I > path.Count - 1 || next2I > path.Count - 1)
                        break;

                    next1 = path[next1I];
                    next2 = path[next2I];
                }

            }
            return SmoothCurves(path, grid);
        }

        public static bool ContainsObstacleInBetween(NodeRecord curr, NodeRecord next2, Grid<NodeRecord> grid)
        {
            int diffX = next2.x - curr.x;
            int diffY = next2.y - curr.y;

            int signX = (int)Mathf.Sign(diffX);
            int signY = (int)Mathf.Sign(diffY);

            for (int x = 0; x <= Mathf.Abs(diffX); ++x)
            {
                for(int y = 0; y <= Mathf.Abs(diffY); ++y)
                {
                    NodeRecord nr = grid.GetGridObject(curr.x + signX*x, curr.y + signY * y);
                    if (!nr.isWalkable)
                        return true;
                }
            }

            return false;
        }

        public static List<NodeRecord> SmoothCurves(List<NodeRecord> path, Grid<NodeRecord> grid)
        {
            List<Tuple<int, NodeRecord>> toAdd = new List<Tuple<int, NodeRecord>>();
            List<NodeRecord> toRemove = new List<NodeRecord>();

            for (int i = 0; i < path.Count; ++i)
            {
                int nextI = i + 1;

                if (nextI > path.Count - 1)
                    break;

                NodeRecord curr = path[i];
                NodeRecord next = path[nextI];

                int diffX = next.x - curr.x;
                int diffY = next.y - curr.y;

                int signX = (int)Mathf.Sign(diffX);
                int signY = (int)Mathf.Sign(diffY);

                // Is it a diagonal?
                if (Mathf.Abs(diffX) == 1 && Mathf.Abs(diffY) == 1)
                {
                    NodeRecord nextX = grid.GetGridObject(curr.x + signX, curr.y);

                    if (nextX.isWalkable) {
                        toAdd.Add(new Tuple<int, NodeRecord>(i, nextX));
                        toRemove.Add(next);
                        continue;
                     }

                    NodeRecord nextY = grid.GetGridObject(curr.x, curr.y + signY);
                    if (nextY.isWalkable)
                    {
                        toRemove.Add(next);
                        toAdd.Add(new Tuple<int, NodeRecord>(i, nextY));
                    }
                }
            }

            for (int i = 0; i < toAdd.Count; ++i)
            {
                Tuple<int, NodeRecord> curr = toAdd[i];
                path[curr.Item1] = curr.Item2;
            }

            for (int i = 0; i < toRemove.Count; ++i)
            {
                path.Remove(toRemove[i]);
            }

            return path;
        }
    }
}