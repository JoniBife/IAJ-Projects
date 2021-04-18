using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class ShieldOfFaith : Action
    {

        private int expectedManaChange;
        private int shieldHp;
        private AutonomousCharacter character;

        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            expectedManaChange = 5;
            shieldHp = 5;
            this.character = character;
        }

        public override bool CanExecute()
        {
            if (character.GameManager.characterData.ShieldHP < shieldHp && character.GameManager.characterData.Mana >= expectedManaChange) return true;
            return false;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if ((int)worldModel.GetProperty(PropertiesId.ShieldHP) < shieldHp && (int)worldModel.GetProperty(PropertiesId.MANA) >= expectedManaChange) return true;
            return false;
        }

        public override void Execute()
        {
            base.Execute();
            this.character.GameManager.ShieldOfFaith();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var currShieldHp = (int)worldModel.GetProperty(PropertiesId.ShieldHP);

            worldModel.SetProperty(PropertiesId.ShieldHP, shieldHp);

            var currmana = (int)worldModel.GetProperty(PropertiesId.MANA);
            currmana -= expectedManaChange;
            worldModel.SetProperty(PropertiesId.MANA, currmana);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - (shieldHp - currShieldHp));
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int currShield = (int)worldModel.GetProperty(PropertiesId.ShieldHP);
            int maxShield = 5;

            int normalizedShieldHp = currShield / maxShield;

            if (currShield <= 2)
                return -100;
            else
                return normalizedShieldHp * 2;
        }
    }
}