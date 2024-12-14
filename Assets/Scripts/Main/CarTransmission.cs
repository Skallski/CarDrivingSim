using UnityEngine;

namespace Main
{
    [System.Serializable]
    public class CarTransmission
    {
        [Header("Settings")]
        [SerializeField] private bool _isAutomaticClutch;
        [SerializeField] private float _automaticClutchMinAngularVelocity;
        [SerializeField] private float _automaticClutchMaxAngularVelocity;
        // gears go like this:
        // -1 : reverse, 0 : neutral, 1 : first, ect.
        [field: SerializeField] public int CurrentGear { get; set; }
        
        [Space]
        [SerializeField] private float[] _forwardGearRatios;
        [SerializeField] private float _reverseGearRatio;
        [SerializeField] private float _finalGearRatio;

        [Space]
        [SerializeField] private float _clutchMaxTorque = 100;
        [SerializeField] private float _clutchTorqueSlipMultiplier = 2;
        
        private float[] _forwardGearRatiosInverse;
        private float _reverseGearRatioInverse;
        private float _finalGearRatioInverse;

        private float _clutchInputAfterUpdate;

        private float _resultFrictionTorqueLast = 0;
        private float _resultFrictionTorqueChange = 0;
        private float _resultFrictionTorqueChangeLast = 0;

        public void Setup()
        {
            _forwardGearRatiosInverse = new float[_forwardGearRatios.Length];
            for (int i = 0, c = _forwardGearRatiosInverse.Length; i < c; i++)
            {
                _forwardGearRatiosInverse[i] = 1f / _forwardGearRatios[i];
            }

            _reverseGearRatioInverse = 1f / _reverseGearRatio;
            _finalGearRatioInverse = 1f / _finalGearRatio;
        }

        private float GetGearRatio(int gear)
        {
            gear = Mathf.Clamp(gear, -1, _forwardGearRatios.Length);          

            return gear switch
            {
                -1 => _reverseGearRatio * _finalGearRatio,
                0 => 1, // neutral
                _ => _forwardGearRatios[gear - 1] * _finalGearRatio
            };
        }
        
        private float GetGearRatioInverse(int gear)
        {
            gear = Mathf.Clamp(gear, -1, _forwardGearRatiosInverse.Length);

            return gear switch
            {
                -1 => _reverseGearRatioInverse * _finalGearRatioInverse,
                0 => 1, // neutral
                _ => _forwardGearRatiosInverse[gear - 1] * _finalGearRatioInverse
            };
        }

        public void SetGear(int gear)
        {
            if (_isAutomaticClutch == false)
            {
                if (_clutchInputAfterUpdate > 0.9f)
                {
                    CurrentGear = Mathf.Clamp(gear, -1, _forwardGearRatiosInverse.Length);
                }
                else
                {
                    Debug.LogWarning("Uwaga! Zmielisz synchrosy!");
                }
            }
            else
            {
                CurrentGear = Mathf.Clamp(gear, -1, _forwardGearRatiosInverse.Length);
            }
        }

        public (float engineResultTorque, float wheelsResultTorque) Update(float clutchInput, float engineAngularVelocity, 
            float wheelsAngularVelocityAvg)
        {
            _clutchInputAfterUpdate = clutchInput;
            float clutchPosition = 1f - clutchInput; //when the clutch pedal is pressed then the clutch is disengaged (no friction)!

            if (_isAutomaticClutch)
            {
                if (CurrentGear <= 1)
                {
                    clutchPosition = Mathf.InverseLerp(_automaticClutchMinAngularVelocity, _automaticClutchMaxAngularVelocity, engineAngularVelocity);
                    clutchPosition = Mathf.Clamp01(clutchPosition);
                }
                else
                {
                    clutchPosition = 1;
                }
            }
            else
            {
                clutchPosition = Mathf.Clamp01(clutchPosition);
            }

            if (CurrentGear == 0)
            {
                return (0, 0);
            }

            float gearRatio = GetGearRatio(CurrentGear);
            Debug.Log("Gear ratio = " + gearRatio);

            float gearRationInverse = GetGearRatioInverse(CurrentGear);

            float angularVelocityDifference = engineAngularVelocity - (wheelsAngularVelocityAvg) * gearRatio;
            float resultFrictionTorque = Mathf.Clamp(
                angularVelocityDifference * _clutchTorqueSlipMultiplier * Mathf.Abs(gearRationInverse) * _clutchMaxTorque,
                    -_clutchMaxTorque, _clutchMaxTorque) * clutchPosition;

            _resultFrictionTorqueChangeLast = _resultFrictionTorqueChange;
            _resultFrictionTorqueChange = (resultFrictionTorque - _resultFrictionTorqueLast);
            _resultFrictionTorqueLast = resultFrictionTorque;

            float resultFrictionTorqueCorrected = resultFrictionTorque - (_resultFrictionTorqueChange- _resultFrictionTorqueChangeLast)*ValueTesterScript.testValueStatic;

            return (-resultFrictionTorqueCorrected, resultFrictionTorqueCorrected * gearRatio);
        }
    }
}