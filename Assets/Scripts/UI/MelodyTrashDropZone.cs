
using Assets.Scripts.UI.Melody;
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
            if (!eventData.pointerDrag.TryGetComponent<MelodyReorderItem>(out var item))
            {
                return;
            }

            if (!MelodyPlayer.Instance.CanDeleteMelody(item.Melody))
            {
                item.ResetToDragStart();
                SimpleModalWindow.Create(ignorable: false)
                    .SetHeader("Error")
                    .SetBody("This melody cannot be deleted")
                    .AddButton("OK", () => { }, ModalButtonType.Success)
                    .Show();
                return;
            }

            SimpleModalWindow.Create(ignorable: false)
               .SetHeader("Confirm Deletion")
               .SetBody("Are you sure you want to delete this melody?")
               .AddButton("OK", () => {
                   _builder.HandleTrashDrop(item); // Execute only when OK is pressed
               }, ModalButtonType.Danger)
               .AddButton("Cancel", () => {
                   item.ResetToDragStart();
               }, ModalButtonType.Success)
               .Show();
        }
    }
}