using Assets.Scripts.UI.Melody;
using R3;
using Scripts.Domain;
using Scripts.UI.Melody;
using Scripts.UI.Piano;
using System.Collections.Generic;
using System.Linq; // Added for event stream bundling
using UnityEngine;
using UnityEngine.UI; // ScrollRect

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：鍵盤の入力管理・イベント伝達
    /// </summary>
    public sealed class PianoController : MonoBehaviour
    {
        /// <summary>
        /// 鍵盤押下時のイベント
        /// </summary>

        [SerializeField]
        [Tooltip("鍵盤一覧")] private List<PianoKeyUI>
            _pianoKeys;

        private (PianoNote Min, PianoNote Max)? _highlightedKeys;
        private readonly HashSet<PianoNote> _playingKeys = new();
        private readonly HashSet<PianoNote> _coloredKeys = new();
        private Dictionary<PianoNoteEnum, PianoKeyUI> _keyDict;
        [SerializeField]
        [Tooltip("ピアノ鍵盤を含む ScrollRect (水平スクロール)")] private ScrollRect _scrollRect;
        public IReadOnlyList<PianoKeyUI> PianoKeys => _pianoKeys;

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
        }

        private void Start()
        {
            SetEvent();
        }

        /// <summary>
        /// イベント初期化（LINQで束ねて簡潔化）
        /// </summary>
        private void SetEvent()
        {
            if (_pianoKeys == null)
            {
                Debug.LogError("PianoController: _pianoKeys is null.");
                return;
            }
            if (_pianoKeys.Count == 0)
            {
                Debug.LogWarning("PianoController: _pianoKeys is empty.");
                return;
            }

            var keyClickStream = _pianoKeys.Select(k => k.OnClickKeyAsObservable).Merge();
            var keyOnEnterStream = _pianoKeys.Select(k => k.OnPointerEnterAsObservable).Merge();

            keyClickStream
                .Subscribe(key =>
                {
                    Debug.Log($"[Catch In Controller] Pressed Key is : {key}");
                    var melody = MelodyManager.Instance.CurrentMelody;

                    TeacherSideManager.Instance.TeacherSideButtonState = false;
                    MelodyPlayer.Instance.HighlightMinMaxKeys(melody, key);
                    MelodyPlayer.Instance.PlayMelody(melody, key);
                    MelodyPlayer.Instance.EnsureKeyRangeVisible(melody, key);
                })
                .AddTo(this);

            keyOnEnterStream
                .Subscribe(key =>
                {
                    var melody = MelodyManager.Instance.CurrentMelody;
                    MelodyPlayer.Instance.HighlightMinMaxKeys(melody, key);
                    Debug.Log($"[Catch In Controller] Entered Key is : {key}");
                })
                .AddTo(this);
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
                key.PlaySound(volume);
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
            keyUI.StopSound();
            keyUI.SetKeyVisual(setPlayedColor);
        }

        public void StopMelody(bool setKeyVisual)
        {
            foreach (var key in _playingKeys)
            {
                var keyUI = GetKeyUI(key);
                keyUI.StopSound();
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
            if (minKey.Index < 0 || maxKey.Index >= KeyCount) return;

            var minRT = GetKeyUI(minKey).transform as RectTransform;
            var maxRT = GetKeyUI(maxKey).transform as RectTransform;
            if (minRT == null || maxRT == null) return;

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

        public void SetHighlight(PianoNote key1, PianoNote key2)
        {
            var minKey = key1.Index <= key2.Index ? key1 : key2;
            var maxKey = key1.Index <= key2.Index ? key2 : key1;

            if (_highlightedKeys.HasValue)
            {
                GetKeyUI(_highlightedKeys.Value.Min).ResetHighlightedColor();
                GetKeyUI(_highlightedKeys.Value.Max).ResetHighlightedColor();
            }

            _highlightedKeys = (minKey, maxKey);

            GetKeyUI(minKey).SetMinHighlightColor();
            GetKeyUI(maxKey).SetMaxHighlightColor();
        }

    }

}
