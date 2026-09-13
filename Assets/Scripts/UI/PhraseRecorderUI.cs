using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Modal;
using R3;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public sealed class PhraseRecorderUI : MonoBehaviour
    {
        private static readonly Color StandbyColor = AppColors.TeacherSide;
        private static readonly Color DisabledColor = Color.white;
        private static readonly Color RecordingDimColor = new(0.55f, 0.15f, 0.15f);
        private const float BlinkIntervalSec = 0.5f;

        private static readonly LocalizedMessage MicAccessFailedBody = new(
            japanese: "マイクへのアクセスが許可されていません。OSのプライバシー設定を確認してください。",
            english: "Microphone access is not allowed. Please check your OS privacy settings.");

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _recordToggleButton;
        [SerializeField] private GameObject _disabledGuide;
        [SerializeField] private GameObject _standbyGuide;

        private Coroutine _blinkCoroutine;

        private void Start()
        {
            PhraseRecorder.Instance.CanPlay
                .Subscribe(canPlay => _playButton.interactable = canPlay)
                .AddTo(this);

            PhraseRecorder.Instance.State
                .Subscribe(state => ApplyState(state))
                .AddTo(this);

            PhraseRecorder.Instance.IsPlaybackActive
                .Subscribe(isPlaying => _playButton.image.color = AppColors.ActiveOrWhite(isPlaying))
                .AddTo(this);

            PhraseRecorder.Instance.OnMicAccessFailed
                .Subscribe(_ => OnMicAccessFailed())
                .AddTo(this);

            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _recordToggleButton.onClick.AddListener(OnRecordToggleClicked);
        }

        private void ApplyState(RecordingState state)
        {
            StopBlink();
            _disabledGuide.SetActive(state == RecordingState.Disabled);
            _standbyGuide.SetActive(state == RecordingState.Standby);
            switch (state)
            {
                case RecordingState.Disabled:
                    _recordToggleButton.image.color = DisabledColor;
                    break;
                case RecordingState.Standby:
                    _recordToggleButton.image.color = StandbyColor;
                    break;
                case RecordingState.Recording:
                    _blinkCoroutine = StartCoroutine(Blink());
                    break;
            }
        }

        private IEnumerator Blink()
        {
            var wait = new WaitForSeconds(BlinkIntervalSec);
            while (true)
            {
                _recordToggleButton.image.color = StandbyColor;
                yield return wait;
                _recordToggleButton.image.color = RecordingDimColor;
                yield return wait;
            }
        }

        private void StopBlink()
        {
            if (_blinkCoroutine == null)
            {
                return;
            }
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        private void OnPlayButtonClicked()
        {
            PhraseRecorder.Instance.Play();
        }

        private void OnRecordToggleClicked()
        {
            PhraseRecorder.Instance.ToggleRecording();
        }

        private void OnMicAccessFailed()
        {
            ConfirmModalUI.Show(MicAccessFailedBody.ForCurrentLanguage());
        }
    }
}
