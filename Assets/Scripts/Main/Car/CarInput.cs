using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main.Car
{
    public class CarInput : MonoBehaviour
    {
        [field: SerializeField] public float HorizontalInput { get; private set; }
        [field: SerializeField] public float ClutchInput { get; private set; }
        [field:SerializeField] public float BrakeInput { get; private set; }
        [field: SerializeField] public float ThrottleInput { get; private set; }

        public event Action OnIgnitionInputDetected;
        public event Action<int> OnGearSelected;
        public event Action<Blinker.Side> OnBlinkerInteracted;
        
        private readonly Dictionary<KeyCode, int> _gearKeyMapping = new()
        {
            { KeyCode.Alpha0, 0 },
            { KeyCode.Alpha1, 1 },
            { KeyCode.Alpha2, 2 },
            { KeyCode.Alpha3, 3 },
            { KeyCode.Alpha4, 4 },
            { KeyCode.Alpha5, 5 },
            { KeyCode.Alpha6, 6 },
            { KeyCode.Minus, -1 },
        };

        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            
            ClutchInput = Input.GetKey(KeyCode.L) ? 1 : 0;
            BrakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
            ThrottleInput = Input.GetKey(KeyCode.W) ? 1 : 0;

            if (Input.GetKeyDown(KeyCode.P))
            {
                OnIgnitionInputDetected?.Invoke();
            }

            foreach (KeyValuePair<KeyCode, int> kvp in _gearKeyMapping)
            {
                if (Input.GetKeyDown(kvp.Key))
                {
                    OnGearSelected?.Invoke(kvp.Value);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                OnBlinkerInteracted?.Invoke(Blinker.Side.Left);
            }
            
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                OnBlinkerInteracted?.Invoke(Blinker.Side.Right);
            }
        }
    }
}