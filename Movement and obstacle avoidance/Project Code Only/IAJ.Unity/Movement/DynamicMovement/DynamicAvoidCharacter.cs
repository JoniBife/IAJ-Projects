using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidCharacter : DynamicMovement
    {
        public override string Name
        {
            get { return "Avoid Character"; }
        }

        public float MaxTimeLookAhead { get; set; }
        public float AvoidMargin { get; set; }


        public DynamicAvoidCharacter(KinematicData target)
        {
            this.Target = target;
            this.Output = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
            this.Output.Clear();

            Vector3 deltaPos = Target.Position - Character.Position;
            Vector3 deltaVel = Target.velocity - Character.velocity;

            float deltaSqrSpeed = deltaVel.sqrMagnitude;

            if (deltaSqrSpeed == 0)
            {
                return this.Output;
            }

            float tClosest = -(Vector3.Dot(deltaPos, deltaVel) / deltaSqrSpeed);

            if (tClosest > MaxTimeLookAhead)
            {
                return this.Output;
            }

            Vector3 futureDeltaPos = deltaPos + deltaVel * tClosest;
            float sqrFutureDistance = futureDeltaPos.sqrMagnitude;

            float sqrAvoidMargin = (2 * AvoidMargin) * (2 * AvoidMargin);

            if (sqrFutureDistance > sqrAvoidMargin)
            {
                return this.Output;
            }

            if (sqrFutureDistance <= 0 || deltaPos.sqrMagnitude < sqrAvoidMargin)
            {
                this.Output.linear = Character.Position - Target.Position;
                
            } else
            {
                this.Output.linear = futureDeltaPos * -1;
            }

            this.Output.linear.Normalize();
            this.Output.linear *= MaxAcceleration;
            this.Output.linear.y = 0.0f;

            return this.Output;

        }
    }
}
