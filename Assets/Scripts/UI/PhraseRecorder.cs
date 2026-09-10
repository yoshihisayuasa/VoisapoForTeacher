using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI.MelodyUI;
using R3;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class PhraseRecorder : MonoBehaviour
    {
        public static PhraseRecorder Instance { get; private set; }

        [SerializeField] private AudioSource _playbackSource;

        private MicrophoneCapture _capture = new(null);
        private AudioClip _capturedClip;

        public string[] AvailableDevices => Microphone.devices;

        private readonly ReactiveProperty<bool> _hasCapture = new(false);
        public Observable<bool> HasCapture => _hasCapture;

        private readonly ReactiveProperty<RecordingState> _state = new(RecordingState.Disabled);
        public Observable<RecordingState> State => _state;

        private readonly Subject<Unit> _onMicAccessFailed = new();
        public Observable<Unit> OnMicAccessFailed => _onMicAccessFailed;

        private readonly ReactiveProperty<bool> _isPlaybackActive = new(false);
        public Observable<bool> IsPlaybackActive => _isPlaybackActive;

        private Coroutine _playbackWatch;

        public void SelectDevice(string deviceName)
        {
            if (_capture.IsSameDevice(deviceName))
            {
                return;
            }
            // 開きっぱなしのマイクは切り替え前のデバイスを掴んでいるため、閉じて録音から降ろす
            _capture.Close();
            _capture = new MicrophoneCapture(deviceName);
            _state.Value = RecordingState.Disabled;
        }

        public void ToggleRecording()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                if (!_capture.Open())
                {
                    _onMicAccessFailed.OnNext(Unit.Default);
                    return;
                }
                _state.Value = RecordingState.Standby;
                return;
            }

            _capture.Close();
            _state.Value = RecordingState.Disabled;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            MelodyPlayer.Instance.OnMelodyBegan
                .Subscribe(_ => StartRecording())
                .AddTo(this);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(shouldCapture => OnPlayEnded(shouldCapture))
                .AddTo(this);
        }

        private void StartRecording()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                return;
            }
            _capture.MarkStart();
            _state.Value = RecordingState.Recording;
        }

        private void OnPlayEnded(bool shouldCapture)
        {
            if (_state.Value == RecordingState.Disabled)
            {
                return;
            }
            if (shouldCapture)
            {
                StartCoroutine(CaptureAfterDelay(0.5f));
            }
            else
            {
                _state.Value = RecordingState.Standby;
            }
        }

        private IEnumerator CaptureAfterDelay(float delaySec)
        {
            yield return new WaitForSeconds(delaySec);

            if (_state.Value == RecordingState.Disabled)
            {
                yield break;
            }
            _capturedClip = _capture.ExtractSinceStart();
            _hasCapture.Value = _capturedClip != null;
            _state.Value = RecordingState.Standby;
        }

        public void Play()
        {
            if (_capturedClip == null)
            {
                return;
            }
            _playbackSource.clip = _capturedClip;
            _playbackSource.Play();
            _isPlaybackActive.Value = true;

            if (_playbackWatch != null)
            {
                StopCoroutine(_playbackWatch);
            }
            _playbackWatch = StartCoroutine(WatchPlaybackEnd());
        }

        private IEnumerator WatchPlaybackEnd()
        {
            yield return new WaitWhile(() => _playbackSource.isPlaying);

            _isPlaybackActive.Value = false;
            _playbackWatch = null;
        }

        private void OnDestroy()
        {
            _capture.Close();
            _hasCapture.Dispose();
            _state.Dispose();
            _isPlaybackActive.Dispose();
            _onMicAccessFailed.Dispose();
        }
    }
}
