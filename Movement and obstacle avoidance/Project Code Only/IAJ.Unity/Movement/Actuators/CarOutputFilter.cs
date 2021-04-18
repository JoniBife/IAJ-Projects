using UnityEngine;
using Assets.Scripts.IAJ.Unity.Util;
using System;

namespace Assets.Scripts.IAJ.Unity.Movement.Actuators
{
    public class CarOutputFilter : OutputFilter
    {

        private float maxSteeringAngle;

        public CarOutputFilter(float maxSteeringAngle)
        {
            this.maxSteeringAngle = maxSteeringAngle;
        }

        public override MovementOutput Filter(KinematicData characterData, MovementOutput desiredMo)
        {
            // The new velocity which is trying to be achieved
            Vector3 newVelocity = characterData.velocity + desiredMo.linear * Time.deltaTime;

            // The angle between the new velocity and the current velocity
            float angle = Vector3.Angle(characterData.velocity, newVelocity);

            // Only filter if angle is larger than max steering angle
            if (((angle * MathConstants.MATH_PI) / 180) > maxSteeringAngle)
            {
                float desiredVelocityAngle = MathHelper.ConvertVectorToOrientation(newVelocity);
                float currentVelocityAngle = MathHelper.ConvertVectorToOrientation(characterData.velocity);

                // Calculating the new acceleration angle with the same direction of the desired velocty
                float newAccelerationAngle = currentVelocityAngle + maxSteeringAngle * Math.Sign(desiredVelocityAngle);
                Vector3 newAcceleration = MathHelper.ConvertOrientationToVector(newAccelerationAngle);
                newAcceleration *= desiredMo.linear.magnitude;
                return new MovementOutput() { linear = newAcceleration , angular = desiredMo.angular };
            }

            return desiredMo;
        }
    }
}