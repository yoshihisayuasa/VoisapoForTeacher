using R3;
using AsseScripts.Domain;
using AsseScripts.Infrastructure;
using AsseScripts.UI;
using AsseScripts.UI.Piano;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：鍵盤の入力管理・イベント伝達
    /// </summary>
    public sealed class PianoController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("鍵盤一覧")] private List<PianoKeyUI>
            _pianoKeys;

        private (PianoNote Min, PianoNote Max)? _highlightedKeys;
        private PianoNote _selectedKey;
        private readonly HashSet<PianoNote> _playingKeys = new();
        private readonly HashSet<PianoNote> _coloredKeys = new();
        private Dictionary<PianoNoteEnum, PianoKeyUI> _keyDict;

        private Subject<PianoNote> _virtualKeyClicks;
        private Subject<PianoNote> _virtualKeyUps;
        private Subject<PianoNote> _virtualKeyEnters;

        [SerializeField]
        [Tooltip("ピアノ鍵盤を含む ScrollRect (水平スクロール)")] private ScrollRect _scrollRect;
        public void ApplySoundSet(PianoSoundSet soundSet)
        {
            foreach (var keyUI in _pianoKeys)
            {
                keyUI.SwapAudioClip(soundSet.GetClip(keyUI.NoteEnum));
            }
        }

        public static PianoController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _keyDict = new Dictionary<PianoNoteEnum, PianoKeyUI>(_pianoKeys.Count);
            foreach (var key in _pianoKeys)
            {
                _keyDict[key.NoteEnum] = key;
            }

            _virtualKeyClicks = new Subject<PianoNote>();
            _virtualKeyUps    = new Subject<PianoNote>();
            _virtualKeyEnters = new Subject<PianoNote>();

            OnAnyKeyUpAsObservable    = _pianoKeys.Select(k => k.OnPointerUpAsObservable).Merge().Merge(_virtualKeyUps);
            OnAnyKeyClickAsObservable = _pianoKeys.Select(k => k.OnClickKeyAsObservable).Merge().Merge(_virtualKeyClicks);
            OnAnyKeyEnterAsObservable = _pianoKeys.Select(k => k.OnPointerEnterAsObservable).Merge().Merge(_virtualKeyEnters);
        }

        private void Start()
        {
            SetAccent(new PianoNote(PianoNoteEnum.C4));
            OnAnyKeyClickAsObservable.Subscribe(SetAccent).AddTo(this);
        }

        private void OnDisable()
        {
            ResetAccent();
        }

        /// <summary>
        /// // シングルトン重複検知で Awake を早期 return した場合、Subject が未初期化のため nullチェックが必要
        /// </summary>
        private void OnDestroy()
        {
            _virtualKeyClicks?.Dispose();
            _virtualKeyUps?.Dispose();
            _virtualKeyEnters?.Dispose();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.sKey.wasPressedThisFrame||kb.jKey.wasPressedThisFrame) MoveSelection(-1);
            if (kb.fKey.wasPressedThisFrame||kb.lKey.wasPressedThisFrame) MoveSelection(1);
            if (kb.gKey.wasPressedThisFrame || kb.semicolonKey.wasPressedThisFrame) MoveSelection(-6);
            if (kb.aKey.wasPressedThisFrame || kb.hKey.wasPressedThisFrame) MoveSelection(6);
            if ((kb.dKey.wasPressedThisFrame||kb.kKey.wasPressedThisFrame) && _selectedKey != null) _virtualKeyClicks.OnNext(_selectedKey);
            if ((kb.dKey.wasReleasedThisFrame||kb.kKey.wasReleasedThisFrame) && _selectedKey != null) _virtualKeyUps.OnNext(_selectedKey);
        }

        private void MoveSelection(int delta)
        {
            Debug.Assert(_selectedKey != null, "_selectedKey is null");
            int currentIndex = _selectedKey.Index;
            int nextIndex = Mathf.Clamp(currentIndex + delta, 0, KeyCount - 1);
            var nextKey = new PianoNote((PianoNoteEnum)nextIndex);

            SetAccent(nextKey);
            _virtualKeyEnters.OnNext(nextKey);
        }

        public Observable<PianoNote> OnAnyKeyUpAsObservable { get; private set; }
        public Observable<PianoNote> OnAnyKeyClickAsObservable { get; private set; }
        public Observable<PianoNote> OnAnyKeyEnterAsObservable { get; private set; }

        public PianoNote SelectedKey
        {
            get => _selectedKey;
            set => SetAccent(value);
        }

        public int KeyCount => _pianoKeys.Count;

        /// <summary>
        /// 視覚は常に更新し、音再生は isPlaySound で制御する。
        /// </summary>
        public void Play(PianoNote pressedKey, bool isPlaySound, float volume)
        {
            _playingKeys.Add(pressedKey);
            var key = GetKeyUI(pressedKey);
            key.SetPlayingVisual();
            if (isPlaySound)
            {
                key.PlaySound(volume * SoundSourceSwitcher.Instance.CurrentVolumeMultiplier);
            }
        }

        public void Stop(PianoNote key, bool setPlayedColor)
        {
            _playingKeys.Remove(key);
            if (setPlayedColor)
            {
                _coloredKeys.Add(key);
            }
            else
            {
                _coloredKeys.Remove(key);
            }
            var keyUI = GetKeyUI(key);
            keyUI.StopSound(BPMManager.Instance.SecondPerBeat * 0.6f);
            keyUI.SetKeyVisual(setPlayedColor);
        }

        public void StopMelody(bool setKeyVisual)
        {
            float fadeOut = BPMManager.Instance.SecondPerBeat * 0.6f;
            foreach (var key in _playingKeys)
            {
                var keyUI = GetKeyUI(key);
                keyUI.StopSound(fadeOut);
                if(setKeyVisual)
                {
                    keyUI.SetKeyVisual(false);
                }
                else
                {
                    _coloredKeys.Add(key);
                }
            }
            _playingKeys.Clear();

            if (setKeyVisual)
            {
                foreach (var key in _coloredKeys)
                {
                    var keyUI = GetKeyUI(key);
                    keyUI.SetKeyVisual(false);
                }
                _coloredKeys.Clear();
            }
        }

        /// <summary>
        /// 指定したキー範囲が viewport に収まるよう ScrollRect をスクロールする。
        /// </summary>
        public void EnsureRangeVisible(PianoNote minKey, PianoNote maxKey)
        {
            if (minKey.Index < 0 || maxKey.Index >= KeyCount)
            {
                return;
            }

            var minRT = GetKeyUI(minKey).transform as RectTransform;
            var maxRT = GetKeyUI(maxKey).transform as RectTransform;
            if (minRT == null || maxRT == null)
            {
                return;
            }

            _scrollRect.horizontalNormalizedPosition =
                ScrollRectRangeVisualizer.CalcNormalizedPosition(
                    _scrollRect.content,
                    _scrollRect.viewport,
                    minRT,
                    maxRT,
                    _scrollRect.horizontalNormalizedPosition);
        }
        /// <summary>
        /// インデックスが鍵盤配列の範囲内か判定します。
        /// </summary>

        public PianoKeyUI GetKeyUI(PianoNote pressedKey)
        {
            return _keyDict[pressedKey.Note];
        }

        public void SetAccent(PianoNote key)
        {
            if (_selectedKey != null)
            {
                GetKeyUI(_selectedKey).ResetAccentColor();
                RestoreHighlightIfNeeded(_selectedKey);
            }
            _selectedKey = key;
            GetKeyUI(_selectedKey).SetAccentColor();
        }

        public void ResetAccent()
        {
            if (_selectedKey == null) return;
            GetKeyUI(_selectedKey).ResetAccentColor();
            RestoreHighlightIfNeeded(_selectedKey);
            _selectedKey = null;
        }

        private void RestoreHighlightIfNeeded(PianoNote key)
        {
            if (!_highlightedKeys.HasValue) return;
            if (key == _highlightedKeys.Value.Min)
                GetKeyUI(key).SetMinHighlightColor();
            else if (key == _highlightedKeys.Value.Max)
                GetKeyUI(key).SetMaxHighlightColor();
        }

        public void SetHighlight(PianoNote key1, PianoNote key2)
        {
            var minKey = key1.Index <= key2.Index ? key1 : key2;
            var maxKey = key1.Index <= key2.Index ? key2 : key1;

            if (_highlightedKeys.HasValue)
            {
                var oldMin = _highlightedKeys.Value.Min;
                var oldMax = _highlightedKeys.Value.Max;
                if (oldMin != _selectedKey) GetKeyUI(oldMin).ResetHighlightedColor();
                if (oldMax != _selectedKey) GetKeyUI(oldMax).ResetHighlightedColor();
            }

            _highlightedKeys = (minKey, maxKey);

            if (minKey != _selectedKey) GetKeyUI(minKey).SetMinHighlightColor();
            if (maxKey != _selectedKey) GetKeyUI(maxKey).SetMaxHighlightColor();
        }

    }

}
