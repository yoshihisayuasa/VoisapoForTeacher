
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// ピアノ鍵盤の拡大縮小（ズーム）制御。
    /// - ScrollRect の content をスケール（X のみ）
    /// - ズーム時は常にビューポート中央をフォーカスしてスクロール位置を補正
    /// - Ctrl + マウスホイールでズーム
    /// </summary>
    public class PianoZoomController : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [Header("Zoom Settings")]
        [SerializeField] private float _minScale = 0.3f;
        [SerializeField] private float _maxScale = 3.0f;
        [SerializeField] private float _threshold = 0.5f;
        private const float _wheelSensitivity = 0.01f;

        public float CurrentScale => _scrollRect != null ? _scrollRect.content.localScale.x : 1f;
        private void Awake()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void Start()
        {
            if (_scrollRect?.content != null)
            {
                var ls = _scrollRect.content.localScale;
                if (ls.x <= 0f) ls.x = 1f;
                // Y は常に 1 に固定
                ls.y = 1f;
                _scrollRect.content.localScale = ls;
            }
        }

        private void Update()
        {
            if (_scrollRect == null || _scrollRect.content == null || _scrollRect.viewport == null)
            {
                return;
            }
            bool ctrl =
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (ctrl && Mathf.Abs(wheel) > _wheelSensitivity)
            {
                float current = CurrentScale;
                float target = Mathf.Clamp(current * Mathf.Exp(wheel * _threshold), _minScale, _maxScale);
                SetScale(target, 0.5f); 
            }
        }

        public void SetScale(float newScale, float focusViewportFactor = 0.5f)
        {
            if (_scrollRect == null || _scrollRect.content == null || _scrollRect.viewport == null)
            {
                return;
            }
            newScale = Mathf.Clamp(newScale, _minScale, _maxScale);

            var content = _scrollRect.content;
            var viewport = _scrollRect.viewport;

            float contentWidth = content.rect.width;
            float viewportWidth = viewport.rect.width;

            float oldScale = Mathf.Max(content.localScale.x, _wheelSensitivity);
            float oldVisualContentWidth = contentWidth * oldScale;
            float oldScrollable = Mathf.Max(oldVisualContentWidth - viewportWidth, 0f);

            float currentLeft = (oldScrollable > 0f)
                ? _scrollRect.horizontalNormalizedPosition * oldScrollable : 0f;

            float focusLeftBasis = currentLeft + viewportWidth * focusViewportFactor;
            float focusContentXUnscaled = focusLeftBasis / oldScale;
            content.localScale = new Vector3(newScale, 1f, 1f);


            float newVisualContentWidth = contentWidth * newScale;
            float newScrollable = Mathf.Max(0f, newVisualContentWidth - viewportWidth);

            float desiredLeft = focusContentXUnscaled * newScale - viewportWidth * focusViewportFactor;
            desiredLeft = Mathf.Clamp(desiredLeft, 0f, newScrollable);

            float normalized = (newScrollable > 0f) ? desiredLeft / newScrollable : 0f;
            _scrollRect.horizontalNormalizedPosition = normalized;

        }
        public void ZoomIn(float step = 0.1f)
        {
            float target = Mathf.Clamp(CurrentScale * (1f + step), _minScale, _maxScale);
            SetScale(target, 0.5f);
        }

        public void ZoomOut(float step = 0.1f)
        {
            float target = Mathf.Clamp(CurrentScale / (1f + step), _minScale, _maxScale);
            SetScale(target, 0.5f);
        }
    }
}
