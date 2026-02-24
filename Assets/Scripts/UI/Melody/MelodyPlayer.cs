using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Scripts.UI.Piano;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.UI.AutoKeyChangeManager;
using DomainMelody = Scripts.Domain.Melody;
using DomainPianoNote = Scripts.Domain.PianoNote;

namespace Scripts.UI.Melody
{
    public sealed class MelodyPlayer : MonoBehaviour
    {
        private readonly struct PlayModeSettings
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

        private readonly List<DomainPianoNote> _highlightedKeys = new();
        private DomainPianoNote _currentRootKey;
        private DomainMelody _currentMelody;
        public static MelodyPlayer Instance { get; private set; }

        [Header("Metronome Settings")]
        [SerializeField, Tooltip("メトロノーム音源")]
        private AudioClip _metronomeClip;

        [SerializeField, Tooltip("メトロノーム用AudioSource")]
        public AudioSource MetronomeAudioSource;
        private AutoKeyChangeState _autoKeyChangeState = AutoKeyChangeState.None;
        private AutoKeyChangeState _prevAutoKeyChangeState = AutoKeyChangeState.None;
        private bool _isPlayingChord = false;
        private bool _lastPlayPiano = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (MetronomeAudioSource == null)
            {
                MetronomeAudioSource = gameObject.AddComponent<AudioSource>();
                MetronomeAudioSource.playOnAwake = false;
            }
        }

        public void PlayMelody(DomainMelody melody, DomainPianoNote pressedKey)
        {
            var piano = PianoController.Instance;
            bool isTeacherSide = TeacherSideManager.Instance.TeacherSideButtonState;

            if (melody == null)
            {
                var settings = PlayModeSettings.FromFlags(isTeacherSide, EarphoneModeManager.Instance.EarphoneMode);
                _lastPlayPiano = settings.PlayPiano;
                piano.Play(pressedKey, settings.PlayPiano, VolumeManager.Instance.Volume);
            }
            else
            {
                _currentMelody = melody;
                _currentRootKey = pressedKey;
                StartCoroutine(PlayMelodyLoopCoroutine(piano, melody));

            }

        }

        public void StopMelody(bool setPlayedColor)
        {
            var piano = PianoController.Instance;
            
            if (_currentMelody == null)
            {
                piano.Stop(_currentRootKey, setPlayedColor);
                return;
            }
            StopPlay(piano, _currentMelody, _currentRootKey, setPlayedColor);
            StopAllCoroutines();
            _isPlayingChord = false;

            var player = MelodyPlayer.Instance;
            
            if (player.MetronomeAudioSource.isPlaying)
            {
                player.MetronomeAudioSource.Stop();
            }

        }

        /// <summary>
        /// メロディーを再生するコルーチン
        /// </summary>
        /// <param name="melody"></param>
        /// <param name="pressedKey"></param>
        /// <returns></returns>
        private IEnumerator PlayMelodyAtKeyOnce(PianoController piano, DomainMelody melody, DomainPianoNote pressedKey, PlayModeSettings settings)
        {
            if (!IsMelodyPlayableWithinRange(melody, pressedKey))
            {
                yield break;
            }

            var chordKeys = new List<DomainPianoNote>();
            for (int i = 0; i < melody.CordLength; i++)
            {
                var note = melody.Notes[i];
                if (note.Beats == 0)
                {
                    break;
                }
                var key = pressedKey + note.Interval;
                if (0 <= key.Index && key.Index < piano.KeyCount)
                {
                    chordKeys.Add(key);
                }
            }

            foreach (var key in chordKeys)
            {
                piano.Play(key, settings.PlayCode, VolumeManager.Instance.Volume);
            }

            _isPlayingChord = true;

            float beatSec = BPMManager.Instance.SecondPerBeat;
            int beats = melody.CordLength;
            if (settings.PlayMetronome)
            {
                for (int b = 0; b < beats; b++)
                {
                    PlayMetronomeSound();
                    yield return new WaitForSeconds(beatSec);
                }
            }
            else
            {
                yield return new WaitForSeconds(beatSec * beats);
            }

            foreach (var key in chordKeys)
            {
                piano.Stop(key, false);
            }
            _isPlayingChord = false;

            for (int i = melody.CordLength; i < melody.Length; i++)
            {
                var note = melody.Notes[i];
                var key = pressedKey + note.Interval;
                piano.Play(key, settings.PlayPiano, VolumeManager.Instance.Volume);
                yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                piano.Stop(key, true);
            }

        }

