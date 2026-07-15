using Assets.Scripts.UI;
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

        /// <summary>
        /// 現在の音量設定で1回鳴らす。鍵盤と違い音源別の倍率は掛けない。
        /// </summary>
        public void PlayOneShot()
        {
            _audioSource.PlayOneShot(_clip, VolumeManager.Instance.Volume.Value);
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
