using AsseScripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AsseScripts.Domain;
using AsseScripts.Infrastructure;
using R3;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// UI操作（UI層）
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PianoKeyUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {

        private readonly Subject<PianoNote> _onClickKeySubject = new();
        private readonly Subject<PianoNote> _onPointerUpSubject = new();
        private readonly Subject<PianoNote> _onPointerEnterSubject = new();

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

        private PianoKeyDomain _domain;
        private PianoKeyInfrastructure _infra;

        private enum KeyLogicalState  { Default, Playing, Played }
        private enum KeyHighlightState { None, Min, Max }


        public PianoNoteEnum NoteEnum => _keyEnum;

        public Observable<PianoNote> OnClickKeyAsObservable => _onClickKeySubject;
        public Observable<PianoNote> OnPointerUpAsObservable => _onPointerUpSubject;
        public Observable<PianoNote> OnPointerEnterAsObservable => _onPointerEnterSubject;

        private void Awake()
        {
            _keyLabelBg = GetComponent<Image>();
            if (_keyLabelBg != null)
            {
                _defaultColor = _keyLabelBg.color;
            }

            _domain = new PianoKeyDomain(_keyEnum);
            _infra = new PianoKeyInfrastructure(_audioSource, null, GetComponent<Photon.Pun.PhotonView>());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _onClickKeySubject.OnNext(_domain.Key);
            try
            {
                _infra.SendPlayKey(_domain.Key);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Photon RPC failed: {ex.Message}");
            }
        }
   
        public void OnPointerUp(PointerEventData eventData)
        {
            _onPointerUpSubject.OnNext(_domain.Key);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPointerEnterSubject.OnNext(_domain.Key);
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
            _infra.PlaySound(volume * SoundSourceSwitcher.Instance.CurrentVolumeMultiplier);
        }

        // 音のみ停止（フェードアウト）
        public void StopSound()
        {
            float fadeOutDuration = BPMManager.Instance.SecondPerBeat * 0.6f;
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

        private void UpdateVisual()
        {
            if (_logicalState == KeyLogicalState.Playing)
            {
                _keyLabelBg.color = AppColors.PianoKeyPlaying;
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