
using Assets.Scripts.UI;
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
    public sealed class PianoZoomController : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [Header("Zoom Settings")]
        private readonly float _minScale = 0.3f;
        private readonly float _maxScale = 3.0f;
        private float _wheelSensitivity = 0.02f;

        public float MinScale => _minScale;
        public float MaxScale => _maxScale;
        public float CurrentScale => _scrollRect.content.localScale.x;

        private void Start()
        {
            var ls = _scrollRect.content.localScale;
            if (ls.x <= 0f) ls.x = 1f;
            // Y は常に 1 に固定
            ls.y = 1f;
            _scrollRect.content.localScale = ls;
        }

        private void Update()
        {
            float wheel = ScrollWheelInput.ReadScroll() * _wheelSensitivity; // 係数は要調整

            if (ScrollWheelInput.IsCtrlOrCommandPressed() && Mathf.Abs(wheel) > 0.0f)
            {
                float target = Mathf.Clamp(CurrentScale + wheel, _minScale, _maxScale);
                SetScale(target, 0.5f);
            }
        }

        public void SetScale(float newScale, float focusViewportFactor = 0.5f)
        {
            var content = _scrollRect.content;
            var viewport = _scrollRect.viewport;

            float contentWidth = content.rect.width;
            float viewportWidth = viewport.rect.width;

            float oldScale = ScrollRectGeometry.ScaleX(content);
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
