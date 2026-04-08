using Photon.Pun;
using UnityEngine;
using AsseScripts.Domain;
using Cysharp.Threading.Tasks;

namespace AsseScripts.Infrastructure
{
    /// <summary>
    /// 音源再生・Photon通信（インフラ層）
    /// </summary>
    public class PianoKeyInfrastructure
    {
        private readonly AudioSource _audioSource;
        private readonly PhotonView _photonView;
        private System.Threading.CancellationTokenSource _fadeOutCts;
        
        /// <summary>
        /// フェードアウト中かどうかを管理するフラグ
        /// </summary>

        public PianoKeyInfrastructure(AudioSource audioSource, AudioClip audioClip, PhotonView photonView)
        {
            _audioSource = audioSource;
            _audioSource.clip = audioClip;
            _photonView = photonView;
        }

        public void SwapClip(AudioClip newClip)
        {
            _audioSource.clip = newClip;
        }

        public void PlaySound(float volume)
        {
            // 新しい音の再生時はフェードアウトをキャンセル
            if(_fadeOutCts != null)
            {
                _fadeOutCts.Cancel();
                _fadeOutCts.Dispose();
                _fadeOutCts = null;

            }
            _audioSource.volume = volume;
            _audioSource.Play();
        }

        public void StopSound(float fadeOutDuration)
        {
            if (_fadeOutCts!=null) return;
            _fadeOutCts = new System.Threading.CancellationTokenSource();
            FadeOutAndStopAsync(_audioSource, fadeOutDuration, _fadeOutCts.Token).Forget();
        }

        private async UniTaskVoid FadeOutAndStopAsync(AudioSource source, float duration, System.Threading.CancellationToken token)
        {
            float startVolume = source.volume;
            float time = 0.0f;
            while (time < duration)
            {
               time += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, Mathf.SmoothStep(0f, 1f, time / duration));
                await UniTask.Yield(PlayerLoopTiming.Update, token);//次のフレームまで待機
            }

            source.volume = 0f;
            source.Stop();
            _fadeOutCts?.Dispose();
            _fadeOutCts = null;

        }
        public void SendPlayKey(PianoNote note)
        {
            _photonView?.RPC("PlayKeyReciver", RpcTarget.Others, note);
        }
    }
}