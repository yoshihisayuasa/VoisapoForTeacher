using UnityEngine;
using UnityEngine.EventSystems;
using DomainMelody = Scripts.Domain.Melody;

namespace Assets.Scripts.UI
{
    public sealed class MelodyReorderItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private MelodyListBuilder _builder;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private RectTransform _dragRoot;
        private Transform _dragStartSlot;
        private int _dragStartIndex;

        public DomainMelody Melody { get; private set; }
        public Transform CurrentSlot { get; private set; }

        public void Initialize(DomainMelody melody, MelodyListBuilder builder, Transform slot)
        {
            Melody = melody;
            _builder = builder;
            CurrentSlot = slot;

            _rectTransform = transform as RectTransform;
            _canvasGroup = GetComponent<CanvasGroup>();

            var canvas = GetComponentInParent<Canvas>();
            _dragRoot =  canvas.transform as RectTransform;

            SetSlot(slot, builder.GetSlotIndex(slot));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragStartSlot = CurrentSlot;
            _dragStartIndex = _builder.GetSlotIndex(CurrentSlot);

            _canvasGroup.blocksRaycasts = false;
            transform.SetParent(_dragRoot, true);
        }

        public void OnDrag(PointerEventData eventData)
        {

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dragRoot, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        /// <summary>
        ///pointerEnter は Canvas 外でドロップすると null になる
        // → null チェックなしだと NullReferenceException
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            if (eventData.pointerEnter == null)
            {
                ResetToDragStart();
                return;
            }

            if (eventData.pointerEnter.GetComponentInParent<MelodyTrashDropZone>() != null)
            {
                return;
            }


            var slot = eventData.pointerEnter.GetComponentInParent<MelodyDropSlot>();
            if (slot == null)
            {
                ResetToDragStart();
            }
        }

        /// <summary>
        ///ドロップ先が無効だった場合や、入れ替え処理が成立しない場合にドラッグ開始時のスロットへ戻す。
        /// </summary>
        public void ResetToDragStart()
        {
            if (_dragStartSlot == null) return;
            SetSlot(_dragStartSlot, _dragStartIndex);
        }

        public void SetSlot(Transform slot, int slotIndex)
        {
            if (slot == null) return;

            CurrentSlot = slot;
            transform.SetParent(slot, false);

            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = Vector2.zero;
                _rectTransform.localRotation = Quaternion.identity;
                _rectTransform.localScale = Vector3.one;
            }

            if (Melody != null && slotIndex >= 0)
            {
                Melody.SetPosition(slotIndex);
            }
        }
    }
}