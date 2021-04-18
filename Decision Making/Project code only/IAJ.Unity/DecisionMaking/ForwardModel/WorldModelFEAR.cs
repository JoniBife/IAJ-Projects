using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{

    public class WorldModelFEAR : WorldModel
    {
        public object[] _props; // All properties have an id which can be used to access this array
        public GameManager.GameManager _GameManager;

        public WorldModelFEAR(GameManager.GameManager gameManager, List<Action> actions) : base(actions)
        {
            this._GameManager = gameManager;
            int targets = 0;

            foreach (Action action in actions)
            {
                WalkToTargetAndExecuteAction aTarget = action as WalkToTargetAndExecuteAction;
                if (aTarget != null)
                    ++targets;
            }

            this._props = new object[targets + 9];

            int i = 0;

            _props[i] = gameManager.characterData.Mana;
            _props[++i] = gameManager.characterData.HP;
            _props[++i] = gameManager.characterData.ShieldHP;
            _props[++i] = gameManager.characterData.MaxHP;
            _props[++i] = gameManager.characterData.XP;
            _props[++i] = gameManager.characterData.Time;
            _props[++i] = gameManager.characterData.Money;
            _props[++i] = gameManager.characterData.Level;
            _props[++i] = gameManager.initialPosition;

            foreach (Action action in actions)
            {
                WalkToTargetAndExecuteAction aTarget = action as WalkToTargetAndExecuteAction;
                if (aTarget != null)
                {
                    ++i;
                    _props[i] = true;
                    aTarget.TargetId = i;
                }    
            }
        }

        public WorldModelFEAR(WorldModelFEAR worldModel) : base(worldModel)
        {
            this._GameManager = worldModel._GameManager;
            this._props = new object[worldModel._props.Length];

            for (int i = 0; i < worldModel._props.Length; ++i)
            {
                _props[i] = worldModel._props[i];
            }
        }

        public override object GetProperty(int propertyId)
        {
            return _props[propertyId];
        }

        public override void SetProperty(int propertyId, object value)
        {
            _props[propertyId] = value;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new WorldModelFEAR(this);
        }
    }
}