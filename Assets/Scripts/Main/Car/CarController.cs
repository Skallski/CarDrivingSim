using UnityEngine;

namespace Main.Car
{
    public class CarController : MonoBehaviour
    {
        [Header("Car Elements")] 
        [SerializeField] private CarEngine _carEngine;
        [SerializeField] private CarTransmission _carTransmission;
        [SerializeField] private CarSuspension[] _carSuspensions;
        [SerializeField] private CarWheel[] _carWheels;

        [Header("Aerodynamics")] 
        [SerializeField] private float _airDragCoefficient = 0.33f;
        [SerializeField] private float _airDragFrontalArea = 1.9f;
        [SerializeField] private float _airDragSagittalArea = 3.5f;

        [Header("Max Parameters")] 
        [SerializeField] private float _maxSteeringAngle = 30;
        [SerializeField] private float _maxBrakingTorque = 80;

        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CarInput _carInput;
        [SerializeField] private CarEngineAudioPlayer _carEngineAudioPlayer;
        
#if UNITY_EDITOR
        private void Reset()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
            }

            if (_carInput == null)
            {
                _carInput = FindObjectOfType<CarInput>();
            }
        }
#endif

        private void OnEnable()
        {
            _carInput.OnIgnitionInputDetected += _carEngine.Ignite;
            _carInput.OnGearSelected += _carTransmission.SetGear;

            _carEngine.OnEngineStarted += _carEngineAudioPlayer.Play;
            _carEngine.OnEngineStopped += _carEngineAudioPlayer.Stop;
        }

        private void OnDisable()
        {
            _carInput.OnIgnitionInputDetected -= _carEngine.Ignite;
            _carInput.OnGearSelected -= _carTransmission.SetGear;

            _carEngine.OnEngineStarted -= _carEngineAudioPlayer.Play;
            _carEngine.OnEngineStopped -= _carEngineAudioPlayer.Stop;
        }

        private void Start()
        {
            SetupAllSuspensions();
            
            _carEngine.Setup();
            _carTransmission.Setup();
        }

        private void FixedUpdate()
        {
            _carEngine.Update(_carInput.ThrottleInput);
            _carEngineAudioPlayer.TargetPitch = _carEngine.GetRpm();
            _carEngineAudioPlayer.TargetThrottle = _carInput.ThrottleInput;

            // update transmission
            float poweredWheelsVelocityAvg = GetPoweredWheelsVelocityAvg(out int poweredWheelsCount);
            (float engineResultTorque, float wheelsResultTorque) = _carTransmission.Update(_carInput.ClutchInput,
                _carEngine.AngularVelocity, poweredWheelsVelocityAvg);

            float torquePerPoweredWheel = 0;
            if (poweredWheelsCount > 0)
            {
                torquePerPoweredWheel = wheelsResultTorque / poweredWheelsCount;
            }
            
            _carEngine.ApplyExternalTorque(engineResultTorque);

            UpdateAllWheels(torquePerPoweredWheel);

            //powered wheels velocity difference limiter
            if (_carTransmission.PoweredWheelsVelocityDifferenceLimiter!=0)
            {
                for (int i = 0, c = _carWheels.Length; i < c; i++)
                {
                    CarWheel wheel = _carWheels[i];
                    if (wheel.IsPowered)
                    {
                        wheel.ApplyExternalTorque((poweredWheelsVelocityAvg - wheel.AngularVelocity) *
                                                  _carTransmission.PoweredWheelsVelocityDifferenceLimiter);
                    }
                }
            }

            ApplyAerodynamics();
        }

        private void SetupAllSuspensions()
        {
            for (int i = 0, c = _carSuspensions.Length; i < c; i++)
            {
                _carSuspensions[i].Setup(transform, _carWheels[i].StaticPartTransform);
            }
        }

        private void UpdateAllWheels(float engineToWheelsTorque)
        {
            float targetSteeringAngle = _maxSteeringAngle * _carInput.SteeringWheelInput;
            float brakingTorque = _maxBrakingTorque * _carInput.BrakeInput;
            
            for (int i = 0, c = _carWheels.Length; i < c; i++)
            {
                _carWheels[i].Update(_rb, _carSuspensions[i], targetSteeringAngle, engineToWheelsTorque, brakingTorque);
            }
        }

        private float GetPoweredWheelsVelocityAvg(out int count)
        {
            float sum = 0;
            count = 0;
            
            for (int i = 0, c = _carWheels.Length; i < c; i++)
            {
                CarWheel wheel = _carWheels[i];
                if (wheel.IsPowered)
                {
                    sum += _carWheels[i].AngularVelocity;
                    count++;
                }
            }

            if (count == 0)
            {
                Debug.LogError("No wheels are powered! Avg velocity is 0.");
                return 0;
            }
            
            return sum / count;
        }

        private void ApplyAerodynamics()
        {
            Vector3 velocity = _rb.velocity;
            
            float currentDragArea = Mathf.Lerp(_airDragSagittalArea, _airDragFrontalArea,
                Mathf.Abs(Vector3.Dot(transform.forward, velocity.normalized)));

            Vector3 airDragForce = velocity * (0.5f * _airDragCoefficient * currentDragArea * 1.2f * velocity.magnitude);
            
            _rb.AddForce(-airDragForce);
        }

        public bool IsEngineIgnited() => _carEngine.IsRunning;

        public float GetSpeed() => _carEngine.IsRunning ? _rb.velocity.magnitude * 3.6f : 0;

        public float GetEngineRpm() => _carEngine.IsRunning ? _carEngine.GetRpm() : 0;

        public int GetCurrentGear() => _carTransmission.CurrentGear;
    }
}