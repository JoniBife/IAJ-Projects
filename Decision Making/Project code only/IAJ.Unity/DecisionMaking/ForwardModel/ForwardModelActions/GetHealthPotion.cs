using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {

        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion", character, target)
        {

        }

        public override bool CanExecute()
        {
            if (Character.GameManager.enemies.Count > 0 && base.CanExecute() && Character.GameManager.characterData.HP < Character.GameManager.characterData.MaxHP) return true;
            return false;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (Character.GameManager.enemies.Count > 0 && base.CanExecute(worldModel) && (int)worldModel.GetProperty(PropertiesId.HP) < (int)worldModel.GetProperty(PropertiesId.MAXHP)) return true;
            return false;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.GetHealthPotion(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            var currHp = (int)worldModel.GetProperty(PropertiesId.HP);
            var maxHp = (int)worldModel.GetProperty(PropertiesId.MAXHP);

            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, 0);
            worldModel.SetProperty(PropertiesId.HP, maxHp);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.TargetId, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int maxhp = (int)worldModel.GetProperty(PropertiesId.MAXHP);

            // If there are no enemies it would be dumb to rest
            if (Character.GameManager.enemies.Count == 0)
            {
                return 100;
            }

            //Acceptable rest hp
            // Using 10 as an arbitrary value because maxhp/2 might cause the character the rest when he has a lot of health
            if (maxhp - (int)worldModel.GetProperty(PropertiesId.HP) >= 10)
                return base.GetHValue(worldModel);
            else
                return 100; // No need to get health potion when you have a lot of health
        }
    }
}