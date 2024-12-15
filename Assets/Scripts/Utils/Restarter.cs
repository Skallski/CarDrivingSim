using UnityEngine;

namespace Utils
{
    public class Restarter : MonoBehaviour
    {
        [SerializeField] private Rigidbody _carRb;
        [SerializeField] private Vector3 _defaultPosition;
        [SerializeField] private KeyCode _restartKeyCode = KeyCode.R;

        private Transform _carTransform;
        
        private void Update()
        {
            if (Input.GetKeyDown(_restartKeyCode))
            {
                _carRb.velocity = Vector3.zero;

                if (_carTransform == null)
                {
                    _carTransform = _carRb.transform;
                }

                _carTransform.position = _defaultPosition;
                _carTransform.rotation = Quaternion.identity;

                Debug.Log("Restarted");
            }
        }
    }
}