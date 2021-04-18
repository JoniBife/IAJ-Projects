using UnityEngine;
using UnityEditor;

namespace Assets.Scripts.IAJ.Unity.Movement.Actuators
{
    public abstract class OutputFilter
    {
        /**
         * Receives a movement output as parameter
         * and converts it to another MovementOutput
         * that is more adequate to the type of character
         */
        public abstract MovementOutput Filter(KinematicData characterData, MovementOutput desiredMo);
    }
}