using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Rigidbody TrackedRigibody;
    [SerializeField] private Vector3 _pointOffset;
    [SerializeField] private Vector3 _eulerAnglesRotationOffset;

    [SerializeField] private float _positionLerpSpeed = 5f;
    [SerializeField] private float _rotationLerpSpeed = 5f;

    private Quaternion smoothedRotation = Quaternion.identity;

    public bool useVelocity = true;

    // Update is called once per frame
    void Update()
    {
        if (TrackedRigibody)
        {
            if (useVelocity)
            {
                float velocityMagnitude = TrackedRigibody.velocity.magnitude;

                float velocityModeLerp = Mathf.Clamp01(velocityMagnitude*0.25f - 0.5f);
                Quaternion rotationBasedOnVelocity = Quaternion.LookRotation(TrackedRigibody.velocity.normalized, Vector3.up);

                smoothedRotation = Quaternion.Lerp(smoothedRotation, Quaternion.Lerp(TrackedRigibody.rotation, rotationBasedOnVelocity, velocityModeLerp), Time.deltaTime * _rotationLerpSpeed);
            }
            else
            {
                smoothedRotation = Quaternion.Lerp(smoothedRotation, TrackedRigibody.rotation, Time.deltaTime * _rotationLerpSpeed);
            }
            

            transform.position = Vector3.Lerp(transform.position, TrackedRigibody.position + smoothedRotation * _pointOffset, Time.deltaTime * _positionLerpSpeed);
            transform.rotation = smoothedRotation * Quaternion.Euler(_eulerAnglesRotationOffset);

            
        }
    }
}
