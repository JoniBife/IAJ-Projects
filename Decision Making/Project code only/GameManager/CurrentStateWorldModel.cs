using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;

namespace Assets.Scripts.GameManager
{
    //class that represents a world model that corresponds to the current state of the world,
    //all required properties and goals are stored inside the game manager
    public class CurrentStateWorldModel : FutureStateWorldModel
    {
        private Dictionary<string, Goal> Goals { get; set; } 

        public CurrentStateWorldModel(GameManager gameManager, List<Action> actions, List<Goal> goals) : base(gameManager, actions)
        {
            this.Parent = null;
            this.Goals = new Dictionary<string, Goal>();

            foreach (var goal in goals)
            {
                this.Goals.Add(goal.Name,goal);
            }
        }

        public void Initialize()
        {
            this.ActionEnumerator.Reset();
        }

        public override object GetProperty(string propertyName)
        {

            //TIP: this code can be optimized by using a dictionary with lambda functions instead of if's  
            if (propertyName.Equals(Properties.MANA)) return this.GameManager.characterData.Mana;

            if (propertyName.Equals(Properties.XP)) return this.GameManager.characterData.XP;

            if (propertyName.Equals(Properties.MAXHP)) return this.GameManager.characterData.MaxHP;

            if (propertyName.Equals(Properties.HP)) return this.GameManager.characterData.HP;

            if (propertyName.Equals(Properties.ShieldHP)) return this.GameManager.characterData.ShieldHP;

            if (propertyName.Equals(Properties.MONEY)) return this.GameManager.characterData.Money;

            if (propertyName.Equals(Properties.TIME)) return this.GameManager.characterData.Time;

            if (propertyName.Equals(Properties.LEVEL)) return this.GameManager.characterData.Level;

            if (propertyName.Equals(Properties.POSITION))
                return this.GameManager.characterData.CharacterGameObject.transform.position;

            //if an object name is found in the dictionary of disposable objects, then the object still exists. The object has been removed/destroyed otherwise
            return this.GameManager.disposableObjects.ContainsKey(propertyName);
        }

        public override object GetProperty(int propertyId)
        {
            //TIP: this code can be optimized by using a dictionary with lambda functions instead of if's  
            if (propertyId.Equals(PropertiesId.MANA)) return this.GameManager.characterData.Mana;

            if (propertyId.Equals(PropertiesId.XP)) return this.GameManager.characterData.XP;

            if (propertyId.Equals(PropertiesId.MAXHP)) return this.GameManager.characterData.MaxHP;

            if (propertyId.Equals(PropertiesId.HP)) return this.GameManager.characterData.HP;

            if (propertyId.Equals(PropertiesId.ShieldHP)) return this.GameManager.characterData.ShieldHP;

            if (propertyId.Equals(PropertiesId.MONEY)) return this.GameManager.characterData.Money;

            if (propertyId.Equals(PropertiesId.TIME)) return this.GameManager.characterData.Time;

            if (propertyId.Equals(PropertiesId.LEVEL)) return this.GameManager.characterData.Level;

            if (propertyId.Equals(PropertiesId.POSITION)) return this.GameManager.characterData.CharacterGameObject.transform.position;

            //if an object name is found in the dictionary of disposable objects, then the object still exists. The object has been removed/destroyed otherwise
            foreach(Action a in Actions)
            {
                WalkToTargetAndExecuteAction walk = a as WalkToTargetAndExecuteAction;

                if (walk != null &&
                    walk.Target != null &&
                    GameManager.disposableObjects.ContainsKey(walk.Target.name) &&
                    walk.TargetId == propertyId && 
                    (bool)_props[propertyId] )
                {
                    return true;
                }
            }

            return false;
        }

        public override float GetGoalValue(string goalName)
        {
            return this.Goals[goalName].InsistenceValue;
        }

        public override void SetGoalValue(string goalName, float goalValue)
        {
            //this method does nothing, because you should not directly set a goal value of the CurrentStateWorldModel
        }

        public override void SetProperty(string propertyName, object value)
        {
            //this method does nothing, because you should not directly set a property of the CurrentStateWorldModel
        }

        public override int GetNextPlayer()
        {
            //in the current state, the next player is always player 0
            return 0;
        }
    }
}
