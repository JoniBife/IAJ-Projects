using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest",character,target)
        {
        }

        public float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.GET_RICH_GOAL) change -= 5.0f;
            return change;
        }

        public override bool CanExecute()
        {

            if (!base.CanExecute() /*|| Character.GameManager.EnemyInRange(Target.transform.position)*/)
                return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel) /*|| Character.GameManager.EnemyInRange(Target.transform.position)*/) return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.PickUpChest(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.GET_RICH_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GET_RICH_GOAL, goalValue - 5.0f);

            var money = (int)worldModel.GetProperty(PropertiesId.MONEY);
            worldModel.SetProperty(PropertiesId.MONEY, money + 5);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.TargetId, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(PropertiesId.POSITION);
            var distance = getDistance(position, Target.transform.position);

            // If the chest is next to the player it would be dumb not to get it
            if (distance <= 20)
                return -100;

            
            return base.GetHValue(worldModel)/1.5f;
        }
    }
}
