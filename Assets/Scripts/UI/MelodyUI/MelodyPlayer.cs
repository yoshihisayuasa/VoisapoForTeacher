using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.MelodyUI.PlayStrategies;
using Assets.Scripts.UI.Piano;
using R3;
using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MelodyUI
{
    public sealed class MelodyPlayer : MonoBehaviour, IMelodyPlaybackContext
    {
        private readonly Subject<bool> _onPlayEnded = new();
        private readonly Subject<bool> _onMelodyBegan = new();
        private readonly Subject<bool> _onTeacherPlayStatus = new();

        public Observable<bool> OnPlayEnded => _onPlayEnded;
        public Observable<bool> OnMelodyBegan => _onMelodyBegan;
        public Observable<bool> OnTeacherPlayStatus => _onTeacherPlayStatus;

        private Melody _currentMelody;
        public static MelodyPlayer Instance { get; private set; }

        [Header("Metronome Settings")]
        [SerializeField, Tooltip("メトロノーム音源")]
        private AudioClip _metronomeClip;

        [SerializeField, Tooltip("メトロノーム用AudioSource")]
        private AudioSource _metronomeAudioSource;

        private MetronomePlayer _metronomePlayer;
        private Dictionary<MelodyKind, IMelodyPlayStrategy> _strategies;
        private readonly MelodyRangePresenter _rangePresenter = new();

        private AutoKeyChangeState _autoKeyChangeState = AutoKeyChangeState.None;
        private AutoKeyChangeState _prevAutoKeyChangeState = AutoKeyChangeState.None;
        private bool _isPlayingChord = false;
        private PlayModeSettings _currentSettings;

        private CompositeDisposable _pianoDisposable = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PersistentRegistry.Register(gameObject);

            if (_metronomeAudioSource == null)
            {
                _metronomeAudioSource = gameObject.AddComponent<AudioSource>();
                _metronomeAudioSource.playOnAwake = false;
            }
            _metronomePlayer = new MetronomePlayer(_metronomeAudioSource, _metronomeClip);

            // 戦略は状態を持たないため、種別ごとに1インスタンスを使い回す。
            _strategies = new Dictionary<MelodyKind, IMelodyPlayStrategy>
            {
                [MelodyKind.Standard]           = new StandardPlayStrategy(this),
                [MelodyKind.Single]             = new SinglePlayStrategy(this),
                [MelodyKind.MajorWithMetronome] = new MajorWithMetronomePlayStrategy(this),
                [MelodyKind.MajorChord]         = new MajorChordPlayStrategy(this),
            };
        }

        public void PlayMelody(Melody melody, PlayModeSettings settings)
        {
            StopMelodyAndReset();

            var piano = PianoController.Instance;
            _currentMelody = melody;
            _currentSettings = settings;
            StartCoroutine(PlayMelodyLoopCoroutine(piano, melody));
        }

        /// <summary>
        /// 再生を中断し、鍵盤の色をすべてリセットして録音も即時停止する。
        /// メロディ切替・新規再生の前処理・移調による再スタートに使う。
        /// </summary>
        public void StopMelodyAndReset()
        {
            StopMelodyCore(setKeyVisual: true, shouldDelayRecordStop: false);
        }

        /// <summary>
        /// 演奏として終了する。演奏済み色を保持し、録音停止は余韻のため遅延させる。
        /// 停止ボタン・離鍵・ネットワーク停止受信から使う。
        /// </summary>
        public void FinishMelody()
        {
            StopMelodyCore(setKeyVisual: false, shouldDelayRecordStop: true);
        }

        private void StopMelodyCore(bool setKeyVisual, bool shouldDelayRecordStop)
        {
            var piano = PianoController.Instance;
            piano.StopAllKeys(setKeyVisual);

            StopAllCoroutines();
            _isPlayingChord = false;

            _metronomePlayer.Stop();

            _onPlayEnded.OnNext(shouldDelayRecordStop);
        }

        private IMelodyPlayStrategy GetStrategy(Melody melody) =>
            _strategies.TryGetValue(melody.Kind, out var strategy)
                ? strategy
                : _strategies[MelodyKind.Standard];

        // ── IMelodyPlaybackContext（再生戦略からの通知窓口） ──

        void IMelodyPlaybackContext.NotifyMelodyBegan()
        {
            _onMelodyBegan.OnNext(SoundPlayManager.Instance.IsSoundPlay);
        }

        void IMelodyPlaybackContext.BeginChordSection()
        {
            _isPlayingChord = true;
        }

        void IMelodyPlaybackContext.EndChordSection()
        {
            _isPlayingChord = false;
        }

        void IMelodyPlaybackContext.PlayMetronomeBeat()
        {
            _metronomePlayer.PlayOneShot(VolumeManager.Instance.Volume);
        }

        public bool CanDeleteMelody(Melody melody) => GetStrategy(melody).CanDelete;

        private IEnumerator PlayMelodyAtKeyOnce(PianoController piano, Melody melody,
                                                PianoNote pressedKey, PlayModeSettings settings)
        {
            NotifyTeacherPlayStatus();
            yield return StartCoroutine(GetStrategy(melody).Execute(piano, melody, pressedKey, settings));
        }

        // 「先生側で鳴っているか」は端末ごとに視点が異なる。先生は自分が音源側なら true、
        // 生徒は状態が反転同期されるため自分が音源側でない（false）ときが先生側再生。
        // ループ再生中に音源側が切り替わっても表示へ追従できるよう、再生のたびに現在状態を通知する。
        private void NotifyTeacherPlayStatus()
        {
            bool isSoundPlay = SoundPlayManager.Instance.IsSoundPlay;
            bool isTeacherSidePlaying = AppMode.IsTeacher ? isSoundPlay : !isSoundPlay;
            _onTeacherPlayStatus.OnNext(isTeacherSidePlaying);
        }

        private IEnumerator PlayMelodyLoopCoroutine(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (!melody.IsPlayableAt(rootKey, piano.KeyCount))
            {
                yield break;
            }
            bool prevPlaySide = _currentSettings.PlayPiano;
            var strategy = GetStrategy(melody);

            yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, rootKey, _currentSettings));

            if (!strategy.SupportAutoKeyChange)
            {
                yield break;
            }

            while (_autoKeyChangeState.IsActive())
            {
                piano.StopAllKeys(true);

                bool playSideChanged = _currentSettings.PlayPiano != prevPlaySide;
                if (!playSideChanged)
                {
                    var nextKey = piano.SelectedKey + _autoKeyChangeState.NextRootStep();
                    piano.SelectKey(nextKey);

                    if (!melody.IsPlayableAt(nextKey, piano.KeyCount))
                    {
                        break;
                    }
                }
                prevPlaySide = _currentSettings.PlayPiano;

                yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, piano.SelectedKey, _currentSettings));
            }

            FinishMelody();
        }

        private void HandleAutoKeyChangeState(AutoKeyChangeState newState)
        {
            _autoKeyChangeState = newState;
            TryImmediateDirectionSwap(_prevAutoKeyChangeState, _autoKeyChangeState);
            _prevAutoKeyChangeState = newState;
        }

        // Up↔Down 反転時、コード再生中なら ±2 半音へ即移調し再スタート
        private void TryImmediateDirectionSwap(AutoKeyChangeState previous, AutoKeyChangeState current)
        {
            if (!_isPlayingChord)
            {
                return;
            }

            if (!current.IsOppositeOf(previous))
            {
                return;
            }

            var piano = PianoController.Instance;
            var transposed = piano.SelectedKey + current.DirectionSwapStep();

            if (!_currentMelody.IsPlayableAt(transposed, piano.KeyCount))
            {
                return; // 範囲外
            }

            // 現在の再生を中断（色はリセット）
            StopMelodyAndReset();

            // 新ルート設定・再開
            piano.SelectKey(transposed);
            StartCoroutine(PlayMelodyLoopCoroutine(piano, _currentMelody));
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            MelodyManager.Instance.MelodyChanged.Subscribe(OnMelodyChanged).AddTo(this);
            AutoKeyChangeManager.Instance.State.Subscribe(HandleAutoKeyChangeState).AddTo(this);
            SubscribeToPiano();

            EarphoneModeManager.Instance.OnModeChanged
                .Subscribe(_ => RefreshCurrentSettings())
                .AddTo(this);

            SoundPlayManager.Instance.OnStateChanged
                .Subscribe(_ => RefreshCurrentSettings())
                .AddTo(this);

            RefreshCurrentSettings();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SubscribeToPiano();
        }

        private void SubscribeToPiano()
        {
            _pianoDisposable.Dispose();
            _pianoDisposable = new CompositeDisposable();

            var piano = PianoController.Instance;
            if (piano == null) return;

            piano.OnRootKeyPressedAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    _rangePresenter.RefreshHighlight(piano, melody);
                    PlayMelody(melody, _currentSettings);
                    _rangePresenter.EnsureVisible(piano, melody);
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyUpAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    if (GetStrategy(melody).StopOnKeyUp)
                    {
                        FinishMelody();
                    }
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyEnterAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    _rangePresenter.RefreshHighlight(piano, melody);
                })
                .AddTo(_pianoDisposable);
        }

        // 現在の再生サイド・イヤホン状態から再生設定を組み立て直す。
        private void RefreshCurrentSettings()
        {
            _currentSettings = PlayModeSettings.FromFlags(
                SoundPlayManager.Instance.IsSoundPlay,
                EarphoneModeManager.Instance?.EarphoneMode);
        }

        private void OnMelodyChanged(Melody melody)
        {
            if (melody == null) return;
            var piano = PianoController.Instance;
            _rangePresenter.RefreshHighlight(piano, melody);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _pianoDisposable.Dispose();
        }

    }

}
