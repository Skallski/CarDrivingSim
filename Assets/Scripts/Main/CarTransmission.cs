using UnityEngine;

namespace Main
{
    [System.Serializable]
    public class CarTransmission
    {
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

        public void Setup()
        {
            _forwardGearRatiosInverse = new float[_forwardGearRatios.Length];
            for (int i = 0, c = _forwardGearRatiosInverse.Length; i < c; i++)
            {
                _forwardGearRatiosInverse[i] = 1f - _forwardGearRatios[i];
            }

            _reverseGearRatioInverse = 1f - _reverseGearRatio;
            _finalGearRatioInverse = 1f - _finalGearRatio;
        }

        private float GetGearRatio(int gear)
        {
            gear = Mathf.Clamp(gear, -1, _forwardGearRatios.Length);
            Debug.Log(gear);

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
            Debug.Log(gear);

            return gear switch
            {
                -1 => _reverseGearRatioInverse * _finalGearRatioInverse,
                0 => 1, // neutral
                _ => _forwardGearRatiosInverse[gear - 1] * _finalGearRatioInverse
            };
        }

        public (float engineResultTorque, float wheelsResultTorque) Update(float clutchInput, float engineAngularVelocity, 
            float wheelsAngularVelocityAvg)
        {
            clutchInput = Mathf.Clamp01(clutchInput);

            if (CurrentGear == 0)
            {
                return (0, 0);
            }

            float gearRatio = GetGearRatio(CurrentGear);
            float gearRationInverse = GetGearRatioInverse(CurrentGear);
            float angularVelocityDifference = engineAngularVelocity - wheelsAngularVelocityAvg * gearRationInverse;
            float resultFrictionTorque = Mathf.Clamp(
                angularVelocityDifference * _clutchTorqueSlipMultiplier * _clutchMaxTorque,
                    -_clutchMaxTorque, _clutchMaxTorque) * clutchInput;

            return (-resultFrictionTorque, resultFrictionTorque * gearRatio);
        }
    }
}