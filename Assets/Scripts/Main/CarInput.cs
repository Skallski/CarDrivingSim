using System;
using UnityEngine;

namespace Main
{
    public class CarInput : MonoBehaviour
    {
        [field: SerializeField] public float HorizontalInput { get; private set; }
        [field: SerializeField] public float ForwardInput { get; private set; }
        [field:SerializeField] public float BrakeInput { get; private set; }

        public event Action OnIgnitionInputDetected;
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            ForwardInput = Input.GetAxisRaw("Vertical");
            BrakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;

            if (Input.GetKeyDown(KeyCode.P))
            {
                OnIgnitionInputDetected?.Invoke();
            }
        }
    }
}