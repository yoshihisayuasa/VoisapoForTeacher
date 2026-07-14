using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// UI操作（UI層）
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class PianoKeyUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("鍵盤の種別")]
        private PianoNoteEnum _keyEnum;

        [SerializeField]
        [Tooltip("鍵盤用AudioSource")]
        private AudioSource _audioSource;

        private PlaybackState _playbackState = PlaybackState.NotPlayed;
        private RangeEdge _rangeEdge = RangeEdge.None;
        private bool _isSelected;

        private Image _keyLabelBg;
        private Color _defaultColor;

        private PianoKeyInfrastructure _infra;

        private enum PlaybackState { NotPlayed, Playing, Played }
        private enum RangeEdge { None, Min, Max }


        public PianoNoteEnum NoteEnum => _keyEnum;

        private void Awake()
        {
            _keyLabelBg = GetComponent<Image>();
            _defaultColor = _keyLabelBg.color;

            _infra = new PianoKeyInfrastructure(_audioSource, null);
        }

        public void SetPlayingVisual()
        {
            _playbackState = PlaybackState.Playing;
            UpdateVisual();
        }

        public void SetKeyVisual(bool setPlayedColor)
        {
            _playbackState = setPlayedColor ? PlaybackState.Played : PlaybackState.NotPlayed;
            UpdateVisual();
        }

        public void SwapAudioClip(AudioClip newClip)
        {
            _infra.SwapClip(newClip);
        }

        // 音のみ再生
        public void PlaySound(float volume)
        {
            _infra.PlaySound(volume);
        }

        // 音のみ停止（フェードアウト）
        public void StopSound(float fadeOutDuration)
        {
            _infra.StopSound(fadeOutDuration);
        }

        public void MarkAsRangeMax()
        {
            _rangeEdge = RangeEdge.Max;
            UpdateVisual();
        }

        public void MarkAsRangeMin()
        {
            _rangeEdge = RangeEdge.Min;
            UpdateVisual();
        }

        public void ClearRangeEdge()
        {
            _rangeEdge = RangeEdge.None;
            UpdateVisual();
        }

        public void Select()
        {
            _isSelected = true;
            UpdateVisual();
        }

        public void Deselect()
        {
            _isSelected = false;
            UpdateVisual();
        }

        /// <summary>
        /// 「選択（根音）」と「範囲の端（Min/Max）」は独立した状態として保持し、
        /// 同じ鍵盤に重なったときの優先順位はここで一元的に解決する。
        /// 選択が外れれば、残っている状態（Min/Max等）の色が自然に現れる。
        /// </summary>
        private void UpdateVisual()
        {
            if (_playbackState == PlaybackState.Playing)
            {
                _keyLabelBg.color = AppColors.PianoKeyPlaying;
                return;
            }

            if (_isSelected)
            {
                _keyLabelBg.color = AppColors.Accent;
                return;
            }

            if (_rangeEdge == RangeEdge.Min)
            {
                _keyLabelBg.color = AppColors.PianoKeyMin;
                return;
            }

            if (_rangeEdge == RangeEdge.Max)
            {
                _keyLabelBg.color = AppColors.PianoKeyMax;
                return;
            }

            _keyLabelBg.color = _playbackState == PlaybackState.Played
                ? AppColors.PianoKeyPlayed
                : _defaultColor;
        }
    }
}