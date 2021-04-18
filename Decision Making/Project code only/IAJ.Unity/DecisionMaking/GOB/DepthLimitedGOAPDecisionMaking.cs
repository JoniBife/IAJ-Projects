using Assets.Scripts.GameManager;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 4;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        private WorldModel BestWorldModel;
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var processedActionCombinations = 0;

            var startTime = Time.realtimeSinceStartup;
            var currentValue = 0.0f;

            while (processedActionCombinations < ActionCombinationsProcessedPerFrame && CurrentDepth >= 0)
            {
                if (CurrentDepth >= MAX_DEPTH)
                {
                    currentValue = Models[CurrentDepth].CalculateDiscontentment(Goals);

                    if (currentValue < BestDiscontentmentValue)
                    {
                        BestDiscontentmentValue = currentValue;
                        BestActionSequence = ActionPerLevel;
                        BestAction = BestActionSequence[0];
                        BestWorldModel = Models[CurrentDepth];
                    }

                    ++processedActionCombinations;
                    --CurrentDepth;
                    continue;
                }

                Action nextAction = Models[CurrentDepth].GetNextAction();

                if (nextAction != null)
                {
                    Models[CurrentDepth + 1] = Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(Models[CurrentDepth + 1]);
                    ActionPerLevel[CurrentDepth] = nextAction;
                    Models[CurrentDepth + 1].CalculateNextPlayer();
                    ++CurrentDepth;
                }
                else
                    --CurrentDepth;
            }

            this.TotalActionCombinationsProcessed += processedActionCombinations;
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
