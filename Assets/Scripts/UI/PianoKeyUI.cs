using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Scripts.Domain;
using Scripts.Infrastructure;
using R3;
using static Scripts.UI.Piano.PianoKeyColors;

namespace Scripts.UI.Piano
{
    /// <summary>
    /// UI操作（UI層）
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PianoKeyUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        private readonly Subject<PianoNote> _onClickKeySubject = new();
        private readonly Subject<PianoNote> _onReleaseKeySubject = new();
        private readonly Subject<PianoNote> _onPointerEnterSubject = new();
        private readonly Subject<PianoNote> _onPointerExitEnterSubject = new();
        private readonly Subject<bool> _onTeacherFlagAsObservable = new();

        [SerializeField]
        [Tooltip("鍵盤の種別")]
        private PianoNoteEnum _keyEnum;

        [SerializeField]
        [Tooltip("鍵盤の音源")]
        private AudioClip _audioClip;

        [SerializeField]
        [Tooltip("鍵盤用AudioSource")]
        private AudioSource _audioSource;

        private KeyColorState _colorState = KeyColorState.Default;

        private Image _keyLabelBg;
        private Color _defaultColor;

        private PianoKeyDomain _domain;
        private PianoKeyInfrastructure _infra;

        private enum KeyColorState
        {
            Default,
            Playing,
            Played,
        }


        public Observable<PianoNote> OnClickKeyAsObservable => _onClickKeySubject;
        public Observable<PianoNote> OnReleaseKeyAsObservable => _onReleaseKeySubject;
        public Observable<PianoNote> OnPointerEnterAsObservable => _onPointerEnterSubject;
        public Observable<PianoNote> OnPointerExitAsObservable => _onPointerExitEnterSubject;
        public Observable<bool> OnTeacherFlagAsObservable => _onTeacherFlagAsObservable;

        private void Awake()
        {
            _keyLabelBg = GetComponent<Image>();
            if (_keyLabelBg != null)
            {
                _defaultColor = _keyLabelBg.color;
            }

            _domain = new PianoKeyDomain(_keyEnum);
            _infra = new PianoKeyInfrastructure(_audioSource, _audioClip, GetComponent<Photon.Pun.PhotonView>());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            bool isCtrlOrCmd =
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

            _onTeacherFlagAsObservable.OnNext(isCtrlOrCmd);

            if (!_domain.IsPressed)
            {
                _onClickKeySubject.OnNext(_domain.Key);
                _domain.Press();

                try
                {
                    _infra.SendPlayKey(_domain.Key);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Photon RPC failed: {ex.Message}");
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_domain.IsPressed)
            {
                _domain.Release();
                _onReleaseKeySubject.OnNext(_domain.Key);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPointerEnterSubject.OnNext(_domain.Key);
        }

        public void SetPlayingVisual()
        {
            if (_keyLabelBg == null)
            {
                return;
            }
            _keyLabelBg.color = Playing;
            _colorState = KeyColorState.Playing;
        }

        public void SetStoppedVisual(bool setPlayedColor)
        {
            if (_keyLabelBg == null)
            {
                return;
            }
            if (setPlayedColor)
            {
                _keyLabelBg.color = SetPlayedColor(_domain.Key.IsSharp);
                _colorState = KeyColorState.Played;
            }
            else
            {
                _keyLabelBg.color = _defaultColor;
                _colorState = KeyColorState.Default;
            }
        }

        // 音のみ再生
        public void PlaySound(float volume)
        {
            _infra.PlaySound(volume);
        }

        // 音のみ停止（フェードアウト）
        public void StopSound(float fadeOutDuration = 0.5f)
        {
            _infra.StopSound(fadeOutDuration);
        }

        // 統合版
        public void SetMaxHighlightColor()
        {
            if (_keyLabelBg == null) return;
            _keyLabelBg.color = SetMaxColor(_domain.Key.IsSharp);
        }

        public void SetMinHighlightColor()
        {
            if (_keyLabelBg == null) return;
            _keyLabelBg.color = SetMinColor(_domain.Key.IsSharp);
        }

        public void ResetHighlightedColor()
        {
            if (_keyLabelBg == null)
            {
                return;
            }

            switch (_colorState)
            {
                case KeyColorState.Playing:
                    _keyLabelBg.color = Playing;
                    break;
                case KeyColorState.Played:
                    _keyLabelBg.color = SetPlayedColor(_domain.Key.IsSharp);
                    break;
                case KeyColorState.Default:
                default:
                    _keyLabelBg.color = _defaultColor;
                    break;
            }
        }
    }
}