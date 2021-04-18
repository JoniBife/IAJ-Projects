using Assets.Scripts.IAJ.Unity.Util;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngineInternal;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public override string Name
        {
            get { return "Avoid Obstacle"; }
        }

        private GameObject obstacle;

        public GameObject Obstacle
        {
            get { return this.obstacle; }
            set
            {
                this.obstacle = value;
                this.ObstacleCollider = value.GetComponent<Collider>();
            }
        }

        private Collider ObstacleCollider { get; set; }
        public float MaxLookAhead { get; set; }

        public float AvoidMargin { get; set; }

        public float FanAngle { get; set; }

        public DynamicAvoidObstacle(GameObject obstacle)
        {
            this.Obstacle = obstacle;
            this.Target = new KinematicData();
            this.ObstacleCollider = obstacle.GetComponent<Collider>();
        }

        public override MovementOutput GetMovement()
        {
            this.Output.Clear();

            Vector3 newTargetPosition;

            if(!SmallWhiskersRaycast(out newTargetPosition))
            {
                return new MovementOutput();
            }

            base.Target.Position = newTargetPosition;
            return base.GetMovement();
        }

        private bool SimpleRaycast(out Vector3 newTargetPosition)
        {
            RaycastHit hit;

            Vector3 rayOrigin = this.Character.Position;
            Vector3 rayDirection = this.Character.velocity.normalized;

            // Casts a Ray that ignores all colliders except ObstacleCollider
            bool collision = ObstacleCollider.Raycast(
                new Ray(rayOrigin, rayDirection), out hit, MaxLookAhead);

            // Visible raycast used for debug
            Debug.DrawRay(this.Character.Position, rayDirection * MaxLookAhead, Color.white);

            // Was there a collision?
            if (collision)
            {
                // Collision occured so we seek towards the normal of the hit point
                newTargetPosition = hit.point + hit.normal * AvoidMargin;
                return true;
            }
            newTargetPosition = Vector3.zero;
            return false;
        }

        private bool SmallWhiskersRaycast(out Vector3 newTargetPosition)
        {
            
            // Straight Ray
            RaycastHit hitCenter;
            Vector3 rayOrigin = this.Character.Position;
            Vector3 rayDirection = this.Character.velocity.normalized;
           
            // Left Whisker
            RaycastHit hitLeft;
            Vector3 leftWhiskerDirection = MathHelper.Rotate2D(rayDirection, -FanAngle);

            // Right Whisker
            RaycastHit hitRight;
            Vector3 rightWhiskerDirection = MathHelper.Rotate2D(rayDirection, FanAngle);

            float smallerWhiskerLookAhead = MaxLookAhead / 2;

            // Casts a Ray that ignores all colliders except ObstacleCollider
            bool collisionCenter = ObstacleCollider.Raycast(
                new Ray(rayOrigin, rayDirection), out hitCenter, MaxLookAhead);
            bool collisionLeft = ObstacleCollider.Raycast(
                new Ray(rayOrigin, leftWhiskerDirection), out hitLeft, smallerWhiskerLookAhead);
            bool collisionRight = ObstacleCollider.Raycast(
                new Ray(rayOrigin, rightWhiskerDirection), out hitRight, smallerWhiskerLookAhead);

            // Visible raycast used for debug
            Debug.DrawRay(rayOrigin, rayDirection * MaxLookAhead, Color.white);
            Debug.DrawRay(rayOrigin, leftWhiskerDirection * smallerWhiskerLookAhead, Color.white);
            Debug.DrawRay(rayOrigin, rightWhiskerDirection * smallerWhiskerLookAhead, Color.white);

            // Was there a collision?
            if (collisionLeft)
            {
                // Collision occured so we seek towards the normal of the hit point
                newTargetPosition = hitLeft.point + hitLeft.normal * AvoidMargin;
                return true;
            } else if (collisionRight)
            {
                // Collision occured so we seek towards the normal of the hit point
                newTargetPosition = hitRight.point + hitRight.normal * AvoidMargin;
                return true;
            } else if (collisionCenter)
            {
                // Collision occured so we seek towards the normal of the hit point
                newTargetPosition = hitCenter.point + hitCenter.normal * AvoidMargin;
                return true;
            }

            newTargetPosition = Vector3.zero;
            return false;
        }

    }
}
