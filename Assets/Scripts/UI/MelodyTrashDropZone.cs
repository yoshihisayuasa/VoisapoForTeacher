
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Modal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class MelodyTrashDropZone : MonoBehaviour, IDropHandler
    {
        [SerializeField] private PremiumLockIcon _lockIcon;

        private MelodyListBuilder _builder;

        public void Initialize(MelodyListBuilder builder)
        {
            _builder = builder;
        }

        private void Start()
        {
            // ゴミ箱はボタンではないので PremiumGateButton は付けられない。
            // 鍵の表示だけ PremiumLockIcon に任せ、ドロップ確定のゲートと同じ MelodyDelete で判定する。
            _lockIcon.Bind(new FeatureGate(PremiumFeature.MelodyDelete));
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag.TryGetComponent<DraggableMelodyButton>(out var item))
            {
                return;
            }

            if (item.Entry.IsProtected)
            {
                item.ResetToDragStart();
                ConfirmModalUI.Show("You cannot delete this melody.");
                return;
            }

            // 削除はプレミアム限定。ドロップ確定のここでゲートする。
            EntitlementManager.Instance.Current.CurrentValue.Gate(
                PremiumFeature.MelodyDelete,
                onAllowed: () => ConfirmDelete(item),
                onDenied: () => DenyDelete(item));
        }

        private void ConfirmDelete(DraggableMelodyButton item)
        {
            ConfirmModalUI.Show(
                "May I delete this melody?",
                onConfirm: () => _builder.HandleTrashDrop(item),
                onCancel: () => item.ResetToDragStart());
        }

        private void DenyDelete(DraggableMelodyButton item)
        {
            item.ResetToDragStart();
            PremiumPurchaseModalUI.Show();
        }
    }
}
