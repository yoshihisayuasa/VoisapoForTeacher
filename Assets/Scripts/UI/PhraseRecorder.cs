using Assets.Scripts.UI.Melody;
using R3;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class PhraseRecorder : MonoBehaviour
    {
        private const int MaxPhraseSec = 30;
        private const int SampleRate = 44100;

        public static PhraseRecorder Instance { get; private set; }

        [SerializeField] private AudioSource _playbackSource;

        private AudioClip _micClip;
        private string _selectedDevice = null;

        public string[] AvailableDevices => Microphone.devices;

        public ReactiveProperty<bool> HasCapture { get; } = new(false);

        private readonly ReactiveProperty<bool> _isRecordingEnabled = new(false);
        public Observable<bool> IsRecordingEnabled => _isRecordingEnabled;

        private readonly Subject<Unit> _onMicAccessFailed = new();
        public Observable<Unit> OnMicAccessFailed => _onMicAccessFailed;

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
            bool newValue = !_isRecordingEnabled.Value;
            if (newValue && Microphone.devices.Length == 0)
            {
                return;
            }
            _isRecordingEnabled.Value = newValue;
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
            if (!_isRecordingEnabled.Value)
            {
                return;
            }
            _micClip = Microphone.Start(_selectedDevice, false, MaxPhraseSec, SampleRate);
            if (_micClip == null)
            {
                _isRecordingEnabled.Value = false;
                _onMicAccessFailed.OnNext(Unit.Default);
            }
        }

        private void OnPlayEnded(bool shouldCapture)
        {
            if (!_isRecordingEnabled.Value)
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
            }
        }

        private IEnumerator CaptureAfterDelay(float delaySec)
        {
            yield return new WaitForSeconds(delaySec);

            Microphone.End(_selectedDevice);
            HasCapture.Value = _micClip != null;
        }

        public void Play()
        {
            if (_micClip == null)
            {
                return;
            }
            _playbackSource.clip = _micClip;
            _playbackSource.Play();
        }

        private void OnDestroy()
        {
            Microphone.End(_selectedDevice);
            HasCapture.Dispose();
            _onMicAccessFailed.Dispose();
        }
    }
}