        private IEnumerator PlayMelodyLoopCoroutine(PianoController piano, DomainMelody melody)
        {
            if (!IsMelodyPlayableWithinRange(melody, _currentRootKey))
            {
                yield return null;
            }
            var settings = PlayModeSettings.FromFlags(TeacherSideManager.Instance.TeacherSideButtonState, EarphoneModeManager.Instance.EarphoneMode);
            _lastPlayPiano = settings.PlayPiano;

            yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, _currentRootKey, settings));

            while (true)
            {

                if (_autoKeyChangeState == AutoKeyChangeState.None)
                {
                    break;
                }
                StopPlay(piano, melody, _currentRootKey, false);

                var settingsLoop = PlayModeSettings.FromFlags(TeacherSideManager.Instance.TeacherSideButtonState, EarphoneModeManager.Instance.EarphoneMode);

                if (settingsLoop.PlayPiano == _lastPlayPiano)
                {
                    _currentRootKey = GetNextRoot(_currentRootKey, _autoKeyChangeState);

                    if (!IsMelodyPlayableWithinRange(melody, _currentRootKey))
                    {
                        break;
                    }
                }
                _lastPlayPiano = settingsLoop.PlayPiano;

                yield return StartCoroutine(PlayMelodyAtKeyOnce(piano, melody, _currentRootKey, settingsLoop));


            }
        }

        private DomainPianoNote GetNextRoot(DomainPianoNote current, AutoKeyChangeState direction)
        {
            int step = 0;
            switch (direction)
            {
                case AutoKeyChangeState.Up:
                    step = 1;
                    break;
                case AutoKeyChangeState.Down:
                    step = -1;
                    break;
                case AutoKeyChangeState.None:
                    step = 0;
                    break;

            }
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
            if (piano == null)
            {
                return;
            }
            if (melody == null || melody.Notes == null || melody.Notes.Count == 0)
            {
                return;
            }

            piano.ResetHighlight(_highlightedKeys);
            _highlightedKeys.Clear();


            var minKey = pressedKey + melody.MinInterval;
            var maxKey = pressedKey + melody.MaxInterval;

            _highlightedKeys.Add(minKey);
            _highlightedKeys.Add(maxKey);

            piano.SetHighlight(_highlightedKeys);


        }

        /// <summary>
        /// setPlayedColor
        /// </summary>
        /// <param name="oldMelody"></param>
        /// <param name="oldPressedKey"></param>
        /// <param name="setPlayedColor"></param>
        private void StopPlay(PianoController piano, DomainMelody oldMelody, DomainPianoNote oldPressedKey, bool setPlayedColor)
        {

            for (int i = 0; i < oldMelody.CordLength; i++)
            {
                var note = oldMelody.Notes[i];
                var key = oldPressedKey + note.Interval;
                if (0 <= key.Index && key.Index < piano.KeyCount)
                {
                    piano.Stop(key, setPlayedColor);
                }
            }

            for (int i = oldMelody.CordLength; i < oldMelody.Notes.Count; i++)
            {
                var note = oldMelody.Notes[i];
                if (note.Beats == 0)
                {
                    break;
                }
                var key = oldPressedKey + note.Interval;
                if (0 <= key.Index && key.Index < piano.KeyCount)
                {
                    piano.Stop(key, setPlayedColor);
                }
            }
        }

        /// <summary>
        /// メトロノーム音をPlayOneShotで再生（制御不要なワンショット再生）
        /// </summary>
        private void PlayMetronomeSound()
        {
            MetronomeAudioSource.PlayOneShot(_metronomeClip, VolumeManager.Instance.Volume);
        }

        private bool IsMelodyPlayableWithinRange(DomainMelody melody, DomainPianoNote rootKey)
        {
            var piano = PianoController.Instance;
            if (piano == null || melody == null)
            {
                return false;
            }
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
            StopMelody(false);

            // 新ルート設定・再開
            _currentRootKey = transposed;
            var piano = PianoController.Instance;
            StartCoroutine(PlayMelodyLoopCoroutine(piano, _currentMelody));
        }

        private void OnEnable()
        {
            // AutoKeyChangeManager の状態変化を受け取る
            AutoKeyChangeManager.Instance.OnStateChanged += HandleAutoKeyChangeState;
            // 起動時に現在の状態で同期
            HandleAutoKeyChangeState(AutoKeyChangeManager.Instance.State);
        }

        private void OnDisable()
        {
            if (AutoKeyChangeManager.Instance != null)
            {
                AutoKeyChangeManager.Instance.OnStateChanged -= HandleAutoKeyChangeState;
            }
        }

    }



}