using R3;
using Scripts.Domain;
using Scripts.UI.Melody;
using Scripts.UI.Piano;
using System.Collections.Generic;
using System.Linq; // Added for event stream bundling
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ScrollRect
using DomainMelody = Scripts.Domain.Melody;



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

        private readonly List<PianoNote> _highlightedKeys = new();
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
            var keyReleaseStream = _pianoKeys.Select(k => k.OnReleaseKeyAsObservable).Merge();
            var keyOnEnterStream = _pianoKeys.Select(k => k.OnPointerEnterAsObservable).Merge();

            keyClickStream
                .Subscribe(key =>
                {
                    Debug.Log($"[Catch In Controller] Pressed Key is : {key}");
                    var melody = MelodyManager.Instance.CurrentMelody;

                    MelodyPlayer.Instance.StopMelody(true);
                    TeacherSideManager.Instance.TeacherSideButtonState = false;
                    MelodyPlayer.Instance.HighlightMinMaxKeys(melody, key);
                    MelodyPlayer.Instance.PlayMelody(melody, key);
                    EnsureRangeVisible(melody, key);
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
        /// メロディの最小/最大インターバルに基づくキー範囲を実際の RectTransform 位置から算出し、
        /// 必要最小限のスクロールで範囲を可視化。
        /// </summary>
        public void EnsureRangeVisible(DomainMelody melody, PianoNote pressedKey)
        {
            PianoNote minKey = pressedKey + melody.MinInterval;
            PianoNote maxKey = pressedKey + melody.MaxInterval;
            
            if (minKey.Index < 0 || maxKey.Index >= KeyCount)
            {
                return ;
            }

            var content = _scrollRect.content;
            var viewport = _scrollRect.viewport;

            var minRT = GetKeyUI(minKey).transform as RectTransform;
            var maxRT = GetKeyUI(maxKey).transform as RectTransform;
            if (minRT == null || maxRT == null)
            {
                return;
            }
            float minCenterX = GetKeyCenterLocalX(minRT, content);
            float maxCenterX = GetKeyCenterLocalX(maxRT, content);

            float contentWidth = content.rect.width;
            float pivotOffset = contentWidth * content.pivot.x;
            float minCenterLeftBasis = minCenterX + pivotOffset;
            float maxCenterLeftBasis = maxCenterX + pivotOffset;

            float rangeCenterX = (minCenterLeftBasis + maxCenterLeftBasis) * 0.5f;

            float contentScaleX = content.localScale.x;
            if (contentScaleX <= 0f) contentScaleX = 1f;

            float visualContentWidth = content.rect.width * contentScaleX;
            float visualViewportWidth = viewport.rect.width;
            if (visualContentWidth <= visualViewportWidth)
            {
                _scrollRect.horizontalNormalizedPosition = 0f;
                return;
            }

            float visualScrollable = visualContentWidth - visualViewportWidth;

            GetKeyEdgesVisual(minRT, content, contentScaleX, out float minLeft, out float _);
            GetKeyEdgesVisual(maxRT, content, contentScaleX, out float _, out float maxRight);

            float currentLeft = _scrollRect.horizontalNormalizedPosition * visualScrollable;
            float currentRight = currentLeft + visualViewportWidth;

            bool minOutsideLeft = (minLeft < currentLeft);
            bool maxOutsideRight = (maxRight > currentRight);

            if (!minOutsideLeft && !maxOutsideRight)
            {
                return;
            }

            float visualRangeCenter = rangeCenterX * contentScaleX;

            float visualDesiredLeft = visualRangeCenter - visualViewportWidth * 0.5f;
            visualDesiredLeft = Mathf.Clamp(visualDesiredLeft, 0f, visualScrollable);

            float normalized = visualDesiredLeft / visualScrollable;
            _scrollRect.horizontalNormalizedPosition = normalized;

            float GetKeyCenterLocalX(RectTransform rt, RectTransform targetContent)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                var worldCenter = (corners[0] + corners[2]) * 0.5f;
                return targetContent.InverseTransformPoint(worldCenter).x;
            }

            void GetKeyEdgesVisual(RectTransform rt, RectTransform targetContent, float scaleX, out float left, out float right)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                Vector3 bl = targetContent.InverseTransformPoint(corners[0]);
                Vector3 tr = targetContent.InverseTransformPoint(corners[2]);

                float pivotOffset = targetContent.rect.width * targetContent.pivot.x; // 左端0基準に補正
                left = (bl.x + pivotOffset) * scaleX;
                right = (tr.x + pivotOffset) * scaleX;
            }

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

            foreach (var key in _highlightedKeys)
            {
                GetKeyUI(key).ResetHighlightedColor();
            }
            _highlightedKeys.Clear();

            _highlightedKeys.Add(minKey);
            _highlightedKeys.Add(maxKey);

            GetKeyUI(minKey).SetMinHighlightColor();
            GetKeyUI(maxKey).SetMaxHighlightColor();
        }

    }

}
