using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {

        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion", character, target)
        {

        }

        public override bool CanExecute()
        {
            if (base.CanExecute() && Character.GameManager.characterData.Mana < 10) return true;
            return false;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (base.CanExecute(worldModel) && (int)worldModel.GetProperty(PropertiesId.MANA) < 10) return true;
            return false;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.GetManaPotion(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            worldModel.SetProperty(PropertiesId.MANA, 10);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.TargetId, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {

            int mana = (int)worldModel.GetProperty(PropertiesId.MANA);

            int normalizedMana = mana / 10;

            return base.GetHValue(worldModel)/4 + 1.5f* normalizedMana;
        }
    }
}