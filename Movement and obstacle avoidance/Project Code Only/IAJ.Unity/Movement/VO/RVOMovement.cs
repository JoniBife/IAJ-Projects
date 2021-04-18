//adapted to IAJ classes by João Dias and Manuel Guimarães

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEditorInternal;
using UnityEngine;
using UnityScript.Steps;

namespace Assets.Scripts.IAJ.Unity.Movement.VO
{
    public class RVOMovement : DynamicMovement.DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "RVO"; }
        }

        protected List<KinematicData> Characters { get; set; }
        protected List<Collider> Obstacles { get; set; }
        public float CharacterSize { get; set; }
        public float ObstacleSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }
        public int NumSamples { get; set; }

        public float TcEpsilon { get; set; }

        public float MaxLookAhead { get; set; }
        public float CharacterWeight { get; set; }
        public float ObstacleWeight { get; set; }

        private List<Vector3> Samples = new List<Vector3>();
        
        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<GameObject> obs)
        {
            this.DesiredMovement = goalMovement;
            base.Target = new KinematicData();
            this.Characters = movingCharacters;
            this.Obstacles = obs.Select(o => o.GetComponent<Collider>()).ToList();
        }

        public override MovementOutput GetMovement()
        {

            // 1) Calculate desired velocity
            MovementOutput desiredMovementOutput = this.DesiredMovement.GetMovement();
            Vector3 desiredVelocity = Character.velocity + desiredMovementOutput.linear;
            
            if(desiredVelocity.sqrMagnitude > MaxSpeed*MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= MaxSpeed;
            }

            // 2) Generate velocity samples
            Samples.Clear();
            Samples.Add(desiredVelocity);
            for(int i = 0; i < NumSamples; ++i)   
            {
                float angle = UnityEngine.Random.Range(0, MathConstants.MATH_2PI);
                float magnitude = UnityEngine.Random.Range(0, MaxSpeed);
                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;
                Samples.Add(velocitySample);
            }

            // 3) Get best velocity sample
            base.Target.velocity = GetBestSample(desiredVelocity, Samples);

            return base.GetMovement();
        }

        private Vector3 GetBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = float.MaxValue;
            Vector3 characterPosition = this.Character.Position;
            RaycastHit hit;
            float sqrIgnoreDistance = IgnoreDistance * IgnoreDistance;
            Vector3 obsMax = Vector3.zero;


            foreach (Vector3 sample in Samples)
            {

                bool hasMaxPenalty = false;
                float maximumTimePenalty = 0;
                Vector3 rayDirection = sample.normalized;
                float sampleMagnitude = sample.magnitude;
                Ray ray = new Ray(characterPosition, rayDirection);

                foreach (Collider obsCollider in Obstacles)
                {
                    Bounds obsBounds = obsCollider.bounds;
                    Vector3 obsCenter = obsBounds.center;
                    obsCenter.y = 0;
                    Vector3 obsExtents = obsBounds.extents;
                    // Not using the Vector3 operators because their performance impact is large
                    obsMax.x = obsCenter.x + obsExtents.x;
                    obsMax.z = obsCenter.z + obsExtents.z;

                    obsMax.x -= obsCenter.x;
                    obsMax.z -= obsCenter.z;
                    float distanceCenterToMax = obsMax.magnitude;

                    obsCenter.x -= characterPosition.x;
                    obsCenter.z -= characterPosition.z;
                    float distanceToCenter = obsCenter.magnitude;

                    if (distanceToCenter > distanceCenterToMax + IgnoreDistance)
                        continue;

                    // Casts a Ray that ignores all colliders except ObstacleCollider
                    bool collision = obsCollider.Raycast(ray, out hit, MaxLookAhead);

                    // Was there a collision?
                    if (collision)
                    {
                        // Visible raycast used for debug
                        Debug.DrawRay(characterPosition, rayDirection * MaxLookAhead, Color.white);

                        // If we move at the sample velocity in how much time will we collide with the obstacle?
                        float tc = hit.distance / sampleMagnitude;
                        float timePenalty = 0;

                        // Will we collide in the future?
                        if (tc > TcEpsilon)
                        {
                            timePenalty = ObstacleWeight / tc;
                            if (timePenalty > maximumTimePenalty)
                                maximumTimePenalty = timePenalty;
                        }
                        else if (tc >= 0 && tc <= TcEpsilon) // Are we currently colliding? 
                        {
                            timePenalty = float.MaxValue;
                            maximumTimePenalty = timePenalty;
                            hasMaxPenalty = true;
                            break; // No need to continue, the timePenalty cannot be larger than float.MaxValue
                        }
                    }
                }

                if (hasMaxPenalty)
                    continue;

                foreach (KinematicData c in Characters)
                {
                    if (c == Character)
                        continue;

                    Vector3 deltaP = c.Position - characterPosition;

                    // Can we ignore this character?
                    if (deltaP.sqrMagnitude > sqrIgnoreDistance)
                    {
                        continue;
                    }

                    Vector3 rayVector = 2 * sample - Character.velocity - c.velocity;
                    float tc = MathHelper.TimeToCollisionBetweenRayAndCircle(characterPosition, rayVector, c.Position, CharacterSize * 2);
                    float timePenalty = 0;

                    // Will we collide in the future?
                    if (tc > TcEpsilon)
                    {
                        timePenalty = CharacterWeight / tc;
                        if (timePenalty > maximumTimePenalty)
                            maximumTimePenalty = timePenalty;
                    }
                    else if (tc >= 0 && tc <= TcEpsilon) // Are we currently colliding? 
                    {
                        timePenalty = float.MaxValue;
                        maximumTimePenalty = timePenalty;
                        hasMaxPenalty = true;
                        break; // No need to continue, the timePenalty cannot be larger than float.MaxValue
                    }
                }

                if (hasMaxPenalty)
                    continue;

                float distancePenalty = (desiredVelocity - sample).magnitude;

                // Calculating penalty
                float penalty = maximumTimePenalty + distancePenalty;

                if (penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;

                    if(penalty == 0)
                    {
                        // There is no penalty so this is the best sample therefore no need to continue
                        break;
                    }
                }
            }

            return bestSample;
        }
    }
}
