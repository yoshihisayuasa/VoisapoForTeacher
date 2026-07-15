using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.MelodyUI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Button))]
    public sealed class DraggableMelodyButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private MelodyListBuilder _builder;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private RectTransform _dragRoot;
        private Transform _dragStartSlot;
        private int _dragStartIndex;
        private Button _button;

        public SavedMelody Entry { get; private set; }
        public Transform CurrentSlot { get; private set; }

        public void Initialize(SavedMelody entry, MelodyListBuilder builder, Transform slot)
        {
            Entry = entry;
            _builder = builder;
            CurrentSlot = slot;

            _rectTransform = transform as RectTransform;
            _canvasGroup = GetComponent<CanvasGroup>();
            _button = GetComponent<Button>();

            var canvas = GetComponentInParent<Canvas>();
            _dragRoot = canvas.transform as RectTransform;

            MelodyManager.Instance.MelodyChanged.Subscribe(OnMelodyChanged).AddTo(this);
            UpdateColor(MelodyManager.Instance.CurrentMelody);

            SetSlot(slot, builder.GetSlotIndex(slot));
        }

        private void OnMelodyChanged(Melody selected) => UpdateColor(selected);

        private void UpdateColor(Melody selected)
        {
            _button.image.color = AppColors.ActiveOrWhite(selected == Entry.Melody);
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
        /// pointerEnter は Canvas 外でドロップすると null になる
        /// </summary>
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

            if (Entry != null && slotIndex >= 0)
            {
                Entry.SetPosition(slotIndex);
            }
        }
    }
}
