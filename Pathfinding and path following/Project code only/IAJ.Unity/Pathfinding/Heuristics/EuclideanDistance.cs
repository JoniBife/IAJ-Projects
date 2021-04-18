using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(NodeRecord node, NodeRecord goalNode)
        {
            return 11 * Mathf.Sqrt((goalNode.x - node.x)*(goalNode.x - node.x) + (goalNode.y - node.y)*(goalNode.y - node.y));
        }
    }
}
