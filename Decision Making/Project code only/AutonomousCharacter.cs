using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.IAJ.Unity.DecisionMaking;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;

namespace Assets.Scripts
{
    public class AutonomousCharacter : MonoBehaviour
    {
        //constants
        public const string SURVIVE_GOAL = "Survive";
        public const string GAIN_LEVEL_GOAL = "GainXP";
        public const string BE_QUICK_GOAL = "BeQuick";
        public const string GET_RICH_GOAL = "GetRich";

        public const float DECISION_MAKING_INTERVAL = 20.0f; // Previously 20
        public const float RESTING_INTERVAL = 5.0f;
        public const int REST_HP_RECOVERY = 2;


        //public fields to be set in Unity Editor
        public GameManager.GameManager GameManager;
        public Text SurviveGoalText;
        public Text GainXPGoalText;
        public Text BeQuickGoalText;
        public Text GetRichGoalText;
        public Text DiscontentmentText;
        public Text TotalProcessingTimeText;
        public Text BestDiscontentmentText;
        public Text ProcessedActionsText;
        public Text BestActionText;
        public Text BestActionDebugText;
        public Text DiaryText;
        public bool MCTSActive;
        public bool Resting = false;
        public float StopRestTime;

        public Goal BeQuickGoal { get; private set; }
        public Goal SurviveGoal { get; private set; }
        public Goal GetRichGoal { get; private set; }
        public Goal GainLevelGoal { get; private set; }
        public List<Goal> Goals { get; set; }
        public List<Action> Actions { get; set; }
        public Action CurrentAction { get; private set; }
        public DynamicCharacter Character { get; private set; }
        public DepthLimitedGOAPDecisionMaking GOAPDecisionMaking { get; set; }
        public MCTS MCTSDecisionMaking { get; set; }

        public bool StochasticWorld;
        public bool MCTSBiased;
        public bool MCTSLimitedPlayout;
        public AStarPathfinding AStarPathFinding;

        //private fields for internal use only
        private Vector3 startPosition;
        private GlobalPath currentSolution;
        private GlobalPath currentSmoothedSolution;
    
        
        private bool draw;
        private float nextUpdateTime = 0.0f;
        private float previousGold = 0.0f;
        private int previousLevel = 1;
        private Vector3 previousTarget;

		private Animator characterAnimator;
        public bool lookingForPath;
        PathfindingManager pathfindingManager;


        public void Initialize()
        {
            this.draw = true;

			this.characterAnimator = this.GetComponentInChildren<Animator>();
        }

