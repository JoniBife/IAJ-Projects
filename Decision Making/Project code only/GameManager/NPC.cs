using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;

namespace Assets.Scripts.GameManager
{

    public class NPC : MonoBehaviour
    {

        public string Name;
        public string Type;
        // Stats
        public int XPvalue;
        public int HP { get; set; }
        public int AC;
        public int simpleDamage;
        //how do you like lambda's in c#?
        public Func<int> dmgRoll;

        public DynamicCharacter character;

        // Use this for initialization
        void Start()
        {
            this.Name = this.transform.gameObject.name;
            this.Type = this.transform.gameObject.tag;
            
            switch (this.Type)
            {
                case "Skeleton":
                    this.XPvalue = 3;
                    this.AC = 10;
                    this.HP = 5;
                    this.dmgRoll = () => RandomHelper.RollD6();
                    this.simpleDamage = 3;
                    break;
                case "Orc":
                    this.XPvalue = 10;
                    this.AC = 14;
                    this.HP = 15;
                    this.dmgRoll = () => RandomHelper.RollD10() +2;
                    this.simpleDamage = 8;
                    break;
                case "Dragon":
                    this.XPvalue = 20;
                    this.AC = 16;
                    this.HP = 30;
                    this.dmgRoll = () => RandomHelper.RollD12() + RandomHelper.RollD12();
                    this.simpleDamage = 13;
                    break;
                default:
                    this.XPvalue = 3;
                    this.AC = 10;
                    this.HP = 5;
                    this.dmgRoll = () => RandomHelper.RollD6();
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (character != null)
                character.Update();
        }
    }
}
