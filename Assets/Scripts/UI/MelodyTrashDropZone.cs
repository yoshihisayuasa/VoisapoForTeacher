
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
            var item = eventData.pointerDrag.GetComponent<MelodyReorderItem>();
            if (item == null)
            {
                return;
            }

            SimpleModalWindow.Create(ignorable: false)
               .SetHeader("確認")
               .SetBody("メロディーを削除してよろしいですか？")
               .AddButton("削除する", () => {
                   _builder.HandleTrashDrop(item); // はいを押したときだけ実行
               }, ModalButtonType.Danger)
               .AddButton("キャンセル", () => {
                   // 何もしない
               }, ModalButtonType.Success)
               .Show();
        }
    }
}