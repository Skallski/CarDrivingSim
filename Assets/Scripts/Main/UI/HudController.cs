using Main.Car;
using TMPro;
using UnityEngine;
using UtilsToolbox.Utils.MultiSwitch;

namespace Main.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private MultiSwitch _digitalElementsSwitch;
        [SerializeField] private TextMeshProUGUI _speedLabel;
        [SerializeField] private TextMeshProUGUI _rpmLabel;
        [SerializeField] private MultiSwitch _gearSwitch;
        [SerializeField] private MultiSwitch _leftBlinkerSwitch;
        [SerializeField] private MultiSwitch _rightBlinkerSwitch;
        
        [Space]
        [SerializeField] private CarController _carController;

        private void OnEnable()
        {
            _carController.GetEngine().OnEngineStarted += OnEngineStarted;
            _carController.GetEngine().OnEngineStopped += OnEngineStopped;
        }

        private void OnDisable()
        {
            _carController.GetEngine().OnEngineStarted -= OnEngineStarted;
            _carController.GetEngine().OnEngineStopped -= OnEngineStopped;
        }

        private void Start()
        {
            _digitalElementsSwitch.SetState(false);
        }

        private void Update()
        {
            if (_speedLabel.gameObject.activeSelf)
            {
                _speedLabel.SetText($"{_carController.GetSpeed():F0} <size=10>KMH</size>");
            }

            if (_rpmLabel.gameObject.activeSelf)
            {
                _rpmLabel.SetText($"{_carController.GetEngineRpm():F0} <size=10>RPM</size>");
            }

            if (_gearSwitch.gameObject.activeSelf)
            {
                int newState = _carController.GetCurrentGear() + 1;
                if (_gearSwitch.State != newState)
                {
                    _gearSwitch.SetState(newState);
                }
            }

            UpdateBlinkers();
        }

        private void UpdateBlinkers()
        {
            if (_leftBlinkerSwitch == null || _rightBlinkerSwitch == null)
            {
                return;
            }
            
            UpdateSingleBlinker(_carController.GetBlinker(CarBlinker.Side.Left), ref _leftBlinkerSwitch);
            UpdateSingleBlinker(_carController.GetBlinker(CarBlinker.Side.Right), ref _rightBlinkerSwitch);

            void UpdateSingleBlinker(CarBlinker blinker, ref MultiSwitch carBlinkerSwitch)
            {
                if (blinker.IsOn)
                {
                    carBlinkerSwitch.SetState(blinker.IsLit);
                }
                else
                {
                    carBlinkerSwitch.SetState(false);
                }
            }
        }
        
        private void OnEngineStarted()
        {
            _digitalElementsSwitch.SetState(true);
        }
        
        private void OnEngineStopped()
        {
            _digitalElementsSwitch.SetState(false);
        }
    }
}