using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class LevelUp : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public LevelUp(AutonomousCharacter character) : base("LevelUp")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            var level = this.Character.GameManager.characterData.Level;
            var xp = this.Character.GameManager.characterData.XP;

            return xp >= level * 10;
        }
        

        public override bool CanExecute(WorldModel worldModel)
        {
            int xp = (int)worldModel.GetProperty(PropertiesId.XP);
            int level = (int)worldModel.GetProperty(PropertiesId.LEVEL);

            return xp >= level * 10;
        }

        public override void Execute()
        {
            this.Character.GameManager.LevelUp();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(PropertiesId.MAXHP);
            int level = (int)worldModel.GetProperty(PropertiesId.LEVEL);

            worldModel.SetProperty(PropertiesId.LEVEL, level + 1);
            worldModel.SetProperty(PropertiesId.MAXHP, maxHP + 10);
            worldModel.SetProperty(PropertiesId.XP, (int)0);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, 0);
        }

        public float GetGoalChange(Goal goal)
        {
            float change = 0.0f;

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change = -goal.InsistenceValue;
            }

            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //you would be dumb not to level up if possible
            return -100;
        }
    }
}
