using UnityEngine;

namespace Main.Camera
{
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

        [SerializeField] private bool firstPersonMode = false;

        private void LateUpdate()
        {
            if (firstPersonMode==false)
            {
                if (_trackedRigibody)
                {
                    if (_useVelocity)
                    {
                        float velocityMagnitude = _trackedRigibody.velocity.magnitude;
                        float velocityModeLerp = Mathf.Clamp01(velocityMagnitude * 0.25f - 0.5f);

                        if (velocityMagnitude > 0.5f)
                        {
                            Quaternion rotationBasedOnVelocity = Quaternion.LookRotation(_trackedRigibody.velocity.normalized, Vector3.up);
                            _smoothedRotation = Quaternion.Lerp(_smoothedRotation, Quaternion.Lerp(_trackedRigibody.transform.rotation, rotationBasedOnVelocity, velocityModeLerp), Time.deltaTime * _rotationLerpSpeed);
                        }
                        else
                        {
                            _smoothedRotation = Quaternion.Lerp(_smoothedRotation, _trackedRigibody.transform.rotation, Time.deltaTime * _rotationLerpSpeed);
                        }
                    }
                    else
                    {
                        _smoothedRotation = Quaternion.Lerp(_smoothedRotation, _trackedRigibody.transform.rotation, Time.deltaTime * _rotationLerpSpeed);
                    }

                    transform.position = Vector3.Lerp(transform.position, _trackedRigibody.transform.position + _smoothedRotation * _pointOffset, Time.deltaTime * _positionLerpSpeed);
                    transform.rotation = _smoothedRotation * Quaternion.Euler(_eulerAnglesRotationOffset);
                }
            }
            else //first person mode
            {
                if (_useVelocity)
                {
                    float velocityMagnitude = _trackedRigibody.velocity.magnitude;
                    float velocityModeLerp = Mathf.Clamp01(velocityMagnitude * 0.25f - 0.5f);

                    if (velocityMagnitude > 0.5f)
                    {
                        Quaternion rotationBasedOnVelocity = Quaternion.LookRotation(_trackedRigibody.velocity.normalized, Vector3.up);
                        _smoothedRotation = Quaternion.Lerp(_smoothedRotation, Quaternion.Lerp(_trackedRigibody.transform.rotation, rotationBasedOnVelocity, velocityModeLerp), Time.deltaTime * _rotationLerpSpeed);
                    }
                    else
                    {
                        _smoothedRotation = Quaternion.Lerp(_smoothedRotation, _trackedRigibody.transform.rotation, Time.deltaTime * _rotationLerpSpeed);
                    }
                }
                else
                {
                    _smoothedRotation = Quaternion.Lerp(_smoothedRotation, _trackedRigibody.transform.rotation, Time.deltaTime * _rotationLerpSpeed);
                }

                transform.position = _trackedRigibody.transform.position + _smoothedRotation * _pointOffset;
                transform.rotation = _smoothedRotation * Quaternion.Euler(_eulerAnglesRotationOffset);
                
            }
            
        }
    }
}
