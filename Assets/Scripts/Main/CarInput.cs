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
            
            ClutchInput = Input.GetKey(KeyCode.L) ? 1 : 0;
            BrakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
            ThrottleInput = Input.GetKey(KeyCode.W) ? 1 : 0;

            if (Input.GetKeyDown(KeyCode.P))
            {
                OnIgnitionInputDetected?.Invoke();
            }


            //TODO: zrobic ladniej
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnGearSelected?.Invoke(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                OnGearSelected?.Invoke(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                OnGearSelected?.Invoke(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                OnGearSelected?.Invoke(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                OnGearSelected?.Invoke(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                OnGearSelected?.Invoke(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                OnGearSelected?.Invoke(0);
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                OnGearSelected?.Invoke(-1);
            }
        }
    }
}