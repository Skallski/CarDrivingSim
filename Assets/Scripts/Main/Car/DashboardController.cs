using UnityEngine;

namespace Main.Car
{
    public class DashboardController : MonoBehaviour
    {
        [System.Serializable]
        public struct DialRemap
        {
            [field: SerializeField] public Transform DialTransform { get; set; }
            [field: SerializeField] public Vector2 OriginalValueRange { get; set; }
            [field: SerializeField] public Vector2 TransformRotationRangeY { get; set; }
        }
    
        [SerializeField] private CarInput _carInput;
        [SerializeField] private CarController _carController;
        [SerializeField] private CarBlinker _leftBlinker;
        [SerializeField] private CarBlinker _rightBlinker;
        [SerializeField] private CarBlinkerAudioPlayer _carBlinkerAudioPlayer;

        [Space]
        [SerializeField] private Renderer _iconTurnLightLeft;
        [SerializeField] private Renderer _iconTurnLightRight;
        [SerializeField] private Renderer _iconTurnHandbrake;

        [Space]
        [SerializeField] private DialRemap _dialSpeed;
        [SerializeField] private DialRemap _dialRpm;

        private void OnEnable()
        {
            _carInput.OnBlinkerInteracted += _leftBlinker.OnInteracted;
            _carInput.OnBlinkerInteracted += _rightBlinker.OnInteracted;
            
            _leftBlinker.OnBlink += _carBlinkerAudioPlayer.PlaySound;
            _rightBlinker.OnBlink += _carBlinkerAudioPlayer.PlaySound;
        }

        private void OnDisable()
        {
            _carInput.OnBlinkerInteracted -= _leftBlinker.OnInteracted;
            _carInput.OnBlinkerInteracted -= _rightBlinker.OnInteracted;
            
            _leftBlinker.OnBlink -= _carBlinkerAudioPlayer.PlaySound;
            _rightBlinker.OnBlink -= _carBlinkerAudioPlayer.PlaySound;
        }

        private void Start()
        {
            _iconTurnLightLeft.material = Instantiate(_iconTurnLightLeft.material);
            _iconTurnLightRight.material = Instantiate(_iconTurnLightRight.material);
            _iconTurnHandbrake.material = Instantiate(_iconTurnHandbrake.material);
        }

        private void Update()
        {
            UpdateBlinkers();
            UpdateDials();
        }

        private void UpdateBlinkers()
        {
            _leftBlinker.Update();
            _rightBlinker.Update();
            
            SetLightState(_iconTurnLightLeft, _leftBlinker.IsLit);
            SetLightState(_iconTurnLightRight, _rightBlinker.IsLit);
            
            static void SetLightState(Renderer iconRenderer, bool turnEnabled)
            {
                iconRenderer.sharedMaterial.SetColor("_EmissionColor", turnEnabled 
                    ? new Color(1, 1, 1) 
                    : new Color(0, 0, 0));
            }
        }

        private void UpdateDials()
        {
            SetDialRotation(_dialSpeed, _carController.GetSpeed());
            SetDialRotation(_dialRpm, _carController.GetEngineRpm());
        }

        private static void SetDialRotation(DialRemap dial, float value)
        {
            dial.DialTransform.localRotation = Quaternion.Euler(0,
                Remap(value, dial.OriginalValueRange.x, dial.OriginalValueRange.y, dial.TransformRotationRangeY.x,
                    dial.TransformRotationRangeY.y), 0);
        
            float Remap(float s, float a1, float a2, float b1, float b2)
            {
                return b1 + (s-a1)*(b2-b1)/(a2-a1);
            }
        }
    }
}
