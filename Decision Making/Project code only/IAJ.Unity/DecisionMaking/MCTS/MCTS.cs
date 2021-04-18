using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public const int MAX_PLAYOUT_DEPTH = 3;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public WorldModel BestActionSequenceWorldState { get; private set; }
        protected int CurrentIterations { get; set; }
        protected int CurrentIterationsInFrame { get; set; }
        protected int CurrentDepth { get; set; }
        public CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        public bool StochasticWorld { get; set; } = false;
        public bool MCTSLimitedPlayout { get; set; } = false;


        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 500;
            this.MaxIterationsProcessedPerFrame = 20;
            this.CurrentIterations = 0;
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public Action Run()
        {
            MCTSNode selectedNode;
            Reward reward;

            var startTime = Time.realtimeSinceStartup;

            this.CurrentIterationsInFrame = 0;

            if (CurrentIterations < MaxIterations)
            {
                while (CurrentIterationsInFrame < MaxIterationsProcessedPerFrame)
                {
                    selectedNode = Selection(InitialNode);
                    reward = Playout(selectedNode.State);
                    Backpropagate(selectedNode, reward);
                    ++CurrentIterationsInFrame;
                    ++CurrentIterations;
                }
            } else
            {
                TotalProcessingTime += Time.realtimeSinceStartup - startTime;

                InProgress = false;
                return BestFinalAction(InitialNode);
            }

            TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            return null;
        }

        // Selection and Expansion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild; 

            while(!currentNode.State.IsTerminal())
            {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null)
                {
                    ++CurrentDepth;
                    if (CurrentDepth > MaxSelectionDepthReached)
                        MaxSelectionDepthReached = CurrentDepth;
                    currentNode = Expand(currentNode, nextAction);
                    break;
                } else {
                    bestChild = BestUCTChild(currentNode);
                    if (bestChild != null)
                    {
                        ++CurrentDepth;
                        currentNode = bestChild;
                    }
                }

            }
            return currentNode;
        }

        protected virtual Reward Playout(WorldModel initialPlayoutState)
        {
            int n_iterations = 1;
            if (StochasticWorld)
                n_iterations = 7 - CurrentDepth; // As we progress in the tree we reduce the number of iterations

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
                    Action randomAction = executableActions[RandomGenerator.Next(0, executableActions.Count)];
                    randomAction.ApplyActionEffects(copyState);

                    if (copyState.IsTerminal() || (MCTSLimitedPlayout && currPlayoutDepth == MAX_PLAYOUT_DEPTH))
                        break;

                    copyState.CalculateNextPlayer();

                    if (copyState.IsTerminal() || (MCTSLimitedPlayout && currPlayoutDepth == MAX_PLAYOUT_DEPTH))
                        break;

                    // No need to get the Executable actions again when the current playout has ended
                    executableActions = copyState.GetExecutableActions();
                    ++currPlayoutDepth;

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

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
        {
           while(node != null)
            {
                node.N += 1;
                node.Q += reward.GetRewardForNode(node);
                node = node.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            WorldModel newState = parent.State.GenerateChildWorldModel();
            // The generated child has to have the effects of its action
            action.ApplyActionEffects(newState);
            newState.CalculateNextPlayer();
            MCTSNode child = new MCTSNode(newState)
            {
                Action = action,
                Parent = parent,
                PlayerID = newState.GetNextPlayer()
            };
            parent.ChildNodes.Add(child);
            return child;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            float bestUi = -1.0f; // -1 so that a node with 0 can be selected
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                // This formula was taken from the book
                float currUi = child.Q / child.N + C * Mathf.Sqrt(Mathf.Log(node.N) / child.N);
                if (currUi > bestUi)
                {
                    bestUi = currUi;
                    bestChild = child;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            float bestUi = -1.0f; // -1 so that a node with zero can be selected
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                float currUi = child.Q / child.N;
                if (currUi > bestUi)
                {
                    bestUi = currUi;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        
        protected Action BestFinalAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while(!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;
                this.BestActionSequenceWorldState = node.State;
            }

            return this.BestFirstChild.Action;
        }

        protected float GetHeuristicScore(WorldModel worldModel)
        {
            float hp = ((int)worldModel.GetProperty(PropertiesId.HP) + (int)worldModel.GetProperty(PropertiesId.ShieldHP))/35;
            float money = (int)worldModel.GetProperty(PropertiesId.MONEY) / 25;
            float time = (float)worldModel.GetProperty(PropertiesId.TIME);
            float mana = (int)worldModel.GetProperty(PropertiesId.MANA) / 10;

            float heuristic = 0.4f * hp + 0.4f * money + 0.1f * 1.0f/(time) + 0.1f * mana;
            return heuristic;
        }

    }
}
