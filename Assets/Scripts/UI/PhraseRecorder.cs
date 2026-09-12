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
        private readonly ReactiveProperty<AudioClip> _capturedClip = new(null);

        public string[] AvailableDevices => Microphone.devices;

        private readonly ReactiveProperty<RecordingState> _state = new(RecordingState.Disabled);
        public Observable<RecordingState> State => _state;

        /// <summary>
        /// 再生できるか。録音中は、これから上書きされるクリップを聴くことになるため押させない。
        /// </summary>
        public Observable<bool> CanPlay =>
            _capturedClip.CombineLatest(_state, (clip, state) =>
                clip != null && state != RecordingState.Recording);

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
                .Subscribe(_ => StartPhrase())
                .AddTo(this);

            MelodyPlayer.Instance.OnMelodyEnded
                .Subscribe(_ => EndPhrase())
                .AddTo(this);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(_ => EndSession())
                .AddTo(this);
        }

        /// <summary>
        /// 1フレーズの録音を始める。自動転調では周ごとに呼ばれ、そのたびに録り直しになる。
        /// </summary>
        private void StartPhrase()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                return;
            }
            _capture.MarkStart();
            _state.Value = RecordingState.Recording;
        }

        /// <summary>1フレーズを弾き終えた。ここまでを切り出して録音済みにする。</summary>
        private void EndPhrase()
        {
            if (_state.Value != RecordingState.Recording)
            {
                return;
            }
            Capture();
        }

        /// <summary>再生が終わった。録音は待機に戻す。</summary>
        private void EndSession()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                return;
            }
            _state.Value = RecordingState.Standby;
        }

        // 切り出せなかったときは録音済みの中身を触らない（直前のフレーズを残す）。
        private void Capture()
        {
            var clip = _capture.ExtractSinceStart();
            if (clip == null)
            {
                return;
            }
            _capturedClip.Value = clip;
        }

        public void Play()
        {
            var clip = _capturedClip.Value;
            if (clip == null)
            {
                return;
            }
            _playbackSource.clip = clip;
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
            _capturedClip.Dispose();
            _state.Dispose();
            _isPlaybackActive.Dispose();
            _onMicAccessFailed.Dispose();
        }
    }
}
