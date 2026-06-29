using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using R3;
using AsseScripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Scripts.UI.AutoKeyChangeManager;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

namespace Assets.Scripts.UI.MelodyUI
{
    public sealed class MelodyPlayer : MonoBehaviour
    {
        private readonly Subject<bool> _onPlayBegan = new();
        private readonly Subject<bool> _onPlayEnded = new();
        private readonly Subject<bool> _onMelodyBegan = new();

        public Observable<bool> OnPlayBegan => _onPlayBegan;
        public Observable<bool> OnPlayEnded => _onPlayEnded;
        public Observable<bool> OnMelodyBegan => _onMelodyBegan;

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

            // 生徒ビルドには EarphoneModeManager が存在しないため、
            // earphoneOn の欠如（null）を「オフ」として扱えるよう nullable で受ける。
            public static PlayModeSettings FromFlags(bool isTeacherSide, bool? earphoneOn)
            {
                if (isTeacherSide)
                {
                    // 先生側: コード＋メトロノーム＋ピアノ
                    return new PlayModeSettings(true, true, true);
                }
                else if (earphoneOn ?? false)
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

        private bool _isTeacherSide = false;
        private Melody _currentMelody;
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
            PersistentRegistry.Register(gameObject);

            if (_metronomeAudioSource == null)
            {
                _metronomeAudioSource = gameObject.AddComponent<AudioSource>();
                _metronomeAudioSource.playOnAwake = false;
            }
            _metronomePlayer = new MetronomePlayer(_metronomeAudioSource, _metronomeClip);
        }

        public void SetTeacherSide(bool value)
        {
            _isTeacherSide = value;
            RefreshCurrentSettings();
        }

        public void PlayMelody(Melody melody, PlayModeSettings settings)
        {
            StopMelody(true, shouldDelayRecordStop: false);

            _onPlayBegan.OnNext(_isTeacherSide);

            var piano = PianoController.Instance;
            _currentMelody = melody;
            _currentSettings = settings;
            StartCoroutine(PlayMelodyLoopCoroutine(piano, melody));
        }

        public void StopMelody(bool setKeyVisual, bool shouldDelayRecordStop)
        {
            var piano = PianoController.Instance;
            piano.StopMelody(setKeyVisual);

            StopAllCoroutines();
            _isPlayingChord = false;

            _metronomePlayer.Stop();

            _onPlayEnded.OnNext(shouldDelayRecordStop);
        }
        private interface IMelodyPlayStrategy
        {
            bool SupportAutoKeyChange { get; }
            bool CanDelete { get; }
            bool StopOnKeyUp { get; }
            IEnumerator Execute(PianoController piano, Melody melody,
                                DomainPianoNote pressedKey, PlayModeSettings settings);
        }

        private sealed class SinglePlayStrategy : IMelodyPlayStrategy
        {
            private readonly MelodyPlayer _player;
            public SinglePlayStrategy(MelodyPlayer player) => _player = player;

            public bool SupportAutoKeyChange => false;
            public bool CanDelete => false;
            public bool StopOnKeyUp => true;
            public IEnumerator Execute(PianoController piano, Melody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
                _player._onMelodyBegan.OnNext(_player._isTeacherSide);
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
            public bool StopOnKeyUp => false;

            public IEnumerator Execute(PianoController piano, Melody melody,
                                       DomainPianoNote pressedKey, PlayModeSettings settings)
            {
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

                _player._onMelodyBegan.OnNext(_player._isTeacherSide);
                while (true)
                {
                    if (settings.PlayMetronome)
                    {
                        _player._metronomePlayer.PlayOneShot(VolumeManager.Instance.Volume);
                    }
                    yield return new WaitForSeconds(beatSec);
                }
            }
        }

