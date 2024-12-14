using System;
using UnityEngine;

namespace Main
{
    public class CarInput : MonoBehaviour
    {
        [field: SerializeField] public float HorizontalInput { get; private set; }
        
        [field: SerializeField] public float ClutchInput { get; private set; }
        [field:SerializeField] public float BrakeInput { get; private set; }
        [field: SerializeField] public float ThrottleInput { get; private set; }

        public event Action OnIgnitionInputDetected;
        public event Action<int> OnGearSelected;
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            
            ClutchInput = Input.GetKey(KeyCode.Q) ? 1 : 0;
            BrakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
            ThrottleInput = Input.GetKey(KeyCode.W) ? 1 : 0;

            if (Input.GetKeyDown(KeyCode.P))
            {
                OnIgnitionInputDetected?.Invoke();
            }
        }
    }
}