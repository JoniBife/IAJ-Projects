using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public interface IMCTSHeuristic 
    {
        float GetHeuristic(Action action);
    }
}