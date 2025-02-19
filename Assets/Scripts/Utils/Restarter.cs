using System;
using UnityEngine;

namespace Utils
{
    public class Restarter : MonoBehaviour
    {
        [SerializeField] private Transform _carTransform;
        [SerializeField] private KeyCode _restartKeyCode = KeyCode.R;

        private Vector3 _defaultPosition;

        public static event Action OnRestarted;

        private void Start()
        {
            _defaultPosition = _carTransform.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_restartKeyCode))
            {
                _carTransform.position = _defaultPosition;
                _carTransform.rotation = Quaternion.identity;
                
                OnRestarted?.Invoke();

                Debug.Log("Restarted");
            }
        }
    }
}