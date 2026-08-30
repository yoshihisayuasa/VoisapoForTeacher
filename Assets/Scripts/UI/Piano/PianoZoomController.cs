
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// ピアノ鍵盤の拡大縮小（ズーム）の見た目への適用。
    /// - PianoScaleManager が持つ倍率を ScrollRect の content のスケール（X のみ）へ反映する
    /// - ズーム時は常にビューポート中央をフォーカスしてスクロール位置を補正
    /// - Ctrl + マウスホイールでズーム
    /// </summary>
    public sealed class PianoZoomController : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [Header("Zoom Settings")]
        private float _wheelSensitivity = 0.02f;

        private void Start()
        {
            // 復元はスケールを当てるだけにする。レイアウト確定前は viewport.rect.width が
            // 当てにならず、スクロール位置の補正計算が成立しないため。
            _scrollRect.content.localScale = new Vector3(PianoScaleManager.Instance.Scale.Value, 1f, 1f);

            PianoScaleManager.Instance.ScaleChanged
                .Subscribe(scale => ApplyScale(scale))
                .AddTo(this);
        }

        private void Update()
        {
            float wheel = ScrollWheelInput.ReadScroll() * _wheelSensitivity; // 係数は要調整

            if (ScrollWheelInput.IsCtrlOrCommandPressed() && Mathf.Abs(wheel) > 0.0f)
            {
                PianoScaleManager.Instance.Zoom(wheel);
            }
        }

        private void ApplyScale(PianoScale scale, float focusViewportFactor = 0.5f)
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
            content.localScale = new Vector3(scale.Value, 1f, 1f);

            float newVisualContentWidth = contentWidth * scale.Value;
            float newScrollable = Mathf.Max(0f, newVisualContentWidth - viewportWidth);

            float desiredLeft = focusContentXUnscaled * scale.Value - viewportWidth * focusViewportFactor;
            desiredLeft = Mathf.Clamp(desiredLeft, 0f, newScrollable);

            float normalized = (newScrollable > 0f) ? desiredLeft / newScrollable : 0f;
            _scrollRect.horizontalNormalizedPosition = normalized;

        }
        public void ZoomIn(float step = 0.1f)
        {
            PianoScaleManager.Instance.ZoomIn(step);
        }

        public void ZoomOut(float step = 0.1f)
        {
            PianoScaleManager.Instance.ZoomOut(step);
        }
    }
}
