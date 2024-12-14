using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Rigidbody _trackedRigibody;
    [SerializeField] private Vector3 _pointOffset;
    [SerializeField] private Vector3 _eulerAnglesRotationOffset;
    
    [Space]
    [SerializeField] private float _positionLerpSpeed = 5f;
    [SerializeField] private float _rotationLerpSpeed = 5f;

    [Space]
    [SerializeField] private bool _useVelocity = true;
    
    private Quaternion _smoothedRotation = Quaternion.identity;

    private void Update()
    {
        if (_trackedRigibody)
        {
            if (_useVelocity)
            {
                float velocityMagnitude = _trackedRigibody.velocity.magnitude;

                float velocityModeLerp = Mathf.Clamp01(velocityMagnitude*0.25f - 0.5f);
                Quaternion rotationBasedOnVelocity = Quaternion.LookRotation(_trackedRigibody.velocity.normalized, Vector3.up);

                _smoothedRotation = Quaternion.Lerp(_smoothedRotation, Quaternion.Lerp(_trackedRigibody.rotation, rotationBasedOnVelocity, velocityModeLerp), Time.deltaTime * _rotationLerpSpeed);
            }
            else
            {
                _smoothedRotation = Quaternion.Lerp(_smoothedRotation, _trackedRigibody.rotation, Time.deltaTime * _rotationLerpSpeed);
            }
            
            transform.position = Vector3.Lerp(transform.position, _trackedRigibody.position + _smoothedRotation * _pointOffset, Time.deltaTime * _positionLerpSpeed);
            transform.rotation = _smoothedRotation * Quaternion.Euler(_eulerAnglesRotationOffset);
        }
    }
}
