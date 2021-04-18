using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class OctileHeuristic : IHeuristic
    {
        public float H(NodeRecord node, NodeRecord goalNode)
        {
            float x = Mathf.Abs(goalNode.x - node.x);
            float y = Mathf.Abs(goalNode.y - node.y);
            return (10 * (x+y) + (14-20)*Mathf.Min(x,y));
        }
    }
}
