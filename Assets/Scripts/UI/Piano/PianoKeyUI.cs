using AsseScripts.UI;
using UnityEngine;
using UnityEngine.UI;
using AsseScripts.Domain;
using AsseScripts.Infrastructure;

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

        private KeyLogicalState _logicalState = KeyLogicalState.Default;
        private KeyHighlightState _highlightState = KeyHighlightState.None;

        private Image _keyLabelBg;
        private Color _defaultColor;

        private PianoKeyInfrastructure _infra;

        private enum KeyLogicalState  { Default, Playing, Played }
        private enum KeyHighlightState { None, Min, Max, Accent }


        public PianoNoteEnum NoteEnum => _keyEnum;

        private void Awake()
        {
            _keyLabelBg = GetComponent<Image>();
            if (_keyLabelBg != null)
            {
                _defaultColor = _keyLabelBg.color;
            }

            _infra = new PianoKeyInfrastructure(_audioSource, null);
        }

        public void SetPlayingVisual()
        {
            if (_keyLabelBg == null) return;
            _logicalState = KeyLogicalState.Playing;
            UpdateVisual();
        }

        public void SetKeyVisual(bool setPlayedColor)
        {
            if (_keyLabelBg == null) return;
            _logicalState = setPlayedColor ? KeyLogicalState.Played : KeyLogicalState.Default;
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

        public void SetMaxHighlightColor()
        {
            _highlightState = KeyHighlightState.Max;
            UpdateVisual();
        }

        public void SetMinHighlightColor()
        {
            _highlightState = KeyHighlightState.Min;
            UpdateVisual();
        }

        public void ResetHighlightedColor()
        {
            _highlightState = KeyHighlightState.None;
            UpdateVisual();
        }

        public void SetAccentColor()
        {
            _highlightState = KeyHighlightState.Accent;
            UpdateVisual();
        }

        public void ResetAccentColor()
        {
            _highlightState = KeyHighlightState.None;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_logicalState == KeyLogicalState.Playing)
            {
                _keyLabelBg.color = AppColors.PianoKeyPlaying;
                return;
            }

            if (_highlightState == KeyHighlightState.Accent)
            {
                _keyLabelBg.color = AppColors.Accent;
                return;
            }

            if (_highlightState == KeyHighlightState.Min)
            {
                _keyLabelBg.color = AppColors.PianoKeyMin;
                return;
            }

            if (_highlightState == KeyHighlightState.Max)
            {
                _keyLabelBg.color = AppColors.PianoKeyMax;
                return;
            }

            _keyLabelBg.color = _logicalState == KeyLogicalState.Played
                ? AppColors.PianoKeyPlayed
                : _defaultColor;
        }
    }
}