using UnityEngine;

namespace Main
{
    public class CarController : MonoBehaviour
    {
        [Header("Car Elements")] 
        [SerializeField] private CarEngine _carEngine;
        [SerializeField] private CarTransmission _carTransmission;
        [SerializeField] private CarSuspension[] _carSuspensions;
        [SerializeField] private CarWheel[] _carWheels;

        [Header("Max Parameters")] 
        [SerializeField] private float _maxSteeringAngle = 30;
        [SerializeField] private float _maxEngineTorque = 80;
        [SerializeField] private float _maxBrakeTorque = 80;

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
            _carInput.OnIgnitionInputDetected += HandleEngineIgnition;
            
            _carEngine.OnEngineStarted += _carEngineAudioPlayer.Play;
            _carEngine.OnEngineStopped += _carEngineAudioPlayer.Stop;

            _carInput.OnGearSelected += _carTransmission.SetGear;
        }

        private void OnDisable()
        {
            _carInput.OnIgnitionInputDetected -= HandleEngineIgnition;
            
            _carEngine.OnEngineStarted -= _carEngineAudioPlayer.Play;
            _carEngine.OnEngineStopped -= _carEngineAudioPlayer.Stop;

            _carInput.OnGearSelected -= _carTransmission.SetGear;
            //
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
                for(int i=0; i<_carWheels.Length; i++)
                {
                    if (_carWheels[i].IsPowered)
                    {
                        _carWheels[i].ApplyExternalTorque((poweredWheelsVelocityAvg - _carWheels[i].AngularVelocity) * _carTransmission.PoweredWheelsVelocityDifferenceLimiter);
                    }
                }
            }
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
            for (int i = 0, c = _carWheels.Length; i < c; i++)
            {
                _carWheels[i].Update(_rb, _carSuspensions[i], 
                    _maxSteeringAngle * _carInput.HorizontalInput, 
                    engineToWheelsTorque, 
                    _maxBrakeTorque * _carInput.BrakeInput);
            }
        }
        
        private void HandleEngineIgnition()
        {
            _carEngine.Ignite();
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

        public float GetSpeed()
        {
            return _rb.velocity.magnitude * 3.6f;
        }

        public float GetEngineRpm()
        {
            return _carEngine.GetRpm();
        }

        public int GetCurrentGear()
        {
            return _carTransmission.CurrentGear;
        }
    }
}