using UnityEngine;

namespace Main.Car
{
    public class CarBlinkerAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _onSfx;
        [SerializeField] private AudioClip _offSfx;
        [SerializeField] private AudioSource _audioSource;

        public void PlaySound(bool isOn)
        {
            _audioSource.PlayOneShot(isOn ? _onSfx : _offSfx);
        }
    }
}