        public void Start()
        {
            this.draw = true;

            this.GameManager = GameObject.Find("Manager").GetComponent<GameManager.GameManager>();
            this.pathfindingManager = GameObject.Find("Manager").GetComponent<PathfindingManager>();

            this.Character = new DynamicCharacter(this.gameObject);

            this.Initialize();

            // initializing debug tools
            this.BeQuickGoalText = GameObject.Find("BeQuickGoal").GetComponent<Text>();
            this.SurviveGoalText = GameObject.Find("SurviveGoal").GetComponent<Text>();
            this.GainXPGoalText = GameObject.Find("GainXP").GetComponent<Text>();
            this.GetRichGoalText = GameObject.Find("GetRichGoal").GetComponent<Text>();
            this.DiscontentmentText = GameObject.Find("Discontentment").GetComponent<Text>();
            this.TotalProcessingTimeText = GameObject.Find("ProcessTime").GetComponent<Text>();
            this.BestDiscontentmentText = GameObject.Find("BestDicont").GetComponent<Text>();
            this.ProcessedActionsText = GameObject.Find("ProcComb").GetComponent<Text>();
            this.BestActionText = GameObject.Find("BestAction").GetComponent<Text>();
            this.BestActionDebugText = GameObject.Find("BestActionDebug").GetComponent<Text>();
            this.DiaryText = GameObject.Find("DiaryText").GetComponent<Text>();


            this.characterAnimator = this.GetComponentInChildren<Animator>();
            //initialization of the GOB decision making
            //let's start by creating 4 main goals

            this.SurviveGoal = new Goal(SURVIVE_GOAL, 2.0f);

            this.GainLevelGoal = new Goal(GAIN_LEVEL_GOAL, 5.0f)
            {
                ChangeRate = 0.1f
            };

            this.GetRichGoal = new Goal(GET_RICH_GOAL, 1.0f)
            {
                InsistenceValue = 5.0f,
                ChangeRate = 0.2f
            };

            this.BeQuickGoal = new Goal(BE_QUICK_GOAL, 1.0f)
            {
                ChangeRate = 0.1f
            };

            this.Goals = new List<Goal>();
            this.Goals.Add(this.SurviveGoal);
            this.Goals.Add(this.BeQuickGoal);
            this.Goals.Add(this.GetRichGoal);
            this.Goals.Add(this.GainLevelGoal);

            //initialize the available actions
            //Uncomment commented actions after you implement them

            this.Actions = new List<Action>();

            this.Actions.Add(new ShieldOfFaith(this));
            this.Actions.Add(new Rest(this));
            this.Actions.Add(new Teleport(this));
            this.Actions.Add(new LevelUp(this));

            foreach (var chest in GameObject.FindGameObjectsWithTag("Chest"))
            {
                this.Actions.Add(new PickUpChest(this, chest));
            }

            foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
            {
                this.Actions.Add(new GetManaPotion(this, potion));
            }

            foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
            {
                this.Actions.Add(new GetHealthPotion(this, potion));
            }

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Skeleton"))
            {
                this.Actions.Add(new SwordAttack(this, enemy));
                this.Actions.Add(new DivineSmite(this, enemy));
            }

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Orc"))
            {
                this.Actions.Add(new SwordAttack(this, enemy));
            }

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Dragon"))
            {
                this.Actions.Add(new SwordAttack(this, enemy));
            }


            var worldModel = new CurrentStateWorldModel(this.GameManager, this.Actions, this.Goals);
            this.GOAPDecisionMaking = new DepthLimitedGOAPDecisionMaking(worldModel,this.Actions,this.Goals);
            if (MCTSBiased) 
                this.MCTSDecisionMaking = new MCTSBiasedPlayout(worldModel);
            else
                this.MCTSDecisionMaking = new MCTS(worldModel); 
            
            this.MCTSDecisionMaking.StochasticWorld = StochasticWorld;
            this.MCTSDecisionMaking.MCTSLimitedPlayout = MCTSLimitedPlayout;

            this.Resting = false;

            DiaryText.text += "My Diary \n I awoke. What a wonderful day to kill Monsters! \n";
        }

        void Update()
        {
            if (GameManager.gameEnded) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                lookingForPath = true;
            }

            if (Time.time > this.nextUpdateTime || this.GameManager.WorldChanged)
            {

                this.GameManager.WorldChanged = false;
                this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;

                //first step, perceptions
                //update the agent's goals based on the state of the world

                this.SurviveGoal.InsistenceValue = this.GameManager.characterData.MaxHP - this.GameManager.characterData.HP;

                this.BeQuickGoal.InsistenceValue += DECISION_MAKING_INTERVAL * this.BeQuickGoal.ChangeRate;
                if(this.BeQuickGoal.InsistenceValue > 10.0f)
                {
                    this.BeQuickGoal.InsistenceValue = 10.0f;
                }

                this.GainLevelGoal.InsistenceValue += this.GainLevelGoal.ChangeRate; //increase in goal over time
                if(this.GameManager.characterData.Level > this.previousLevel)
                {
                    this.GainLevelGoal.InsistenceValue -= this.GameManager.characterData.Level - this.previousLevel;
                    this.previousLevel = this.GameManager.characterData.Level;
                }

                this.GetRichGoal.InsistenceValue += this.GetRichGoal.ChangeRate; //increase in goal over time
                if (this.GetRichGoal.InsistenceValue > 10)
                {
                    this.GetRichGoal.InsistenceValue = 10.0f;
                }

                if (this.GameManager.characterData.Money > this.previousGold)
                {
                    this.GetRichGoal.InsistenceValue -= this.GameManager.characterData.Money - this.previousGold;
                    this.previousGold = this.GameManager.characterData.Money;
                }


                this.SurviveGoalText.text = "Survive: " + this.SurviveGoal.InsistenceValue;
                this.GainXPGoalText.text = "Gain Level: " + this.GainLevelGoal.InsistenceValue.ToString("F1");
                this.BeQuickGoalText.text = "Be Quick: " + this.BeQuickGoal.InsistenceValue.ToString("F1");
                this.GetRichGoalText.text = "GetRich: " + this.GetRichGoal.InsistenceValue.ToString("F1");
                this.DiscontentmentText.text = "Discontentment: " + this.CalculateDiscontentment().ToString("F1");

                //initialize Decision Making Proccess
                lookingForPath = false;
                this.CurrentAction = null;
                if (this.MCTSActive)
                {
                    this.MCTSDecisionMaking.InitializeMCTSearch();
                }
                else
                {
                    this.GOAPDecisionMaking.InitializeDecisionMakingProcess();
                }
            }

