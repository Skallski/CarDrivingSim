using System.Text.RegularExpressions;
using UnityEngine;
using UtilsToolbox.PropertyAttributes;

namespace Main.Car
{
    [System.Serializable]
    public class CarWheel
    {
        [field: SerializeField] public bool IsTurn { get; private set; }
        [field: SerializeField] public bool IsPowered { get; private set; }
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public float AngularInertia { get; private set; }
        [field: SerializeField] public Transform MovingPartTransform { get; private set; }
        [field: SerializeField] public Transform StaticPartTransform { get; private set; }
        
        [field: Space]
        [field: SerializeField, ReadOnly] public float AngularVelocity { get; private set; } // rad/s

        private float _currentRotation;
        private float angularVelocityLast = 0f;

        private const float LONGITUDAL_FRICTION_MULTIPLIER = 0.2f;
        private const float LATERAL_FRICTION_MULTIPLIER = 0.5f;
        private const float BRAKING_STABILITY_MULTIPLIER = 0.08f;

        private float _longitudalSlipIntegral = 0;
        private float _lateralSlipIntegral = 0;
        private Vector3 _velocityInNormalPlane;

        private Vector3 _contactPosition;

        private bool _isGroundedLast;
        

        public void Update(Rigidbody carRb, CarSuspension carSuspension, float targetSteeringAngle, float engineTorque, float brakingTorque)
        {
            // turning wheel simulation
            if (IsTurn)
            {
                StaticPartTransform.rotation = Quaternion.Euler(0, targetSteeringAngle, 0) * carRb.rotation;
            }

            if (IsPowered)
            {
                AngularVelocity += (engineTorque / AngularInertia) * Time.deltaTime;
            }
            MovingPartTransform.localRotation = Quaternion.Euler(_currentRotation, 0, 0);
            
            // braking
            AngularVelocity -= brakingTorque * Mathf.Clamp(
                AngularVelocity * BRAKING_STABILITY_MULTIPLIER, -1f, 1f) / AngularInertia * Time.deltaTime;
            
            Transform carTransform = carRb.transform;
            Vector3 raycastWorldPos = carTransform.TransformPoint(carSuspension.LocalRaycastStartPoint);
            Vector3 raycastWorldDir = carTransform.TransformDirection(carSuspension.LocalRaycastDirection);

            raycastWorldPos -= raycastWorldDir * carSuspension.SpringHeight; // o ile wyzej zaczyna sie raycast nad kolem
            
            // Debug.DrawRay(raycastWorldPos, raycastWorldDir * carSuspension.SpringLength, Color.red, 0, false);
            
            if (Physics.Raycast(raycastWorldPos, raycastWorldDir, out RaycastHit hit, carSuspension.SpringLength,
                    LayerMask.GetMask("Floor")))
            {
                // spring simulation
                float springCompression = carSuspension.SpringLength - hit.distance;
                float springVelocity = (springCompression - carSuspension.SpringCompressionLastFrame) / Time.deltaTime;
                float springForce = springCompression * carSuspension.SpringRate + springVelocity * carSuspension.SpringDamping;
                float dot = Vector3.Dot(-raycastWorldDir, hit.normal);
                
                carRb.AddForceAtPosition(hit.normal * (dot * springForce), hit.point);

                carSuspension.SpringCompressionLastFrame = springCompression;
                StaticPartTransform.position = raycastWorldPos + raycastWorldDir * (hit.distance - Radius);
                
                // slip and friction
                Vector3 forwardForceDirection = Vector3.Cross(StaticPartTransform.right, hit.normal);
                Vector3 sideForceDirection = Vector3.Cross(-StaticPartTransform.forward, hit.normal);
                
                Vector3 carFrontDirection = Vector3.Cross(carRb.transform.right, hit.normal);
                Vector3 carSideDirection = Vector3.Cross(-carRb.transform.forward, hit.normal);
                // Debug.DrawRay(hit.point, forwardForceDirection, Color.magenta, 0, false);
                // Debug.DrawRay(hit.point, sideForceDirection, Color.cyan, 0, false);
                
                float wheelRollingVelocity = AngularVelocity * Radius;
                Vector3 worldWheelVelocity = carRb.GetPointVelocity(hit.point);
                float longitudalSlip = wheelRollingVelocity - Vector3.Dot(worldWheelVelocity, forwardForceDirection);
                float lateralSlip = Vector3.Dot(worldWheelVelocity, -sideForceDirection);

                float longitudalSlipClamped = Mathf.Clamp(longitudalSlip * LONGITUDAL_FRICTION_MULTIPLIER, -1, 1);
                float lateralSlipClamped = Mathf.Clamp(lateralSlip * LATERAL_FRICTION_MULTIPLIER, -1, 1);
                
                float staticFrictionTransition = Mathf.Clamp01(1.2f - (worldWheelVelocity.sqrMagnitude - Mathf.Abs(wheelRollingVelocity))*0.5f)*(brakingTorque*0.001f);
                if (_isGroundedLast==false)
                {
                    _contactPosition = hit.point;
                }
                Vector3 staticFrictionDistance = _contactPosition - hit.point;
                if (staticFrictionTransition > 0)
                {
                    _velocityInNormalPlane = Vector3.ProjectOnPlane(worldWheelVelocity,hit.normal);

                    _longitudalSlipIntegral += Vector3.Dot(-worldWheelVelocity, carFrontDirection) * Time.deltaTime * 2f * staticFrictionTransition;
                    _lateralSlipIntegral += Vector3.Dot(-worldWheelVelocity, carSideDirection) * Time.deltaTime * 2f * staticFrictionTransition;

                    const float intergralLimit = 1f;
                    
                    _longitudalSlipIntegral = Mathf.Clamp(_longitudalSlipIntegral, -intergralLimit*staticFrictionTransition, intergralLimit*staticFrictionTransition);
                    _lateralSlipIntegral = Mathf.Clamp(_lateralSlipIntegral, -intergralLimit*staticFrictionTransition, intergralLimit*staticFrictionTransition);
                }
                else
                {
                    _longitudalSlipIntegral = 0;
                    _lateralSlipIntegral = 0;
                        
                    _contactPosition = hit.point;
                }
                
                float frictionLimiter = 1f;
                Vector2 slipVector = new Vector2(longitudalSlipClamped, lateralSlipClamped);
                if (slipVector.sqrMagnitude > 1)
                {
                    frictionLimiter = 1f / slipVector.magnitude;
                }
                
                Vector3 frictionForce = (longitudalSlipClamped) * forwardForceDirection + (lateralSlipClamped) * sideForceDirection;
                if (staticFrictionTransition>0)
                {
                    frictionForce += (_longitudalSlipIntegral) * carFrontDirection + (_lateralSlipIntegral) * carSideDirection; //static friction during braking (for stopping on ramps)
                    frictionForce -= _velocityInNormalPlane * staticFrictionTransition*0.25f; //damping the bounce from static friction
                }
                
                
                carRb.AddForceAtPosition(
                    frictionForce * (frictionLimiter * (springCompression * carSuspension.SpringRate)), hit.point);
                 Debug.DrawLine(hit.point, hit.point + frictionForce * (springCompression * carSuspension.SpringRate) * 0.001f, Color.red);
                
                // slowing down the wheel from friction
                AngularVelocity -= longitudalSlipClamped * frictionLimiter * springCompression *
                    carSuspension.SpringRate / AngularInertia * Time.deltaTime * Radius;

                AngularVelocity += (AngularVelocity - angularVelocityLast) * -0.6f;

                angularVelocityLast = AngularVelocity;

                _isGroundedLast = true;
            }
            else
            {
                carSuspension.SpringCompressionLastFrame = 0;
                StaticPartTransform.position = raycastWorldPos + raycastWorldDir * (carSuspension.SpringLength - Radius);

                _isGroundedLast = false;
            }

            // spinning wheel simulation
            _currentRotation += AngularVelocity * Mathf.Rad2Deg * Time.deltaTime;
        }

        public void ApplyExternalTorque(float torque)
        {
            AngularVelocity += torque * Time.deltaTime / AngularInertia;
        }
    }
}