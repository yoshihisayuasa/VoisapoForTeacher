using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// ScrollRect の content 内部（スケール・寸法）に踏み込む幾何計算の置き場。
    /// 「見た目上の幅 = width × scale、スケール未初期化（0以下）は 1 とみなす」のルールは
    /// スクロール・ズーム・範囲可視化の3クラスが共有するため、定義はここ1箇所だけが持つ。
    /// </summary>
    public static class ScrollRectGeometry
    {
        /// <summary>content の X スケール。未初期化（0以下）は 1 とみなす。</summary>
        public static float ScaleX(RectTransform content)
        {
            float scale = content.localScale.x;
            return scale > 0f ? scale : 1f;
        }

        /// <summary>スケール適用後の content の見た目上の幅。</summary>
        public static float VisualContentWidth(ScrollRect scrollRect)
        {
            var content = scrollRect.content;
            return content.rect.width * ScaleX(content);
        }
    }
}
