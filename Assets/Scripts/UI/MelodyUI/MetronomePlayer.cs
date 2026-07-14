using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI
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

        public void PlayOneShot(Volume volume)
        {
            _audioSource.PlayOneShot(_clip, volume.Value);
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
