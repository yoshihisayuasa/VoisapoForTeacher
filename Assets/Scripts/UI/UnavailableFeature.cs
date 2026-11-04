using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 今のレッスンモードでは使えない機能。
    /// 消すと「なぜなくなったのか」が伝わらないため、薄く表示したまま残し、押されたら使える場面を案内する。
    /// </summary>
    public sealed class UnavailableFeature
    {
        /// <summary>使えない機能の不透明度。押せないと一目でわかる薄さにする。</summary>
        private const float DimmedAlpha = 0.4f;

        private readonly GameObject _target;
        private readonly LocalizedMessage _notice;

        public UnavailableFeature(GameObject target, LocalizedMessage notice)
        {
            _target = target;
            _notice = notice;
        }

        /// <summary>
        /// 薄く表示し、中のボタンには触れさせず、押されたら案内を出す覆いをかぶせる。
        /// </summary>
        public void Disable()
        {
            var group = _target.AddComponent<CanvasGroup>();
            group.alpha = DimmedAlpha;
            group.interactable = false;

            CreateCover();
        }

        // 中のボタンは押せなくてもクリックを受け止めてしまうため、最前面に透明な覆いを置いてクリックを横取りする。
        private void CreateCover()
        {
            var cover = new GameObject(
                "UnavailableCover",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement),
                typeof(UnavailableFeatureCover));

            var rect = cover.GetComponent<RectTransform>();
            rect.SetParent(_target.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsLastSibling();

            // 透明でもクリックは受け取れる。
            cover.GetComponent<Image>().color = Color.clear;

            // 親にレイアウトグループがあっても、並びに割り込まない。
            cover.GetComponent<LayoutElement>().ignoreLayout = true;

            cover.GetComponent<UnavailableFeatureCover>().Setup(_notice);
        }
    }
}
