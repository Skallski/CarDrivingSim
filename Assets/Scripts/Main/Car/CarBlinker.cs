using System;
using UnityEngine;
using UtilsToolbox.PropertyAttributes;

namespace Main.Car
{
    [System.Serializable]
    public class CarBlinker
    {
        public enum Side
        {
            Left = 0, 
            Right = 1
        }

        [SerializeField] private Side _side;
        [field: SerializeField, ReadOnly] public bool IsOn { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsLit { get; private set; }

        private float _blinkingTimer;
        private const float BLINKING_INTERVAL = 0.321f;

        public event Action<bool> OnBlink;

        public void OnInteracted(Side side)
        {
            if (_side == side)
            {
                IsOn = !IsOn;
                IsLit = IsOn;
                _blinkingTimer = 0;
            }
            else
            {
                IsOn = false;

                IsLit = false;
                _blinkingTimer = 0;
            }
        }

        public void Update()
        {
            if (IsOn == false)
            {
                return;
            }
            
            _blinkingTimer += Time.deltaTime;
            if (_blinkingTimer >= BLINKING_INTERVAL)
            {
                OnBlink?.Invoke(IsLit);
                IsLit = !IsLit;
                _blinkingTimer = 0;
            }
        }
    }
}