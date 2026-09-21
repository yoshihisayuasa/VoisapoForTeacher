using Assets.Scripts.Domain.StaticValues;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
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
        private CaptureStart _phraseStart;
        private readonly ReactiveProperty<RecordedPhrase> _recordedPhrase = new(null);

        public string[] AvailableDevices => Microphone.devices;

        private readonly ReactiveProperty<RecordingState> _state = new(RecordingState.Disabled);
        public Observable<RecordingState> State => _state;

        /// <summary>
        /// 再生できるか。録音中は、これから上書きされるクリップを聴くことになるため押させない。
        /// </summary>
        public Observable<bool> CanPlay =>
            _recordedPhrase.CombineLatest(_state, (phrase, state) =>
                phrase != null && state != RecordingState.Recording);

        private readonly Subject<Unit> _onMicAccessFailed = new();
        public Observable<Unit> OnMicAccessFailed => _onMicAccessFailed;

        private readonly ReactiveProperty<bool> _isPlaybackActive = new(false);
        public Observable<bool> IsPlaybackActive => _isPlaybackActive;

        private Coroutine _playbackRoutine;
        private Coroutine _sessionEndRoutine;

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
                .Subscribe(phrase => EndPhrase(phrase))
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
            CancelSessionEnd();
            _phraseStart = _capture.MarkStart();
            _state.Value = RecordingState.Recording;
        }

        /// <summary>
        /// 1フレーズを弾き終えた。Zoom 越しに遅れて届く歌い終わりまで待ってから切り出す。
        /// 起点はここで確定させて渡すため、待っている間に次のフレーズが始まっても影響しない。
        /// </summary>
        private void EndPhrase(PlayedPhrase phrase)
        {
            if (_state.Value != RecordingState.Recording)
            {
                return;
            }
            StartCoroutine(CaptureAfterTail(_phraseStart, phrase));
        }

        // 切り出せなかったときは録音済みの中身を触らない（直前のフレーズを残す）。
        // テンポは録音した時点のものを持ち帰る。あとで変えられても鍵盤の動きが声からずれない。
        private IEnumerator CaptureAfterTail(CaptureStart start, PlayedPhrase phrase)
        {
            yield return new WaitForSeconds(RecordingRules.CaptureTailSec);

            var clip = start.ExtractUntilNow();
            if (clip == null)
            {
                yield break;
            }
            _recordedPhrase.Value = new RecordedPhrase(clip, phrase, BPMManager.Instance.Current);
        }

        /// <summary>
        /// 再生が終わった。録音は待機に戻す。
        /// 弾き終わりのあとも歌い終わりを待って録り続けているため、その間は録音中のまま見せる。
        /// </summary>
        private void EndSession()
        {
            if (_state.Value == RecordingState.Disabled)
            {
                return;
            }
            CancelSessionEnd();
            _sessionEndRoutine = StartCoroutine(StandbyAfterTail());
        }

        private IEnumerator StandbyAfterTail()
        {
            yield return new WaitForSeconds(RecordingRules.CaptureTailSec);

            _sessionEndRoutine = null;
            // 待っている間に録音を切られていたら、待機に戻さない
            if (_state.Value == RecordingState.Disabled)
            {
                yield break;
            }
            _state.Value = RecordingState.Standby;
        }

        // 待っている間に次の再生が始まったら、そちらの録音中を待機で上書きしないよう取り消す。
        private void CancelSessionEnd()
        {
            if (_sessionEndRoutine == null)
            {
                return;
            }
            StopCoroutine(_sessionEndRoutine);
            _sessionEndRoutine = null;
        }

        public void Play()
        {
            var phrase = _recordedPhrase.Value;
            if (phrase == null)
            {
                return;
            }

            StopPlayback();
            _isPlaybackActive.Value = true;
            _playbackRoutine = StartCoroutine(PlayPhrase(phrase));
        }

        // 録音は MelodyPlayer を通さず自前で鳴らす。通すと転調ループ・権限の受け渡し・
        // 生徒への送信まで動いてしまい、さらに OnMelodyBegan で録音が録り直しになる。
        private IEnumerator PlayPhrase(RecordedPhrase phrase)
        {
            var piano = PianoController.Instance;
            piano.StopAllKeys(true);

            yield return phrase.Replay(piano, _playbackSource);

            _isPlaybackActive.Value = false;
            _playbackRoutine = null;
        }

        // 鳴っている途中でもう一度押されたときのために、伴奏と声の両方を降ろす。
        // コルーチンを止めるだけでは、先に鳴らし始めた声が残って二重に重なる。
        private void StopPlayback()
        {
            if (_playbackRoutine != null)
            {
                StopCoroutine(_playbackRoutine);
                _playbackRoutine = null;
            }
            _playbackSource.Stop();
        }

        private void OnDestroy()
        {
            _capture.Close();
            _recordedPhrase.Dispose();
            _state.Dispose();
            _isPlaybackActive.Dispose();
            _onMicAccessFailed.Dispose();
        }
    }
}
