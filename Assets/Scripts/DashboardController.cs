using System.Collections;
using System.Collections.Generic;
using Main.Car;
using UnityEngine;

public class DashboardController : MonoBehaviour
{
    [System.Serializable]
    public class DialRemap
    {
        public Transform DialTransform;
        public Vector2 originalValueRange;
        public Vector2 transformRotationRangeY;
    }
    
    [SerializeField] private CarController carController;
    [SerializeField] private Renderer _iconTurnLightLeft;
    [SerializeField] private Renderer _iconTurnLightRight;
    
    [SerializeField] private Renderer _iconTurnHandbrake;

    [SerializeField] private DialRemap _dialSpeed;
    [SerializeField] private DialRemap _dialRpm;
    
    void Start()
    {
        _iconTurnLightLeft.material = Instantiate(_iconTurnLightLeft.material);
        _iconTurnLightRight.material = Instantiate(_iconTurnLightRight.material);
        _iconTurnHandbrake.material = Instantiate(_iconTurnHandbrake.material);
    }

    // Update is called once per frame
    void Update()
    {
        SetLightState(_iconTurnLightLeft,carController.GetBlinker(CarBlinker.Side.Left).IsLit);
        SetLightState(_iconTurnLightRight,carController.GetBlinker(CarBlinker.Side.Right).IsLit);
        
        SetDialRotation(_dialSpeed,carController.GetSpeed());
        SetDialRotation(_dialRpm,carController.GetEngineRpm());
    }
    
    
    public void SetLightState(Renderer iconRenderer, bool turnEnabled)
    {
        if (turnEnabled)
        {
            iconRenderer.sharedMaterial.SetColor("_EmissionColor",new Color(1,1,1));
        }
        else
        {
            iconRenderer.sharedMaterial.SetColor("_EmissionColor",new Color(0,0,0));
        }
    }

    public void SetDialRotation(DialRemap dial, float value)
    {
        dial.DialTransform.localRotation = Quaternion.Euler(0,remap(value, dial.originalValueRange.x, dial.originalValueRange.y,dial.transformRotationRangeY.x,dial.transformRotationRangeY.y),0);
    }
    
    float remap(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }
}
