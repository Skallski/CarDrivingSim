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
        
        [Space]
        [SerializeField] private CarController _carController;

        private void Update()
        {
            _speedLabel.SetText($"{_carController.GetSpeed():F0} km/h");
            _rpmLabel.SetText($"{_carController.GetEngineRpm():F0} rpm");

            int newState = _carController.GetCurrentGear() + 1;
            if (_gearSwitch.State != newState)
            {
                _gearSwitch.SetState(newState);
            }
        }
    }
}