using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(NodeRecord node, NodeRecord goalNode)
        {


            // • h(n) = Sqrt((goal.x – n.x)            2 + (goal.y – n.y)



            var xValue = (goalNode.x - node.x) * (goalNode.x - node.x);

            var yValue = (goalNode.y - node.y) * (goalNode.y - node.y);

            var h = Mathf.Sqrt(xValue + yValue);

            return h;
        }
    }
}
