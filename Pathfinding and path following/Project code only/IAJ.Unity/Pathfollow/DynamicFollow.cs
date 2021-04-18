using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Transactions;
using Assets.Scripts.IAJ.Unity.Util;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollow : DynamicArrive
    {
        public Path Path { get; set; }
        private float PathOffset;
        private float CurrParam;

        public DynamicFollow(float PathOffset, List<NodeRecord> path, float cellSize)
        {
            this.CurrParam = 0;
            this.PathOffset = PathOffset;
            this.Path = new Path(path, cellSize);
            this.ArriveTarget = new KinematicData(new StaticData((new GameObject()).transform));
        }

        public override MovementOutput GetMovement()
        {
            CurrParam = Path.GetParam(Character.Position, CurrParam);

            float targetParam = CurrParam + PathOffset;

            ArriveTarget.Position = Path.GetPosition(targetParam);

            return base.GetMovement();
        }
    }

    public class Path
    {
        private readonly float cellSize;
        private readonly List<Vector3> pathTargets;
        //private int currIdx = 0;
        public Path(List<NodeRecord> nodeRecords, float cellSize)
        {
            this.pathTargets = new List<Vector3>(nodeRecords.Count);
            //this.nodeRecords = nodeRecords;
            this.cellSize = cellSize;

            foreach(NodeRecord nr in nodeRecords)
            {
                pathTargets.Add(NodeRecordToWorldPosition(nr));
            }
        }

        // Parameter position is not used
        public float GetParam(Vector3 position, float lastParam) {
        
            int idx = (int)lastParam; // truncating to obtaing the idx
            float closestParam = lastParam;
            float closestDistance = float.MaxValue;

            for (int i = idx; i <= idx + 1; ++i)
            {
                if (i >= pathTargets.Count - 1)
                    break;

                Vector3 a = pathTargets[i];
                Vector3 b = pathTargets[i + 1];

                // Vector from b to a
                Vector3 ba = a - b;
                // Vector from a to b
                Vector3 ab = b - a;

                // Vector from b to position (c)
                Vector3 bc = position - b;
                // Vector from a to position (c)
                Vector3 ac = position - a;

                float angle1 = Vector3.Angle(ab, ac);
                float angle2 = Vector3.Angle(ba, bc);

                // Are we close to this line segment?
                if (angle1 < 90.0f && angle2 < 90.0f)
                {
                    Vector3 projection = Vector3.Project(ac, ab);
                    Vector3 closestPoint = a + projection;

                    float distanceToClosestPoint = (closestPoint - position).magnitude;

                    if (distanceToClosestPoint < closestDistance)
                    {
                        closestDistance = distanceToClosestPoint;
                        
                        closestParam = projection.magnitude / ab.magnitude + i;
                    }
                }
            }
            return closestParam;
        }

        public Vector3 GetPosition(float param)
        {
            int idx = (int)param;

            return (idx > pathTargets.Count - 1) ?
                pathTargets[pathTargets.Count - 1] :
                pathTargets[idx];

        }

        private Vector3 NodeRecordToWorldPosition(NodeRecord nr)
        {
            Vector3 target = new Vector3(nr.x, 0, nr.y) * cellSize;
            target.x += cellSize / 2;
            target.z += cellSize / 2;
            return target;
        }

        private bool IsWithinTargetRadius(Vector3 position, Vector3 target)
        {
            Vector3 direction = target - position;

            return direction.magnitude < cellSize/2;
        }

        private float DistanceToTarget(Vector3 position, Vector3 target)
        {
            Vector3 direction = target - position;

            return direction.magnitude;
        }
    }
}