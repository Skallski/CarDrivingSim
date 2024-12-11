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
        [SerializeField] private float _idleRpm = 750;
        [SerializeField] private float _maxRpm = 6500;
        [SerializeField] private bool _isRunning;
        
        [Space]
        [SerializeField] private float _angularVelocity;

        public event Action OnEngineStarted;
        public event Action OnEngineStopped;

        private const float RPM_TO_RADS = 0.10472f;
        private const float RADS_TO_RPM = 9.5492742551f;

        public void Setup()
        {
            _isRunning = false;
        }

        public float GetTorque(float throttlePositionNormalized)
        {
            float angularVelocityRpm = _angularVelocity * RADS_TO_RPM;
            
            return Mathf.Lerp(-_brakingCurve.Evaluate(angularVelocityRpm), _torqueCurve.Evaluate(angularVelocityRpm),
                throttlePositionNormalized);
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
                _isRunning = true;
                OnEngineStarted?.Invoke();
            }
        }

        public void Update()
        {
            if (_isRunning)
            {
                
            }
        }
    }
}