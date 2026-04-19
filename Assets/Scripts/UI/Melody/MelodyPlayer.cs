using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using R3;
using AsseScripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Scripts.UI.AutoKeyChangeManager;
using DomainMelody = AsseScripts.Domain.Melody;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

namespace Assets.Scripts.UI.Melody
{
    public sealed class MelodyPlayer : MonoBehaviour
    {
        private readonly Subject<bool> _onPlayBegan = new();
        private readonly Subject<Unit> _onPlayEnded = new();

        public Observable<bool> OnPlayBegan => _onPlayBegan;
        public Observable<Unit> OnPlayEnded => _onPlayEnded;

        private bool _suppressPlayEnded = false;

        /// <summary>
        /// true の間はピアノ入力に反応しない（メロディ作成シーン用）。
        /// </summary>
        public bool BlockInput { get; set; } = false;

        public readonly struct PlayModeSettings
        {
            public bool PlayCode { get; }
            public bool PlayPiano { get; }
            public bool PlayMetronome { get; }
            private PlayModeSettings(bool playCode, bool playMetronome, bool playPiano)
            {
                PlayCode = playCode;
                PlayPiano = playPiano;
                PlayMetronome = playMetronome;
            }

            public static PlayModeSettings FromFlags(bool isTeacherSide, bool earphoneOn)
            {
                if (isTeacherSide)
                {
                    // 先生側: コード＋メトロノーム＋ピアノ
                    return new PlayModeSettings(true, true, true);
                }
                else if (earphoneOn)
                {
                    // イヤホンモード: コード＋メトロノーム（ピアノなし）
                    return new PlayModeSettings(true, true, false);
                }
                else
                {
                    // 生徒側: すべてオフ
                    return new PlayModeSettings(false, false, false);
                }
            }
        }

        private DomainPianoNote _currentRootKey;
        private DomainMelody _currentMelody;
        public static MelodyPlayer Instance { get; private set; }

        [Header("Metronome Settings")]
        [SerializeField, Tooltip("メトロノーム音源")]
        private AudioClip _metronomeClip;

        [SerializeField, Tooltip("メトロノーム用AudioSource")]
        private AudioSource _metronomeAudioSource;

        private MetronomePlayer _metronomePlayer;

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
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (_metronomeAudioSource == null)
            {
                _metronomeAudioSource = gameObject.AddComponent<AudioSource>();
                _metronomeAudioSource.playOnAwake = false;
            }
            _metronomePlayer = new MetronomePlayer(_metronomeAudioSource, _metronomeClip);
        }

        public void PlayMelody(DomainMelody melody, DomainPianoNote pressedKey, PlayModeSettings settings)
        {
            // 前回のコルーチン・状態を確実に停止してから開始（競合防止）
            _suppressPlayEnded = true;
            StopMelody(true);
            _suppressPlayEnded = false;

            _onPlayBegan.OnNext(PianoTeacherSideManager.Instance.TeacherSideButtonState);

            var piano = PianoController.Instance;
            _currentMelody = melody;
            _currentRootKey = pressedKey;
            _currentSettings = settings;
            StartCoroutine(PlayMelodyLoopCoroutine(piano, melody));
        }

        public void StopMelody(bool setKeyVisual)
        {
            var piano = PianoController.Instance;
            piano.StopMelody(setKeyVisual);

            StopAllCoroutines();
            _isPlayingChord = false;

            _metronomePlayer.Stop();

            if (!_suppressPlayEnded)
            {
                _onPlayEnded.OnNext(Unit.Default);
            }
        }
        private interface IMelodyPlayStrategy
        {
            bool SupportAutoKeyChange { get; }
            bool CanDelete { get; }
            IEnumerator Execute(PianoController piano, DomainMelody melody,
                                DomainPianoNote pressedKey, PlayModeSettings settings);
        }

        private sealed class SinglePlayStrategy : IMelodyPlayStrategy
        {
            public bool SupportAutoKeyChange => false;
            public bool CanDelete => false;
            public IEnumerator Execute(PianoController piano, DomainMelody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
                piano.Play(pressedKey, settings.PlayPiano, VolumeManager.Instance.Volume);
                yield break;
            }

        }

        private sealed class MajorWithMetronomePlayStrategy : IMelodyPlayStrategy
        {
            private readonly MelodyPlayer _player;
            public MajorWithMetronomePlayStrategy(MelodyPlayer player) => _player = player;

            public bool SupportAutoKeyChange => false;
            public bool CanDelete => false;

