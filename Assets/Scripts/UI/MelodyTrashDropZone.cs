
using Scripts.UI.Melody;
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
                    .SetHeader("通知")
                    .SetBody("このメロディーは削除できません。")
                    .AddButton("OK", () => { }, ModalButtonType.Success)
                    .Show();
                return;
            }

            SimpleModalWindow.Create(ignorable: false)
               .SetHeader("確認")
               .SetBody("メロディーを削除してよろしいですか？")
               .AddButton("削除する", () => {
                   _builder.HandleTrashDrop(item); // はいを押したときだけ実行
               }, ModalButtonType.Danger)
               .AddButton("キャンセル", () => {
                   item.ResetToDragStart();
               }, ModalButtonType.Success)
               .Show();
        }
    }
}