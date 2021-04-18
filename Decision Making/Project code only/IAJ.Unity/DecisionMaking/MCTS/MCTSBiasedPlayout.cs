using UnityEngine;
using UnityEditor;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel) {

        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            int n_iterations = 1;
            if (StochasticWorld)
                n_iterations = 10;

            float reward = 0;
            int i = 0;
            while (n_iterations > i)
            {
                // The playout has to be run on a copied world, not directly in the initialPlayoutState
                WorldModel copyState = initialPlayoutState.GenerateChildWorldModel();

                List<Action> executableActions = copyState.GetExecutableActions();

                int currPlayoutDepth = 0;

                while (true)
                {
                    Action heuristicAction = GetActionUsingHeuristicGIBBS(executableActions,copyState);
                    heuristicAction.ApplyActionEffects(copyState);

                    if (copyState.IsTerminal() || (MCTSLimitedPlayout && currPlayoutDepth == MAX_PLAYOUT_DEPTH))
                        break;

                    copyState.CalculateNextPlayer();

                    if (copyState.IsTerminal() || (MCTSLimitedPlayout && currPlayoutDepth == MAX_PLAYOUT_DEPTH))
                        break;

                    executableActions = copyState.GetExecutableActions();

                    if (currPlayoutDepth > MaxPlayoutDepthReached)
                        MaxPlayoutDepthReached = currPlayoutDepth;
                }

                if (copyState.IsTerminal())
                    reward += copyState.GetScore();
                else if (MCTSLimitedPlayout)
                    reward += GetHeuristicScore(copyState);
                i++;
            }
            reward /= n_iterations;

            return new Reward
            {
                PlayerID = 0,

                Value = reward
            };
        }

        private static System.Random random = new System.Random();
        private Action GetActionUsingHeuristicGIBBS(List<Action> executableActions, WorldModel copystate)
        {
            double sum = 0;
            double heuristic = 0;
            
            foreach (Action action in executableActions)
            {
                float hValue = action.GetHValue(copystate);
                heuristic = Math.Pow(Math.E, -hValue);
                sum += heuristic;
            }

            double prob = random.NextDouble();
            double count = 0;
            double best = 0;
            Action bestA = null;
            foreach (Action action in executableActions)
            {
                heuristic = Math.Pow(Math.E, -action.GetHValue(copystate)) / sum;
                count += heuristic;
                if (count >= prob)
                    return action;
            }
            return executableActions[random.Next(0, executableActions.Count)];
        }

        private Action GetActionUsingHeuristicSOFTMAX(List<Action> executableActions, WorldModel copystate)
        {
            double sum = 0;
            double heuristic = 0;

            foreach (Action action in executableActions)
            {
                float hValue = action.GetHValue(copystate);
                heuristic = Math.Pow(Math.E, hValue);
                sum += heuristic;
            }

            double prob = random.NextDouble();
            double count = 0;
            double best = 0;
            Action bestA = null;
            foreach (Action action in executableActions)
            {
                heuristic = Math.Pow(Math.E, action.GetHValue(copystate)) / sum;
                count += heuristic;
                if (count >= prob)
                    return action;

            }
            return executableActions[random.Next(0, executableActions.Count)];
        }

    }
}

