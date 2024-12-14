using System;
using UnityEngine;

namespace Main
{
    [System.Serializable]
    public class CarEngine
    {
        [SerializeField] private AnimationCurve _torqueCurve;
        [SerializeField] private AnimationCurve _brakingCurve;
        [SerializeField] private float _angularInertia = 0.1f;
        
        [Space]
        [SerializeField] private float _idleRpmLow = 650;
        [SerializeField] private float _idleRpmHigh = 800;
        [SerializeField] private float _maxRpm = 6500;
        [SerializeField] private float _stallRpm = 400;
        
        [Space]
        [SerializeField] private bool _isRunning;

        [field: Space]
        [field: SerializeField] public float AngularVelocity { get; private set; }

        public event Action OnEngineStarted;
        public event Action OnEngineStopped;

        private float _localThrottle;

        private const float RPM_TO_RADS = 0.10472f;
        private const float RADS_TO_RPM = 9.5492742551f;

        public void Setup()
        {
            AngularVelocity = _idleRpmHigh * RPM_TO_RADS; // TODO: wyjebac pozniej
            _isRunning = false;
        }

        public float GetTorque(float throttlePositionNormalized)
        {
            float angularVelocityRpm = GetRpm();
            
            return Mathf.Lerp(-_brakingCurve.Evaluate(Mathf.Abs(angularVelocityRpm)) * Mathf.Sign(angularVelocityRpm),
                _torqueCurve.Evaluate(angularVelocityRpm),
                throttlePositionNormalized);
        }

        public float GetRpm()
        {
            return AngularVelocity * RADS_TO_RPM;
        }

        public void Ignite()
        {
            if (_isRunning)
            {
                _isRunning = false;
                OnEngineStopped?.Invoke();
            }
            else
            {
                AngularVelocity = _idleRpmHigh * RPM_TO_RADS; //setting the engine rpm to idle rpm
                _isRunning = true;
                OnEngineStarted?.Invoke();
            }
        }

        public void Update(float throttleInput)
        {
            float rpm = GetRpm();
            
            _localThrottle = throttleInput;
            
            if (_isRunning)
            {
                _localThrottle += Mathf.InverseLerp(_idleRpmHigh, _idleRpmLow, rpm);
                _localThrottle = Mathf.Clamp01(_localThrottle);

                if (rpm < _stallRpm)
                {
                    _isRunning = false;
                    OnEngineStopped?.Invoke();
                }
            }
            else
            {
                _localThrottle = 0;
            }

            AngularVelocity += GetTorque(_localThrottle) * Time.deltaTime / _angularInertia;
        }

        public void ApplyExternalTorque(float torque)
        {
            AngularVelocity += torque * Time.deltaTime / _angularInertia;
        }
    }
}