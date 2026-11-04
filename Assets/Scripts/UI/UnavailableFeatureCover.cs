using Assets.Scripts.UI.Modal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 使えない機能にかぶせる透明な覆い。押されたら、その機能が使える場面をモーダルで案内する。
    /// <see cref="UnavailableFeature"/> が実行時に生成する。
    /// </summary>
    public sealed class UnavailableFeatureCover : MonoBehaviour, IPointerClickHandler
    {
        private LocalizedMessage _notice;

        public void Setup(LocalizedMessage notice)
        {
            _notice = notice;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ConfirmModalUI.Show(_notice.ForCurrentLanguage());
        }
    }
}
