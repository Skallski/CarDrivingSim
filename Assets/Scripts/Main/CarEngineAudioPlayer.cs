using UnityEngine;

namespace Main
{
    public class CarEngineAudioPlayer : MonoBehaviour
    {
        [System.Serializable]
        private class EngineAudio
        {
            [field: SerializeField] public AudioSource AudioSource { get; set; }
            [field: SerializeField] public float CentreFrequency { get; set; }
            public float BasePitch { get; internal set; }
        }

        [SerializeField] private EngineAudio[] _audios;
        [field: SerializeField] public float TargetPitch { get; set; }
        [field: SerializeField] public float TargetThrottle { get; set; }
        [SerializeField] private float _volumeMultiplier = 0.1f;
        [SerializeField] private float _volumeMultiplierOnThrottle = 0.2f;

        public void Play()
        {
            foreach (EngineAudio engineAudio in _audios)
            {
                engineAudio.BasePitch = 1f / engineAudio.CentreFrequency;
                
                AudioSource audioSource = engineAudio.AudioSource;
                audioSource.Play();
                audioSource.loop = true;
            }
        }

        public void Stop()
        {
            foreach (EngineAudio engineAudio in _audios)
            {
                engineAudio.AudioSource.Stop();
            }
        }

        private void Update()
        {
            for (int i = 0; i < _audios.Length; i++)
            {
                EngineAudio engineAudio = _audios[i];
                AudioSource audioSource = engineAudio.AudioSource;
                audioSource.pitch = TargetPitch * engineAudio.BasePitch;

                float volume = 1;

                if (i == 0)
                {
                    if (TargetPitch > engineAudio.CentreFrequency)
                    {
                        volume = CalculateVolume(engineAudio, _audios[i + 1]);
                    }
                }
                else if (i == _audios.Length - 1)
                {
                    if (TargetPitch > engineAudio.CentreFrequency)
                    {
                        volume = CalculateVolume(engineAudio, _audios[i - 1]);
                    }
                }
                else
                {
                    volume = CalculateVolume(engineAudio, TargetPitch > engineAudio.CentreFrequency 
                        ? _audios[i + 1] 
                        : _audios[i - 1]);
                }

                audioSource.volume = volume * Mathf.Lerp(_volumeMultiplier,_volumeMultiplierOnThrottle,TargetThrottle);
            }
        }

        private float CalculateVolume(EngineAudio first, EngineAudio second)
        {
            return Mathf.Clamp01(1f - Mathf.InverseLerp(first.CentreFrequency,
                second.CentreFrequency, TargetPitch));
        }
    }
}