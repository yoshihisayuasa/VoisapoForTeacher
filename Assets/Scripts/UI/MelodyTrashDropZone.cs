
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Modal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class MelodyTrashDropZone : MonoBehaviour, IDropHandler
    {
        private MelodyListBuilder _builder;

        public void Initialize(MelodyListBuilder builder)
        {
            _builder = builder;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag.TryGetComponent<DraggableMelodyButton>(out var item))
            {
                return;
            }

            if (!MelodyPlayer.Instance.CanDeleteMelody(item.Entry.Melody))
            {
                item.ResetToDragStart();
                ConfirmModalUI.Show("You cannot delete this melody.");
                return;
            }

            ConfirmModalUI.Show(
                "May I delete this melody?",
                onConfirm: () => _builder.HandleTrashDrop(item),
                onCancel: () => item.ResetToDragStart());
        }
    }
}