        private sealed class MajorPlayStrategy : IMelodyPlayStrategy
        {
            private readonly MelodyPlayer _player;
            public MajorPlayStrategy(MelodyPlayer player) => _player = player;
            public bool SupportAutoKeyChange => false;
            public bool CanDelete => false;
            public bool StopOnKeyUp => false;
            public IEnumerator Execute(PianoController piano, Melody melody,
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
            public bool StopOnKeyUp => false;

            public IEnumerator Execute(PianoController piano, Melody melody,
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
                _player._onMelodyBegan.OnNext(SoundPlayManager.Instance.IsSoundPlay);
                foreach (var note in melody.Notes)
                {
                    var key = pressedKey + note.Interval;
                    piano.Play(key, settings.PlayPiano, VolumeManager.Instance.Volume);
                    yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                    piano.Stop(key, true);
                }
            }
        }

        private IMelodyPlayStrategy GetStrategy(Melody melody) =>
            melody.Name switch
            {
                "Single"           => new SinglePlayStrategy(this),
                "Major& Metronome" => new MajorWithMetronomePlayStrategy(this),
                "Major Code"       => new MajorPlayStrategy(this),
                _                  => new DefaultPlayStrategy(this),
            };

        public bool CanDeleteMelody(Melody melody) => GetStrategy(melody).CanDelete;

        private IEnumerator PlayMelodyAtKeyOnce(PianoController piano, Melody melody,
                                                DomainPianoNote pressedKey, PlayModeSettings settings)
        {
            yield return StartCoroutine(GetStrategy(melody).Execute(piano, melody, pressedKey, settings));
        }

        private IEnumerator PlayMelodyLoopCoroutine(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (!IsMelodyPlayableWithinRange(melody, rootKey, piano.KeyCount))
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

            while (_autoKeyChangeState != AutoKeyChangeState.None)
            {
                piano.StopMelody(true);

                bool playSideChanged = _currentSettings.PlayPiano != prevPlaySide;
                if (!playSideChanged)
                {
                    var nextKey = GetNextRoot(piano.SelectedKey, _autoKeyChangeState);
                    piano.SelectedKey = nextKey;

                    if (!IsMelodyPlayableWithinRange(melody, nextKey, piano.KeyCount))
                    {
                        break;
                    }
                }
                prevPlaySide = _currentSettings.PlayPiano;

                yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, piano.SelectedKey, _currentSettings));
            }

            StopMelody(false, shouldDelayRecordStop: true);
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



        private void HighlightMelodyRange(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (rootKey == null || !IsMelodyPlayableWithinRange(melody, rootKey, piano.KeyCount))
            {
                piano.ClearHighlight();
                return;
            }
            piano.SetHighlight(rootKey + melody.MinInterval, rootKey + melody.MaxInterval);
        }

        private void EnsureKeyRangeVisible(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (rootKey == null || !IsMelodyPlayableWithinRange(melody, rootKey, piano.KeyCount)) return;
            piano.EnsureRangeVisible(rootKey + melody.MinInterval, rootKey + melody.MaxInterval);
        }

        private bool IsMelodyPlayableWithinRange(Melody melody, DomainPianoNote rootKey, int keyCount)
        {
            var minKey = rootKey + melody.MinInterval;
            var maxKey = rootKey + melody.MaxInterval;
            return (0 <= minKey.Index) && (maxKey.Index < keyCount);
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
            var piano = PianoController.Instance;
            var transposed = piano.SelectedKey + new Interval(step);

            if (!IsMelodyPlayableWithinRange(_currentMelody, transposed, piano.KeyCount))
            {
                return; // 範囲外
            }

            // 現在コード停止（色は保持）
            StopMelody(true, shouldDelayRecordStop: false);

            // 新ルート設定・再開
            piano.SelectedKey = transposed;
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

            piano.OnAnyKeyClickAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    HighlightMelodyRange(piano, melody);
                    PlayMelody(melody, _currentSettings);
                    EnsureKeyRangeVisible(piano, melody);
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyUpAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    if (GetStrategy(melody).StopOnKeyUp)
                    {
                        StopMelody(false, shouldDelayRecordStop: true);
                    }
                })
                .AddTo(_pianoDisposable);

            piano.OnAnyKeyEnterAsObservable
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    if (melody == null) return;
                    HighlightMelodyRange(piano, melody);
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
            HighlightMelodyRange(piano, melody);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _pianoDisposable.Dispose();
        }

    }

}