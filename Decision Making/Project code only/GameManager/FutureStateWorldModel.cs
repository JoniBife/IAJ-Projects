using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager
{
    public class FutureStateWorldModel : WorldModelFEAR
    {
        protected GameManager GameManager { get; set; }
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected List<Action> NextEnemyActions { get; set; }

        public FutureStateWorldModel(GameManager gameManager, List<Action> actions) : base(gameManager,actions)
        {
            this.GameManager = gameManager;
            this.NextPlayer = 0;
        }

        public FutureStateWorldModel(FutureStateWorldModel parent) : base(parent)
        {
            this.GameManager = parent.GameManager;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new FutureStateWorldModel(this);
        }

        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(PropertiesId.HP);
            float time = (float)this.GetProperty(PropertiesId.TIME);
            int money = (int)this.GetProperty(PropertiesId.MONEY);

            return HP <= 0 ||  time >= 200 || (this.NextPlayer == 0 && money == 25);
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(PropertiesId.MONEY);
            int HP = (int)this.GetProperty(PropertiesId.HP);

            if (HP <= 0) return 0.0f;
            else if (money == 25)
            {
                return 1.0f;
            }
            else return 0.0f;
        }

        public override int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public override void CalculateNextPlayer()
        {
            Vector3 position = (Vector3)this.GetProperty(PropertiesId.POSITION);
            position.y = 0;
            bool enemyEnabled;

            foreach (var action in this.Actions)
            {
                SwordAttack swordAttack = action as SwordAttack;

                if (swordAttack != null && (bool)GetProperty(swordAttack.TargetId) && swordAttack.Target != null)
                {
                    Vector3 enemyPosition = swordAttack.Target.transform.position;
                    enemyPosition.y = 0;

                    if ((enemyPosition-position).sqrMagnitude <= 800)
                    {
                        this.NextPlayer = 1;
                        SwordAttack nextEnemyAction = new SwordAttack(this.GameManager.autonomousCharacter, swordAttack.Target);
                        nextEnemyAction.TargetId = swordAttack.TargetId;
                        this.NextEnemyAction = nextEnemyAction;
                        this.NextEnemyActions = new List<Action> { this.NextEnemyAction };
                        return;
                    }

                } 

            }


            //basically if the character is close enough to an enemy, the next player will be the enemy.
            /*foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool) this.GetProperty(enemy.name);

                Vector3 enemyPosition = enemy.transform.position;
                enemyPosition.y = 0;

                if (enemyEnabled && (enemyPosition - position).sqrMagnitude <= 100)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.GameManager.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return; 
                }
            }*/
            this.NextPlayer = 0;
            //if not, then the next player will be player 0
        }

        public override Action GetNextAction()
        {
            Action action;
            if (this.NextPlayer == 1)
            {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
                return action;
            }
            else return base.GetNextAction();
        }

        public override List<Action> GetExecutableActions()
        {
            if (this.NextPlayer == 1)
            {
                return this.NextEnemyActions;
            }
            else return base.GetExecutableActions();
        }

    }
}
