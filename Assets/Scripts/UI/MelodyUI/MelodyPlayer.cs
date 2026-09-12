using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.Modules;
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
        private readonly Subject<Unit> _onPlayEnded = new();
        private readonly Subject<bool> _onMelodyBegan = new();
        private readonly Subject<Unit> _onMelodyEnded = new();
        private readonly Subject<bool> _onTeacherPlayStatus = new();
        private readonly Subject<PianoNote> _onLoopKeyPlayed = new();

        private readonly PlaybackAuthorityCoordinator _authority = new();

        // 音源側が交代した直後に間を空けるルール。音源側・描画側の双方が同じ規則で
        // 独立に待つため、待っている間も音と鍵盤表示は揃ったままになる。
        private readonly PlaybackHandoverPolicy _handover = new();

        public Observable<Unit> OnPlayEnded => _onPlayEnded;
        public Observable<bool> OnMelodyBegan => _onMelodyBegan;

        /// <summary>メロディを1回弾き終えた。録音はここまでを1フレーズとして切り出す。</summary>
        public Observable<Unit> OnMelodyEnded => _onMelodyEnded;
        public Observable<bool> OnTeacherPlayStatus => _onTeacherPlayStatus;

        /// <summary>トークン保持者（音源側）が周・再スタートの開始キーを弾いた。描画側への送信用（ローカル発のみ）。</summary>
        public Observable<PianoNote> OnLoopKeyPlayed => _onLoopKeyPlayed;

        /// <summary>再生権限を委譲した。次の権威が再生を始めるキーを運ぶ（ローカル発のみ）。</summary>
        public Observable<PianoNote> OnBatonPassed => _authority.OnBatonPassed;

        private Melody _currentMelody;

        // いま鳴らしている周のルート。再生位置の権威はこれで、piano.SelectedKey は見ない。
        // SelectedKey は先生のマウスホバーでも動く「表示上の選択」であり、鍵盤を横切っただけで
        // 変わる。再生の判断に使うと、転調先も生徒へ渡すバトンもカーソル位置に飛んでしまう。
        // 交代待ちのギャップ中（RunPlayback の WaitForSeconds）にも動くため、再生開始を
        // 要求した時点で確定させておく必要がある。
        private PianoNote _playingRoot;

        public static MelodyPlayer Instance { get; private set; }

        [Header("Metronome Settings")]
        [SerializeField, Tooltip("メトロノーム音源")]
        private AudioClip _metronomeClip;

        [SerializeField, Tooltip("メトロノーム用AudioSource")]
        private AudioSource _metronomeAudioSource;

        private MetronomePlayer _metronomePlayer;
        private Dictionary<MelodyKind, MelodyPlayStrategy> _strategies;

        // スクロール追従（EnsureVisible）専用。範囲ハイライトの更新は
        // 演奏シーンにのみ存在する MelodyRangeHighlightBinder が担う。
        private readonly MelodyRangePresenter _rangePresenter = new();

        private AutoKeyChangeState _autoKeyChangeState = AutoKeyChangeState.None;
        private AutoKeyChangeState _prevAutoKeyChangeState = AutoKeyChangeState.None;
        private bool _isPlayingChord = false;
        private PlayModeSettings _currentSettings;

        // 再生権限トークン/バトンの調停は _authority に委譲する。ここではコルーチンの
        // 生存状態だけを持つ（保留バトンの昇格契機を MelodyPlayer 側で選ぶため）。
        private bool _isPlaybackRunning = false;
        private Coroutine _playbackCoroutine;

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
            _strategies = new Dictionary<MelodyKind, MelodyPlayStrategy>
            {
                [MelodyKind.Standard]           = new StandardPlayStrategy(this),
                [MelodyKind.Single]             = new SinglePlayStrategy(this),
                [MelodyKind.ChordWithMetronome] = new ChordWithMetronomePlayStrategy(this),
                [MelodyKind.Chord]              = new ChordPlayStrategy(this),
            };
        }

        public void PlayMelody(Melody melody, PlayModeSettings settings)
        {
            StopMelodyAndReset();

            var piano = PianoController.Instance;
            _currentMelody = melody;
            _currentSettings = settings;

            // 押下で確定した根音をこの場で捕まえる。コルーチンの中で読み直すと、
            // 交代待ちのギャップ中にホバーで動いた値を拾ってしまう。
            _playingRoot = piano.SelectedKey;

            // 再生開始時点の音源側がトークンを持つ。SoundPlayState と KeyDown はどちらも
            // 先生発（同一送信者内で順序保証）なので、この判定は両端末で一致する。
            _authority.AssumeToken(SoundPlayManager.Instance.IsSoundPlay);
            _playbackCoroutine = StartCoroutine(RunPlayback(piano, melody));
        }

        /// <summary>
        /// 再生を中断し、鍵盤の色をすべてリセットする。弾き終わりとしては扱わないため、
        /// 中断されたフレーズは録音されない（次の再生開始で起点を引き直す）。
        /// メロディ切替・新規再生の前処理・移調による再スタートに使う。
        /// </summary>
        public void StopMelodyAndReset()
        {
            StopMelodyCore(setKeyVisual: true);
        }

        /// <summary>
        /// 演奏として終了する。演奏済み色を保持する。
        /// 停止ボタン・離鍵・ネットワーク停止受信から使う。
        /// </summary>
        public void FinishMelody()
        {
            // 弾き終わりをどう区切るかは種別ごとに違う。周ごとに区切る種別はここでは何もしない。
            // まだ一度も再生していなければ種別が決まらない（起動直後の停止操作など）。
            if (_currentMelody != null)
            {
                GetStrategy(_currentMelody).OnPlaybackFinished();
            }
            StopMelodyCore(setKeyVisual: false);
        }

        private void StopMelodyCore(bool setKeyVisual)
        {
            var piano = PianoController.Instance;
            piano.StopAllKeys(setKeyVisual);

            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }

            // コルーチンを止めた場合は RunPlayback の末尾を通らないため、ここで閉じる。
            // 自然終了で既に閉じていれば何も起きない。
            _handover.EndSession(Time.time);

            _isPlayingChord = false;
            _isPlaybackRunning = false;
            _playingRoot = null;
            _authority.Clear();

            _metronomePlayer.Stop();

            _onPlayEnded.OnNext(Unit.Default);
        }

        private MelodyPlayStrategy GetStrategy(Melody melody) =>
            _strategies.TryGetValue(melody.Kind, out var strategy)
                ? strategy
                : _strategies[MelodyKind.Standard];

        // ── IMelodyPlaybackContext（再生戦略からの通知窓口） ──

        void IMelodyPlaybackContext.NotifyMelodyBegan()
        {
            _onMelodyBegan.OnNext(SoundPlayManager.Instance.IsSoundPlay);
        }

        void IMelodyPlaybackContext.NotifyMelodyEnded()
        {
            _onMelodyEnded.OnNext(Unit.Default);
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
            _metronomePlayer.PlayOneShot();
        }

        void IMelodyPlaybackContext.NotifyTeacherPlayStatus() => NotifyTeacherPlayStatus();

        bool IMelodyPlaybackContext.ShouldContinueAutoKeyChange()
            => _authority.HasToken && _autoKeyChangeState.IsActive();

        bool IMelodyPlaybackContext.TryPassBatonAtBoundary()
            => _authority.TryPassBatonAtBoundary(SoundPlayManager.Instance.IsSoundPlay, _playingRoot);

        PianoNote IMelodyPlaybackContext.NextAutoKeyChangeRoot()
            => _playingRoot + _autoKeyChangeState.NextRootStep();

        void IMelodyPlaybackContext.AdvanceTo(PianoNote nextKey)
            => AdvanceTo(nextKey);

        bool IMelodyPlaybackContext.HasPendingBaton => _authority.HasPendingBaton;

        // 「先生側で鳴っているか」は端末ごとに視点が異なる。先生は自分が音源側なら true、
        // 生徒は状態が反転同期されるため自分が音源側でない（false）ときが先生側再生。
        // ループ再生中に音源側が切り替わっても表示へ追従できるよう、周ごとに現在状態を通知する。
        private void NotifyTeacherPlayStatus()
        {
            bool isSoundPlay = SoundPlayManager.Instance.IsSoundPlay;
            bool isTeacherSidePlaying = AppMode.IsTeacher ? isSoundPlay : !isSoundPlay;
            _onTeacherPlayStatus.OnNext(isTeacherSidePlaying);
        }

        /// <summary>
        /// 次の周のルートを確定する。再生位置の更新・鍵盤の選択・描画側への送信は常にこの順で
        /// 揃っている必要があるため、呼び出し側に並べさせず1操作にまとめる。
        /// </summary>
        private void AdvanceTo(PianoNote nextKey)
        {
            _playingRoot = nextKey;
            PianoController.Instance.SelectKey(nextKey);
            _onLoopKeyPlayed.OnNext(nextKey);
        }

        /// <summary>
        /// 再生コルーチンの実行状態を追跡し、弾き切った後に保留中のバトンがあれば権威を引き継ぐ。
        /// （バトン受信時に描画中だった場合、その周を最後まで弾いてから引き継ぐための入口）
        /// </summary>
        private IEnumerator RunPlayback(PianoController piano, Melody melody)
        {
            // 待っている間もこのコルーチンは走っている。先に立てておかないと、
            // 待機中に届いたバトンが割り込んで二重に再生を始めてしまう。
            _isPlaybackRunning = true;

            // 立場の判定はトークンで行う。SoundPlayManager のフラグは先生のトグル操作で
            // その場で変わるが、実際の交代はループ境界のバトンで初めて起きるため、
            // 自動キー変更中は1周ぶん食い違う。フラグで判定すると描画中の周が音源側の
            // セッションとして記録され、次の交代が「交代なし」と見なされて待機が消える。
            float gap = _handover.BeginSession(_authority.HasToken, Time.time);
            if (gap > 0f)
            {
                yield return new WaitForSeconds(gap);
            }

            // 入れ子は StartCoroutine で回さない（別コルーチンになり、StopCoroutine が
            // 外側にしか効かず内側が鳴り続ける）。IEnumerator を直接 yield して
            // このコルーチンの一部として実行し、1ハンドルで丸ごと止められるようにする。
            yield return PlaySession(piano, melody);

            // 昇格より先に閉じる。バトンを引き継ぐ再生は、この終了時刻から間隔を測る。
            _handover.EndSession(Time.time);

            _isPlaybackRunning = false;
            _playbackCoroutine = null;
            PromotePendingBatonIfAny();
        }

        // 1回の再生セッション。開始キーの演奏可否だけ見てから戦略へ委ねる。
        // 自動転調のループは戦略（Standard）が自分のセッション内で回す。
        private IEnumerator PlaySession(PianoController piano, Melody melody)
        {
            // ルートは再生開始を要求した時点で確定済み。ここで piano.SelectedKey を読み直すと、
            // 直前の交代待ち（RunPlayback のギャップ）の間にホバーで動いた値を拾ってしまう。
            var rootKey = _playingRoot;
            if (!melody.IsPlayableAt(rootKey, piano.KeyCount))
            {
                yield break;
            }

            NotifyTeacherPlayStatus();
            yield return GetStrategy(melody).Execute(piano, melody, rootKey, _currentSettings);
        }

        /// <summary>
        /// 受信した周キーを1回だけ再生する（描画側）。音源側の各周に遅れて追従するだけなので、
        /// キーの計算もループもここでは行わない。
        /// </summary>
        public void RenderLoopKey(PianoNote key)
        {
            if (_authority.HasToken || _currentMelody == null)
            {
                return;
            }

            StopMelodyAndReset();

            var piano = PianoController.Instance;
            _playingRoot = key;
            piano.SelectKey(key);
            _playbackCoroutine = StartCoroutine(RunPlayback(piano, _currentMelody));
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

            _authority.SetPending(nextKey);

            // idle 窓でバトンが届いたら、ここで能動的に昇格する。旧保持者は委譲と同時にループを止めて
            // 周キーの送信をやめるため RenderLoopKey はもう来ず、今拾わないとループを駆動する主体が
            // いなくなって演奏が止まる。再生中なら割り込まず RunPlayback 末尾で昇格する（別の窓）。
            if (!_isPlaybackRunning)
            {
                PromotePendingBatonIfAny();
            }
        }

        /// <summary>
        /// バトンを持つ生徒が切断したとき、先生が権限を自己回収する（委譲が永遠に完了しない穴を塞ぐ）。
        /// 現在のキーを起点に、バトン受信と同じ引き継ぎ経路を通す。
        /// </summary>
        public void ReclaimPlaybackAuthority()
        {
            if (_authority.HasToken || !_isPlaybackRunning)
            {
                return;
            }

            if (!SoundPlayManager.Instance.IsSoundPlay)
            {
                return;
            }

            ReceiveBaton(_playingRoot);
        }

        // 保留中のバトンがあればトークン保持者へ昇格し、その再開キーから再生を引き継ぐ。
        // 権限状態の遷移は _authority が担い、鍵盤操作とコルーチン起動はここで行う。
        private void PromotePendingBatonIfAny()
        {
            if (!_authority.TryPromote(out var startKey))
            {
                return;
            }

            var piano = PianoController.Instance;
            piano.StopAllKeys(true);

            // 旧権威（いまは描画側）もこのキーへ追従させる。
            AdvanceTo(startKey);
            _playbackCoroutine = StartCoroutine(RunPlayback(piano, _currentMelody));
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
            if (!_authority.HasToken)
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
            var transposed = _playingRoot + current.DirectionSwapStep();

            if (!_currentMelody.IsPlayableAt(transposed, piano.KeyCount))
            {
                return; // 範囲外
            }

            // 現在の再生を中断（色はリセット。トークンもクリアされるため取り直す）
            StopMelodyAndReset();
            _authority.AssumeToken(true);

            // 新ルート設定・描画側への通知・再開
            AdvanceTo(transposed);
            _playbackCoroutine = StartCoroutine(RunPlayback(piano, _currentMelody));
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
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

            // ハイライトの追従は MelodyRangeHighlightBinder（演奏シーンのみ）が OnSelectionChanged
            // 購読で担うため、ここでは再生とスクロールだけ行う。
            piano.OnRootKeyPressedAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    PlayMelody(melody, _currentSettings);
                    _rangePresenter.EnsureVisible(piano, melody);
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyUpAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    GetStrategy(melody).OnKeyUp();
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

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _pianoDisposable.Dispose();

            _onPlayEnded.Dispose();
            _onMelodyBegan.Dispose();
            _onMelodyEnded.Dispose();
            _onTeacherPlayStatus.Dispose();
            _onLoopKeyPlayed.Dispose();
            _authority.Dispose();
        }

    }

}
