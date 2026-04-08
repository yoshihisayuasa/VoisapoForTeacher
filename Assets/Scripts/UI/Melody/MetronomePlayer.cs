using UnityEngine;

namespace Assets.Scripts.UI.Melody
{
    public sealed class MetronomePlayer
    {
        private readonly AudioSource _audioSource;
        private readonly AudioClip _clip;

        public MetronomePlayer(AudioSource audioSource, AudioClip clip)
        {
            _audioSource = audioSource;
            _clip = clip;
        }

        public void PlayOneShot(float volume)
        {
            _audioSource.PlayOneShot(_clip, volume);
        }

        public void Stop()
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }
    }
}
