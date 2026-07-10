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
        private readonly Subject<PianoNote> _onLoopKeyPlayed = new();
        private readonly Subject<PianoNote> _onBatonPassed = new();

        public Observable<bool> OnPlayEnded => _onPlayEnded;
        public Observable<bool> OnMelodyBegan => _onMelodyBegan;
        public Observable<bool> OnTeacherPlayStatus => _onTeacherPlayStatus;

        /// <summary>トークン保持者（音源側）が周・再スタートの開始キーを弾いた。描画側への送信用（ローカル発のみ）。</summary>
        public Observable<PianoNote> OnLoopKeyPlayed => _onLoopKeyPlayed;

        /// <summary>再生権限を委譲した。次の権威が再生を始めるキーを運ぶ（ローカル発のみ）。</summary>
        public Observable<PianoNote> OnBatonPassed => _onBatonPassed;

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

        // 再生権限トークン。保持者（音源側）だけがループ・周キー送信・±2即時反転の判断を行い、
        // 非保持者（描画側）は受信したキーごとの1回再生に徹する。
        private bool _hasPlaybackToken = false;
        private bool _isPlaybackRunning = false;
        private PianoNote _pendingBatonKey;

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

            // 再生開始時点の音源側がトークンを持つ。SoundPlayState と KeyDown はどちらも
            // 先生発（同一送信者内で順序保証）なので、この判定は両端末で一致する。
            _hasPlaybackToken = SoundPlayManager.Instance.IsSoundPlay;
            StartCoroutine(RunPlayback(piano, melody));
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
            _isPlaybackRunning = false;
            _hasPlaybackToken = false;
            _pendingBatonKey = null;

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

        /// <summary>
        /// 再生コルーチンの実行状態を追跡し、弾き切った後に保留中のバトンがあれば権威を引き継ぐ。
        /// （バトン受信時に描画中だった場合、その周を最後まで弾いてから引き継ぐための入口）
        /// </summary>
        private IEnumerator RunPlayback(PianoController piano, Melody melody)
        {
            _isPlaybackRunning = true;
            yield return StartCoroutine(PlayMelodyLoopCoroutine(piano, melody));
            _isPlaybackRunning = false;
            TryPromoteWithPendingBaton();
        }

        private IEnumerator PlayMelodyLoopCoroutine(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (!melody.IsPlayableAt(rootKey, piano.KeyCount))
            {
                yield break;
            }
            var strategy = GetStrategy(melody);

            yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, rootKey, _currentSettings));

            if (!strategy.SupportAutoKeyChange)
            {
                yield break;
            }

            // ループはトークン保持者（音源側）だけが回す。描画側はこの while に入らず1回で終わる。
            while (_hasPlaybackToken && _autoKeyChangeState.IsActive())
            {
                piano.StopAllKeys(true);

                // 権限委譲: 自分がもう音源側でないなら、弾き切ったこの境界で止めて渡す。
                // 「停止」と「委譲」は境界での不可分な1処理（分けると渡し損ねの中間状態が生まれる）。
                // 新権威は同じキーから再開する（サイド切替の周は移調しない、従来の挙動を維持）。
                if (!SoundPlayManager.Instance.IsSoundPlay)
                {
                    PassBaton(piano.SelectedKey);
                    yield break;
                }

                var nextKey = piano.SelectedKey + _autoKeyChangeState.NextRootStep();

                if (!melody.IsPlayableAt(nextKey, piano.KeyCount))
                {
                    break;
                }

                piano.SelectKey(nextKey);
                _onLoopKeyPlayed.OnNext(nextKey);

                yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, piano.SelectedKey, _currentSettings));
            }

            // 保留中のバトンがあれば終了せずに抜け、RunPlayback 側で権威を引き継ぐ。
            // （FinishMelody は全コルーチン停止と保留バトンの破棄を伴うため、ここで呼ぶと引き継げない）
            if (_pendingBatonKey == null)
            {
                FinishMelody();
            }
        }

        private void PassBaton(PianoNote nextKey)
        {
            _hasPlaybackToken = false;
            _onBatonPassed.OnNext(nextKey);
        }

        /// <summary>
        /// 受信した周キーを1回だけ再生する（描画側）。音源側の各周に遅れて追従するだけなので、
        /// キーの計算もループもここでは行わない。
        /// </summary>
        public void RenderLoopKey(PianoNote key)
        {
            if (_hasPlaybackToken || _currentMelody == null)
            {
                return;
            }

            StopMelodyAndReset();

            var piano = PianoController.Instance;
            piano.SelectKey(key);
            StartCoroutine(RunPlayback(piano, _currentMelody));
        }

        /// <summary>
        /// バトン（再生権限の委譲）を受け取る。描画中の周があればそれを弾き切ってから、
        /// 受け取ったキーでトークン保持者として再生を引き継ぐ。
        /// </summary>
        public void ReceiveBaton(PianoNote nextKey)
        {
            if (_currentMelody == null)
            {
                return;
            }

            _pendingBatonKey = nextKey;

            if (!_isPlaybackRunning)
            {
                TryPromoteWithPendingBaton();
            }
        }

        /// <summary>
        /// バトンを持つ生徒が切断したとき、先生が権限を自己回収する（委譲が永遠に完了しない穴を塞ぐ）。
        /// 現在のキーを起点に、バトン受信と同じ引き継ぎ経路を通す。
        /// </summary>
        public void ReclaimPlaybackAuthority()
        {
            if (_hasPlaybackToken || !_isPlaybackRunning)
            {
                return;
            }

            if (!SoundPlayManager.Instance.IsSoundPlay)
            {
                return;
            }

            ReceiveBaton(PianoController.Instance.SelectedKey);
        }

        private bool TryPromoteWithPendingBaton()
        {
            if (_pendingBatonKey == null)
            {
                return false;
            }

            var piano = PianoController.Instance;
            var startKey = _pendingBatonKey;
            _pendingBatonKey = null;

            piano.StopAllKeys(true);
            piano.SelectKey(startKey);
            _hasPlaybackToken = true;

            // 旧権威（いまは描画側）もこのキーへ追従させる。
            _onLoopKeyPlayed.OnNext(startKey);
            StartCoroutine(RunPlayback(piano, _currentMelody));
            return true;
        }

        private void HandleAutoKeyChangeState(AutoKeyChangeState newState)
        {
            _autoKeyChangeState = newState;
            TryImmediateDirectionSwap(_prevAutoKeyChangeState, _autoKeyChangeState);
            _prevAutoKeyChangeState = newState;
        }

        // Up↔Down 反転時、コード再生中なら ±2 半音へ即移調し再スタート。
        // 判断はトークン保持者（音源側）だけが自分の再生位相で行う。聞こえている音が唯一の
        // 真実なので「和音中かどうか」の判定はラグに関わらず常に正しい。描画側へは判断結果
        // （再スタートキー）を周キーとして送り、受信側は追従して張り直すだけにする。
        private void TryImmediateDirectionSwap(AutoKeyChangeState previous, AutoKeyChangeState current)
        {
            if (!_hasPlaybackToken)
            {
                return;
            }

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

            // 現在の再生を中断（色はリセット。トークンもクリアされるため取り直す）
            StopMelodyAndReset();
            _hasPlaybackToken = true;

            // 新ルート設定・描画側への通知・再開
            piano.SelectKey(transposed);
            _onLoopKeyPlayed.OnNext(transposed);
            StartCoroutine(RunPlayback(piano, _currentMelody));
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
                EarphoneModeManager.Instance.EarphoneMode);
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