            public IEnumerator Execute(PianoController piano, DomainMelody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
                foreach (var interval in melody.Chord.Intervals)
                {
                    var key = pressedKey + interval;
                    piano.Play(key, settings.PlayCode, VolumeManager.Instance.Volume);
                }

                _player._isPlayingChord = true;
                while (true)
                {
                    if (settings.PlayMetronome)
                    {
                        _player._metronomePlayer.PlayOneShot(VolumeManager.Instance.Volume);
                    }
                    yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat);
                }
            }
        }

        private sealed class MajorPlayStrategy : IMelodyPlayStrategy
        {
            private readonly MelodyPlayer _player;
            public MajorPlayStrategy(MelodyPlayer player) => _player = player;
            public bool SupportAutoKeyChange => false;
            public bool CanDelete => false;
            public IEnumerator Execute(PianoController piano, DomainMelody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
                _player._isPlayingChord = false;

                var chordKeys = new List<DomainPianoNote>();
                foreach (var interval in melody.Chord.Intervals)
                {
                    var key = pressedKey + interval;
                    if (0 <= key.Index && key.Index < piano.KeyCount)
                    {
                        chordKeys.Add(key);
                    }
                }

                foreach (var key in chordKeys)
                {
                    piano.Play(key, settings.PlayCode, VolumeManager.Instance.Volume);
                }

                float beatSec = BPMManager.Instance.SecondPerBeat;
                for (int b = 0; b < melody.Chord.Beats; b++)
                {
                    if (settings.PlayMetronome)
                    {
                        _player._metronomePlayer.PlayOneShot(VolumeManager.Instance.Volume);
                    }
                    yield return new WaitForSeconds(beatSec);
                }

                foreach (var key in chordKeys)
                {
                    piano.Stop(key, false);
                }
            }
        }

        private sealed class DefaultPlayStrategy : IMelodyPlayStrategy
        {
            private readonly MelodyPlayer _player;
            public DefaultPlayStrategy(MelodyPlayer player) => _player = player;

            public bool SupportAutoKeyChange => true;
            public bool CanDelete => true;

            public IEnumerator Execute(PianoController piano, DomainMelody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
                // ── 和音パート ──
                var chordKeys = new List<DomainPianoNote>();
                foreach (var interval in melody.Chord.Intervals)
                {
                    var key = pressedKey + interval;
                    if (0 <= key.Index && key.Index < piano.KeyCount)
                    {
                        chordKeys.Add(key);
                    }
                }

                foreach (var key in chordKeys)
                {
                    piano.Play(key, settings.PlayCode, VolumeManager.Instance.Volume);
                }
                _player._isPlayingChord = true;

                float beatSec = BPMManager.Instance.SecondPerBeat;
                for (int b = 0; b < melody.Chord.Beats; b++)
                {
                    if (settings.PlayMetronome)
                    {
                        _player._metronomePlayer.PlayOneShot(VolumeManager.Instance.Volume);
                    }
                    yield return new WaitForSeconds(beatSec);
                }

                foreach (var key in chordKeys)
                {
                    piano.Stop(key, false);
                }
                _player._isPlayingChord = false;

                // ── メロディパート ──
                foreach (var note in melody.Notes)
                {
                    var key = pressedKey + note.Interval;
                    piano.Play(key, settings.PlayPiano, VolumeManager.Instance.Volume);
                    yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                    piano.Stop(key, true);
                }
            }
        }

        private IMelodyPlayStrategy GetStrategy(DomainMelody melody) =>
            melody.Name switch
            {
                "Single"           => new SinglePlayStrategy(),
                "Major& Metronome" => new MajorWithMetronomePlayStrategy(this),
                "Major Code"       => new MajorPlayStrategy(this),
                _                  => new DefaultPlayStrategy(this),
            };

        public bool CanDeleteMelody(DomainMelody melody) => GetStrategy(melody).CanDelete;

        private IEnumerator PlayMelodyAtKeyOnce(PianoController piano, DomainMelody melody,
                                                DomainPianoNote pressedKey, PlayModeSettings settings)
        {
            yield return StartCoroutine(GetStrategy(melody).Execute(piano, melody, pressedKey, settings));
        }

        private IEnumerator PlayMelodyLoopCoroutine(PianoController piano, DomainMelody melody)
        {
            if (!IsMelodyPlayableWithinRange(melody, _currentRootKey))
            {
                yield break;
            }
            bool prevPlaySide = _currentSettings.PlayPiano;
            var strategy = GetStrategy(melody);

            yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, _currentRootKey, _currentSettings));

            if (!strategy.SupportAutoKeyChange)
            {
                yield break;
            }

            while (_autoKeyChangeState != AutoKeyChangeState.None)
            {
                piano.StopMelody(true);

                bool playSideChanged = _currentSettings.PlayPiano != prevPlaySide;
                if (!playSideChanged)
                {
                    _currentRootKey = GetNextRoot(_currentRootKey, _autoKeyChangeState);

                    if (!IsMelodyPlayableWithinRange(melody, _currentRootKey))
                    {
                        break;
                    }
                }
                prevPlaySide = _currentSettings.PlayPiano;

                yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, _currentRootKey, _currentSettings));
            }

            _onPlayEnded.OnNext(Unit.Default);
        }

        private DomainPianoNote GetNextRoot(DomainPianoNote current, AutoKeyChangeState direction)
        {
            int step = direction switch
            {
                AutoKeyChangeState.Up   => 1,
                AutoKeyChangeState.Down => -1,
                _                       => 0,
            };
            var interval = new Interval(step);
            return current + interval;
        }

        public void HighlightMinMaxKeys(DomainMelody melody, DomainPianoNote pressedKey)
        {
            if (!IsMelodyPlayableWithinRange(melody, pressedKey))
            {
                return;
            }
            var piano = PianoController.Instance;
            var minKey = pressedKey + melody.MinInterval;
            var maxKey = pressedKey + melody.MaxInterval;
            piano.SetHighlight( minKey, maxKey);
        }

        public void EnsureKeyRangeVisible(DomainMelody melody, DomainPianoNote pressedKey)
        {
            if (!IsMelodyPlayableWithinRange(melody, pressedKey))
            {
                return;
            }
            var minKey = pressedKey + melody.MinInterval;
            var maxKey = pressedKey + melody.MaxInterval;
            PianoController.Instance.EnsureRangeVisible(minKey, maxKey);
        }

        private bool IsMelodyPlayableWithinRange(DomainMelody melody, DomainPianoNote rootKey)
        {
            var piano = PianoController.Instance;
            var minKey = rootKey + melody.MinInterval;
            var maxKey = rootKey + melody.MaxInterval;

            return (0 <= minKey.Index) && (maxKey.Index < piano.KeyCount);
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

            bool isOpposite =
                (previous == AutoKeyChangeState.Up && current == AutoKeyChangeState.Down) ||
                (previous == AutoKeyChangeState.Down && current == AutoKeyChangeState.Up);

            if (!isOpposite)
            {
                return;
            }

            int step = current == AutoKeyChangeState.Up ? +2 : -2;
            var transposed = _currentRootKey + new Interval(step);

            if (!IsMelodyPlayableWithinRange(_currentMelody, transposed))
            {
                return; // 範囲外
            }

            // 現在コード停止（色は保持）
            StopMelody(true);

            // 新ルート設定・再開
            _currentRootKey = transposed;
            var piano = PianoController.Instance;
            StartCoroutine(PlayMelodyLoopCoroutine(piano, _currentMelody));
        }

        private void Start()
        {
            EarphoneModeManager.Instance.OnModeChanged
                .Subscribe(_ => RefreshCurrentSettings())
                .AddTo(this);

            if (TeacherSideManager.Instance != null)
            {
                TeacherSideManager.Instance.OnStateChanged
                    .Subscribe(_ => RefreshCurrentSettings())
                    .AddTo(this);
            }
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

            piano.OnAnyKeyClickAsObservable
                .Subscribe(key =>
                {
                    if (BlockInput) return;
                    var melody = MelodyManager.Instance.CurrentMelody;
                    HighlightMinMaxKeys(melody, key);
                    var settings = PlayModeSettings.FromFlags(
                        TeacherSideManager.Instance.TeacherSideButtonState,
                        EarphoneModeManager.Instance.EarphoneMode);
                    PlayMelody(melody, key, settings);
                    EnsureKeyRangeVisible(melody, key);
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyEnterAsObservable
                .Subscribe(key =>
                {
                    if (BlockInput) return;
                    var melody = MelodyManager.Instance.CurrentMelody;
                    HighlightMinMaxKeys(melody, key);
                })
                .AddTo(_pianoDisposable);
        }

        private void OnEnable()
        {
            AutoKeyChangeManager.Instance.OnStateChanged += HandleAutoKeyChangeState;
            HandleAutoKeyChangeState(AutoKeyChangeManager.Instance.State);
        }

        private void OnDisable()
        {
            if (AutoKeyChangeManager.Instance != null)
            {
                AutoKeyChangeManager.Instance.OnStateChanged -= HandleAutoKeyChangeState;
            }
        }

        private void RefreshCurrentSettings()
        {
            _currentSettings = PlayModeSettings.FromFlags(
                TeacherSideManager.Instance.TeacherSideButtonState,
                EarphoneModeManager.Instance.EarphoneMode);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _pianoDisposable.Dispose();
        }

    }

}