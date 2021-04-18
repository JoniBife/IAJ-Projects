using UnityEngine;
using UnityEditor;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Rest : Action
    {

        private const float DURATION = 5.0f;
        private const int HEALTH_GAIN = 2;
        private AutonomousCharacter character;

        public Rest(AutonomousCharacter character) : base("Rest")
        {
            this.character = character;
        }
        public override float GetDuration(WorldModel worldModel)
        {
            return DURATION;
        }

        public override float GetDuration()
        {
            return DURATION;
        }


        public override bool CanExecute(WorldModel woldModel)
        {
            return (character.GameManager.enemies.Count > 0 && (int)woldModel.GetProperty(PropertiesId.HP) < (int)woldModel.GetProperty(PropertiesId.MAXHP));
        }

        public override bool CanExecute()
        {
            return (character.GameManager.enemies.Count > 0 && character.GameManager.characterData.HP < character.GameManager.characterData.MaxHP);
        }

        public override void Execute()
        {
            base.Execute();
            this.character.GameManager.Rest();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int currHp = (int)worldModel.GetProperty(PropertiesId.HP);
            int maxHp = (int)worldModel.GetProperty(PropertiesId.MAXHP);

            int newHp = currHp + HEALTH_GAIN;

            if (newHp > maxHp)
                newHp = maxHp;

            worldModel.SetProperty(PropertiesId.HP, newHp);

            var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + DURATION * 0.1f);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - (newHp - currHp));
            
            var time = (float)worldModel.GetProperty(PropertiesId.TIME);
            worldModel.SetProperty(PropertiesId.TIME, time + DURATION);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int maxhp = (int)worldModel.GetProperty(PropertiesId.MAXHP);
            int currHp = maxhp - (int)worldModel.GetProperty(PropertiesId.HP);

            float normalizedHp = currHp / maxhp;

            float currTime = (float)worldModel.GetProperty(PropertiesId.TIME);
            int timeLimit = GameManager.GameManager.TIME_LIMIT;

            float normalizedCurrTime = currTime / timeLimit;

            // If there are no enemies it would be dumb to rest
            if (character.GameManager.enemies.Count == 0)
            {
                return 100;
            }

            //Acceptable rest hp
            if ((int)worldModel.GetProperty(PropertiesId.HP) < 10)
                return 0.5f * normalizedCurrTime + 1.5f * normalizedHp;
            else
                return 7f * normalizedCurrTime; // After a while this action should not be performed
        }
    }

}