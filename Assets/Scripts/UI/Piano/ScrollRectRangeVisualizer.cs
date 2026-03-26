using UnityEngine;
using UnityEngine.UI;

namespace AsseScripts.UI.Piano
{
    /// <summary>
    /// ScrollRect 上の任意のキー範囲を最小スクロールで可視化する計算を担う静的クラス。
    /// </summary>
    public static class ScrollRectRangeVisualizer
    {
        /// <summary>
        /// minKeyRT と maxKeyRT の範囲が viewport に収まるための
        /// horizontalNormalizedPosition を計算して返す。
        /// 既に表示範囲内であれば currentNormalized をそのまま返す。
        /// content width &lt;= viewport width のときは 0f を返す。
        /// </summary>
        public static float CalcNormalizedPosition(
            RectTransform content,
            RectTransform viewport,
            RectTransform minKeyRT,
            RectTransform maxKeyRT,
            float currentNormalized)
        {
            float contentScaleX = content.localScale.x;
            if (contentScaleX <= 0f) contentScaleX = 1f;

            float visualContentWidth = content.rect.width * contentScaleX;
            float visualViewportWidth = viewport.rect.width;
            if (visualContentWidth <= visualViewportWidth)
            {
                return 0f;
            }

            float visualScrollable = visualContentWidth - visualViewportWidth;

            GetKeyEdgesVisual(minKeyRT, content, contentScaleX, out float minLeft, out float minRight);
            GetKeyEdgesVisual(maxKeyRT, content, contentScaleX, out float maxLeft, out float maxRight);

            float currentLeft = currentNormalized * visualScrollable;
            float currentRight = currentLeft + visualViewportWidth;

            bool minOutsideLeft = minLeft < currentLeft;
            bool maxOutsideRight = maxRight > currentRight;

            if (!minOutsideLeft && !maxOutsideRight)
            {
                return currentNormalized;
            }

            float minCenterX = GetKeyCenterLocalX(minKeyRT, content);
            float maxCenterX = GetKeyCenterLocalX(maxKeyRT, content);

            float contentWidth = content.rect.width;
            float pivotOffset = contentWidth * content.pivot.x;
            float minCenterLeftBasis = minCenterX + pivotOffset;
            float maxCenterLeftBasis = maxCenterX + pivotOffset;

            float rangeCenterX = (minCenterLeftBasis + maxCenterLeftBasis) * 0.5f;
            float visualRangeCenter = rangeCenterX * contentScaleX;

            float visualDesiredLeft = visualRangeCenter - visualViewportWidth * 0.5f;
            visualDesiredLeft = Mathf.Clamp(visualDesiredLeft, 0f, visualScrollable);

            return visualDesiredLeft / visualScrollable;
        }

        private static float GetKeyCenterLocalX(RectTransform rt, RectTransform content)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var worldCenter = (corners[0] + corners[2]) * 0.5f;
            return content.InverseTransformPoint(worldCenter).x;
        }

        private static void GetKeyEdgesVisual(RectTransform rt, RectTransform content,
            float scaleX, out float left, out float right)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 bl = content.InverseTransformPoint(corners[0]);
            Vector3 tr = content.InverseTransformPoint(corners[2]);

            float pivotOffset = content.rect.width * content.pivot.x;
            left = (bl.x + pivotOffset) * scaleX;
            right = (tr.x + pivotOffset) * scaleX;
        }
    }
}
