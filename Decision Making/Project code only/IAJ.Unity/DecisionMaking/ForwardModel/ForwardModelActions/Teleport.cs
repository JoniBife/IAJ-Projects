using UnityEngine;
using UnityEditor;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Teleport : Action
    {

        private AutonomousCharacter character;
        private const int MANA_DECREASE = 5;
        public Teleport(AutonomousCharacter character) : base("Teleport")
        {
            this.character = character;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            return (int)worldModel.GetProperty(PropertiesId.LEVEL) >= 2 && (int)worldModel.GetProperty(PropertiesId.MANA) >= 5;
        }

        public override bool CanExecute()
        {
            return character.GameManager.characterData.Level >= 2 && character.GameManager.characterData.Mana >= 5;
        }

        public override void Execute()
        {
            base.Execute();
            character.GameManager.Teleport();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            int currMana = (int)worldModel.GetProperty(PropertiesId.MANA);
            currMana -= MANA_DECREASE;


            worldModel.SetProperty(PropertiesId.MANA, currMana);
            worldModel.SetProperty(PropertiesId.POSITION, character.GameManager.initialPosition);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int shieldHp = (int)worldModel.GetProperty(PropertiesId.ShieldHP);
            int mana = (int)worldModel.GetProperty(PropertiesId.MANA);

            return 1.5f * mana/10 + 0.5f* shieldHp/5;
        }
    }
}