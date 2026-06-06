using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public sealed class MelodyDropSlot : MonoBehaviour, IDropHandler
    {
        private MelodyListBuilder _builder;
        [SerializeField] private int _slotIndex = -1;

        public void Initialize(MelodyListBuilder builder, int slotIndex)
        {
            _builder = builder;
            _slotIndex = slotIndex;
        }

        public int SlotIndex => _slotIndex;

        /// <summary>
        ///ドラッグ中のオブジェクトがこのスロット上でドロップされた瞬間に呼ばれる。
        ///Unity の IDropHandler によって、EventSystem が自動で呼び出します。
        ///このとき、eventData.pointerDrag がドラッグ中の UI（DraggableMelodyButton）です。
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            var item = eventData.pointerDrag.GetComponent<DraggableMelodyButton>();
            _builder.HandleDrop(item, _slotIndex);
        }
    }
}