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
        }

        private void OnDisable()
        {
            _carInput.OnIgnitionInputDetected -= HandleEngineIgnition;
            
            _carEngine.OnEngineStarted -= _carEngineAudioPlayer.Play;
            _carEngine.OnEngineStopped -= _carEngineAudioPlayer.Stop;
        }

        private void Start()
        {
            SetupAllSuspensions();
            _carEngine.Setup();
        }

        private void FixedUpdate()
        {
            UpdateAllWheels();
            _carEngine.Update();
        }

        private void SetupAllSuspensions()
        {
            for (int i = 0, c = _carSuspensions.Length; i < c; i++)
            {
                _carSuspensions[i].Setup(transform, _carWheels[i].StaticPartTransform);
            }
        }

        private void UpdateAllWheels()
        {
            for (int i = 0, c = _carWheels.Length; i < c; i++)
            {
                _carWheels[i].Update(_rb, _carSuspensions[i], 
                    _maxSteeringAngle * _carInput.HorizontalInput, 
                    _maxEngineTorque * _carInput.ForwardInput, 
                    _maxBrakeTorque * _carInput.BrakeInput);
            }
        }

        private void HandleEngineIgnition()
        {
            _carEngine.Ignite();
        }
    }
}