            if (this.MCTSActive)
            {
                this.UpdateMCTS();
            }
            else
            {
                this.UpdateDLGOAP();
            }

            if (this.CurrentAction != null && !lookingForPath)
            {
                if (this.CurrentAction.CanExecute())
                {
                  
                    this.CurrentAction.Execute();
                }
                // Sometimes the Character wants to perfom an action whose target is empty, that cannot happen
                else if (this.CurrentAction is WalkToTargetAndExecuteAction) {
                    var act = (WalkToTargetAndExecuteAction)this.CurrentAction;
                    if (!act.Target)
                    {
                        GameManager.WorldChanged = true;
                        return;
                    }
                }
            }

            //call the pathfinding method if the user specified a new goal

            if (lookingForPath) { 
                var finished = pathfindingManager.finished;
                
                if (finished)
                {
                    this.currentSolution = new GlobalPath();
                    this.currentSmoothedSolution = new GlobalPath();
                   // pathfindingManager.ClearGrid();
                    this.currentSolution = pathfindingManager.CalculateSolution(pathfindingManager.characterSolution);
                    pathfindingManager.finished = false;
                    
                    //lets smooth out the Path
                    this.startPosition = this.Character.KinematicData.position;
                    if (this.currentSolution != null)
                    {

                        // The smoothing algorithm is in the PathfindManager
                        this.currentSmoothedSolution = pathfindingManager.smoothPath(startPosition, this.currentSolution);
                        this.currentSmoothedSolution.CalculateLocalPathsFromPathPositions(this.Character.KinematicData.position);
                        lookingForPath = false;
                        this.Character.Movement = new DynamicFollowPath(this.Character.KinematicData, this.currentSmoothedSolution)
                        {
                            MaxAcceleration = 200.0f,
                            MaxSpeed = 40.0f
                        };
                    }
                }
                
            }

            this.Character.Update();

			//manage the character's animation
			if (this.Character.KinematicData.velocity.sqrMagnitude > 0.1) 
			{
				this.characterAnimator.SetBool ("Walking", true);
			} 
			else 
			{
				this.characterAnimator.SetBool ("Walking", false);
			}
        }


