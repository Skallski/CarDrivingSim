using UnityEngine;

namespace Main
{
    [System.Serializable]
    public class CarWheel
    {
        [field: SerializeField] public bool IsTurn { get; private set; }
        [field: SerializeField] public bool IsPowered { get; private set; }
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public float AngularInertia { get; private set; }
        
        [field: Space]
        [field: SerializeField] public float AngularVelocity { get; private set; } // rad/s

        [field: Space]
        [field: SerializeField] public Transform MovingPartTransform { get; private set; }
        [field: SerializeField] public Transform StaticPartTransform { get; private set; }

        private float _currentRotation;

        private const float FRICTION_MULTIPLIER = 0.5f;
        private const float BRAKING_STABILITY_MULTIPLIER = 0.2f;
        
        public void Update(Rigidbody carRb, CarSuspension carSuspension, float targetSteeringAngle, float engineTorque, float brakingTorque)
        {
            // turning wheel simulation
            if (IsTurn)
            {
                StaticPartTransform.rotation = Quaternion.Euler(0, targetSteeringAngle, 0) * carRb.rotation;
            }

            // spinning wheel simulation
            _currentRotation += AngularVelocity * Mathf.Rad2Deg * Time.deltaTime;
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
                // Debug.DrawRay(hit.point, forwardForceDirection, Color.magenta, 0, false);
                // Debug.DrawRay(hit.point, sideForceDirection, Color.cyan, 0, false);
                
                float wheelRollingVelocity = AngularVelocity * Radius;
                Vector3 worldWheelVelocity = carRb.GetPointVelocity(hit.point);
                float longitudalSlip = wheelRollingVelocity - Vector3.Dot(worldWheelVelocity, forwardForceDirection);
                float lateralSlip = Vector3.Dot(worldWheelVelocity, -sideForceDirection);

                float longitudalSlipClamped = Mathf.Clamp(longitudalSlip * FRICTION_MULTIPLIER, -1, 1);
                float lateralSlipClamped = Mathf.Clamp(lateralSlip * FRICTION_MULTIPLIER, -1, 1);
                
                float frictionLimiter = 1f;
                Vector2 slipVector = new Vector2(longitudalSlipClamped, lateralSlipClamped);
                if (slipVector.sqrMagnitude > 1)
                {
                    frictionLimiter = 1f / slipVector.magnitude;
                }
                
                Vector3 frictionForce = longitudalSlipClamped * forwardForceDirection + lateralSlipClamped * sideForceDirection;
                
                carRb.AddForceAtPosition(
                    frictionForce * (frictionLimiter * (springCompression * carSuspension.SpringRate)), hit.point);
                // Debug.DrawLine(hit.point, hit.point + frictionForce * (springCompression * carSuspension.SpringRate) * 0.001f, Color.red);
                
                // slowing down the wheel from friction
                AngularVelocity -= longitudalSlipClamped * frictionLimiter * springCompression *
                    carSuspension.SpringRate / AngularInertia * Time.deltaTime * Radius;
            }
            else
            {
                carSuspension.SpringCompressionLastFrame = 0;
                StaticPartTransform.position = raycastWorldPos + raycastWorldDir * (carSuspension.SpringLength - Radius);
            }
        }
    }
}