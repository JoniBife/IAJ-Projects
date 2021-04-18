using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedXPChange;
        private int expectedManaChange;
        private int xpChange;
        private int attackRange;
        private bool isTargetSkeleton;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {
            if (target.tag.Equals("Skeleton"))
            {
                this.expectedManaChange = 2;
                this.xpChange = 3;
                this.expectedXPChange = 2.7f;
                isTargetSkeleton = true;
            }
        }

        public override bool CanExecute()
        {
            if (base.CanExecute() && isTargetSkeleton && Character.GameManager.characterData.Mana >= expectedManaChange) return true;
            return false;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (base.CanExecute(worldModel) && isTargetSkeleton && (int)worldModel.GetProperty(PropertiesId.MANA) >= expectedManaChange) return true;
            return false;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int xp = (int)worldModel.GetProperty(PropertiesId.XP);
            int mana = (int)worldModel.GetProperty(PropertiesId.MANA);

            //there was an hit, enemy is destroyed, gain xp
            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.TargetId, false);

            worldModel.SetProperty(PropertiesId.XP, xp + this.xpChange);
            var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, xpValue - this.xpChange);

            worldModel.SetProperty(PropertiesId.MANA, mana - expectedManaChange);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return (base.GetDuration(worldModel) * 40)/1.5f; // TODO Devine Smite
        }
    }
}