        private void UpdateMCTS()
        {
            if (this.MCTSDecisionMaking.InProgress)
            {
                var action = this.MCTSDecisionMaking.Run();
                if (action != null)
                {
                    this.CurrentAction = action;
                    this.DiaryText.text += Time.time + " I decided to " + action.Name + "\n";
                }
            }

            this.TotalProcessingTimeText.text = "Process. Time: " + this.MCTSDecisionMaking.TotalProcessingTime.ToString("F");

            this.ProcessedActionsText.text = "Max Depth: " + this.MCTSDecisionMaking.MaxPlayoutDepthReached.ToString();

            if (this.MCTSDecisionMaking.BestFirstChild != null)
            {
                var q = this.MCTSDecisionMaking.BestFirstChild.Q / this.MCTSDecisionMaking.BestFirstChild.N;
                this.BestDiscontentmentText.text = "Best Exp. Q value: " + q.ToString("F05");
                var actionText = "";
                foreach (var action in this.MCTSDecisionMaking.BestActionSequence)
                {
                    actionText += "\n" + action.Name;
                }
                this.BestActionText.text = "Best Action Sequence: " + actionText;

                //Debug: What is the ptedicted state of the world?
                var endState = MCTSDecisionMaking.BestActionSequenceWorldState;

                var text = "";
                text += "Predicted World State:\n";
                text += "My Level:" + endState.GetProperty(PropertiesId.LEVEL) + "\n";
                text += "My HP:" + endState.GetProperty(PropertiesId.HP) + "\n";
                text += "My Money:" + endState.GetProperty(PropertiesId.MONEY) + "\n";
                text += "Time Passsed:" + endState.GetProperty(PropertiesId.TIME) + "\n";
                this.BestActionDebugText.text = text;
            }
            else
            {
                this.BestActionText.text = "Best Action Sequence:\nNone";
                this.BestActionDebugText.text = "";
            }
        }

        private void UpdateDLGOAP()
        {
            bool newDecision = false;
            if (this.GOAPDecisionMaking.InProgress)
            {
                //choose an action using the GOB Decision Making process
                var action = this.GOAPDecisionMaking.ChooseAction();
                if (action != null && action != this.CurrentAction)
                {
                    this.CurrentAction = action;
                    newDecision = true;
                }
            }

            this.TotalProcessingTimeText.text = "Process. Time: " + this.GOAPDecisionMaking.TotalProcessingTime.ToString("F");
            this.BestDiscontentmentText.text = "Best Discontentment: " + this.GOAPDecisionMaking.BestDiscontentmentValue.ToString("F");
            this.ProcessedActionsText.text = "Act. comb. processed: " + this.GOAPDecisionMaking.TotalActionCombinationsProcessed;

            if (this.GOAPDecisionMaking.BestAction != null)
            {
                if (newDecision)
                {
                  DiaryText.text += Time.time + " I decided to " + GOAPDecisionMaking.BestAction.Name + "\n";
                }
                var actionText = "";
                foreach (var action in this.GOAPDecisionMaking.BestActionSequence)
                {
                    actionText += "\n" + action.Name;
                }
                this.BestActionText.text = "Best Action Sequence: " + actionText;
                this.BestActionDebugText.text = "Best Action: " + GOAPDecisionMaking.BestAction.Name;
            }
            else
            {
                this.BestActionText.text = "Best Action Sequence:\nNone";
                this.BestActionDebugText.text = "Best Action: \n Node";
            }
        }

        public void StartPathfinding(Vector3 targetPosition)
        {

            //if the targetPosition received is the same as a previous target, then this a request for the same target
            //no need to redo the pathfinding search
            if(!this.previousTarget.Equals(targetPosition))
            {
                lookingForPath = true;
                this.pathfindingManager.InitializeSearch(this.Character.KinematicData.position, targetPosition, "Character");
                this.previousTarget = targetPosition;
            }
        }

        public float CalculateDiscontentment()
        {
            var discontentment = 0.0f;

            foreach (var goal in this.Goals)
            {
                discontentment += goal.GetDiscontentment();
            }
            return discontentment;
        }

        public void UpdateMCTSCurrWorldModel()
        {
            if (MCTSActive)
            {

                int i = 0;
                MCTSDecisionMaking.CurrentStateWorldModel._props[i] = GameManager.characterData.Mana;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.HP;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.ShieldHP;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.MaxHP;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.XP;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.Time;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.Money;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.characterData.Level;
                MCTSDecisionMaking.CurrentStateWorldModel._props[++i] = GameManager.initialPosition;

                for (; i < MCTSDecisionMaking.CurrentStateWorldModel._props.Length; ++i)
                {
                    MCTSDecisionMaking.CurrentStateWorldModel._props[i] = 
                        MCTSDecisionMaking.CurrentStateWorldModel.GetProperty(i);
                }
            }
        }

      
    }
}
