using Main.Car;
using TMPro;
using UnityEngine;
using UtilsToolbox.Utils.MultiSwitch;

namespace Main.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _speedLabel;
        [SerializeField] private TextMeshProUGUI _rpmLabel;
        [SerializeField] private MultiSwitch _gearSwitch;
        [SerializeField] private MultiSwitch _leftBlinkerSwitch;
        [SerializeField] private MultiSwitch _rightBlinkerSwitch;
        
        [Space]
        [SerializeField] private CarController _carController;

        private void Update()
        {
            _speedLabel.SetText($"{_carController.GetSpeed():F0} <size=10>KMH</size>");
            _rpmLabel.SetText($"{_carController.GetEngineRpm():F0} <size=10>RPM</size>");

            int newState = _carController.GetCurrentGear() + 1;
            if (_gearSwitch.State != newState)
            {
                _gearSwitch.SetState(newState);
            }
        }
    }
}