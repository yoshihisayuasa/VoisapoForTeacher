using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// 鍵盤入力時のプレビュー音を一定時間だけ鳴らす再生ロジック。
    /// コルーチンの実行はホストの MonoBehaviour に委譲する。
    /// </summary>
    public sealed class NotePreviewPlayer
    {
        private const float SoundDuration = 0.5F;

        private readonly MonoBehaviour _coroutineHost;
        private Coroutine _stopCoroutine;
        private PianoNote _playingNote;

        public NotePreviewPlayer(MonoBehaviour coroutineHost)
        {
            _coroutineHost = coroutineHost;
        }

        /// <summary>
        /// 鳴っている前のプレビュー音を止めてから、指定鍵盤の音を一定時間鳴らす。
        /// </summary>
        public void Play(PianoNote key)
        {
            StopCurrent();

            _playingNote = key;
            PianoController.Instance.Play(key, true, VolumeManager.Instance.Volume);
            _stopCoroutine = _coroutineHost.StartCoroutine(StopAfterDelay(key));
        }

        private void StopCurrent()
        {
            if (_stopCoroutine == null)
            {
                return;
            }

            _coroutineHost.StopCoroutine(_stopCoroutine);
            _stopCoroutine = null;

            if (_playingNote != null)
            {
                PianoController.Instance.Stop(_playingNote);
                _playingNote = null;
            }
        }

        private IEnumerator StopAfterDelay(PianoNote key)
        {
            yield return new WaitForSeconds(SoundDuration);
            PianoController.Instance.Stop(key);
            _stopCoroutine = null;
            _playingNote = null;
        }
    }
}
