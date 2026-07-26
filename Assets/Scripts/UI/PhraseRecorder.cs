using Assets.Scripts.Domain.StaticValues;
using Assets.Scripts.Domain.ValueObjects;
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

        private AudioClip _micClip;
        private string _selectedDevice = null;

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
            if (Microphone.IsRecording(_selectedDevice))
            {
                Microphone.End(_selectedDevice);
            }
            _selectedDevice = deviceName;
        }

        public void ToggleRecording()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                if (Microphone.devices.Length == 0)
                {
                    _onMicAccessFailed.OnNext(Unit.Default);
                    return;
                }
                _state.Value = RecordingState.Standby;
                return;
            }

            if (_state.Value == RecordingState.Recording)
            {
                Microphone.End(_selectedDevice);
            }
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
            try
            {
                _micClip = Microphone.Start(_selectedDevice, false, RecordingRules.MaxPhraseSec, RecordingRules.SampleRate);
            }
            catch (System.Exception)
            {
                _micClip = null;
            }
            if (_micClip == null)
            {
                _state.Value = RecordingState.Disabled;
                _onMicAccessFailed.OnNext(Unit.Default);
                return;
            }
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
                Microphone.End(_selectedDevice);
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
            int recordedSamples = Microphone.GetPosition(_selectedDevice);
            Microphone.End(_selectedDevice);
            _micClip = TrimToRecordedLength(_micClip, recordedSamples);
            _hasCapture.Value = _micClip != null;
            _state.Value = RecordingState.Standby;
        }

        private static AudioClip TrimToRecordedLength(AudioClip source, int recordedSamples)
        {
            if (source == null)
            {
                return null;
            }
            if (recordedSamples <= 0 || recordedSamples >= source.samples)
            {
                // 上限まで録りきってマイクが自動停止した場合はGetPositionが0を返すため、全長をそのまま採用する
                return source;
            }
            var samples = new float[recordedSamples * source.channels];
            source.GetData(samples, 0);

            var trimmed = AudioClip.Create(source.name, recordedSamples, source.channels, source.frequency, false);
            trimmed.SetData(samples, 0);
            return trimmed;
        }

        public void Play()
        {
            if (_micClip == null)
            {
                return;
            }
            _playbackSource.clip = _micClip;
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
            Microphone.End(_selectedDevice);
            _hasCapture.Dispose();
            _state.Dispose();
            _isPlaybackActive.Dispose();
            _onMicAccessFailed.Dispose();
        }
    }
}
