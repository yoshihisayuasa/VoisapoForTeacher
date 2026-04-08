using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public sealed class PhraseRecorderUI : MonoBehaviour
    {
        private static readonly Color RecordingOnColor  = AppColors.TeacherSide;
        private static readonly Color RecordingOffColor = new(0.5f, 0.5f, 0.5f);

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _recordToggleButton;

        private void Start()
        {
            _playButton.interactable = false;

            PhraseRecorder.Instance.HasCapture
                .Subscribe(hasCapture => _playButton.interactable = hasCapture)
                .AddTo(this);

            PhraseRecorder.Instance.IsRecordingEnabled
                .Subscribe(enabled => _recordToggleButton.image.color = enabled ? RecordingOnColor : RecordingOffColor)
                .AddTo(this);

            PhraseRecorder.Instance.OnMicAccessFailed
                .Subscribe(_ => OnMicAccessFailed())
                .AddTo(this);

            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _recordToggleButton.onClick.AddListener(OnRecordToggleClicked);
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
            SimpleModalWindow.Create(ignorable: false)
                .SetHeader("Microphone Access Denied")
                .SetBody("Microphone access is not allowed. Please check your OS privacy settings.")
                .AddButton("OK", () => { }, ModalButtonType.Success)
                .Show();
        }
    }
}
