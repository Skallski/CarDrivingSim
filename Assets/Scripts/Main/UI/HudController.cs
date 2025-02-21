using Main.Car;
using TMPro;
using UnityEngine;
using UtilsToolbox.Utils.MultiSwitch;

namespace Main.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private CarController _carController;
        [Space]
        [SerializeField] private TextMeshProUGUI _speedLabel;
        [SerializeField] private TextMeshProUGUI _rpmLabel;
        [SerializeField] private MultiSwitch _gearSwitch;

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
        }
    }
}