using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.MelodyUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 起動時に MelodyManager のリストからメロディーボタンを自動生成
    /// </summary>
    public sealed class MelodyListBuilder : MonoBehaviour
    {
        [SerializeField] private Button _buttonPrefab;
        [SerializeField] private List<Transform> _positionSlots;
        [SerializeField] private MelodyTrashDropZone _trashDropZone;

        private void Start()
        {
            StartCoroutine(BuildWhenReady());
        }

        private System.Collections.IEnumerator BuildWhenReady()
        {
            yield return null;

            EnsureSlotComponents();
            Build();
            MelodyManager.Instance.ResetToDefault();
        }

        public void Build()
        {
            var manager = MelodyManager.Instance;
            ClearSlots();

            var entries = manager.GetAllMelodies();
            for (int i = 0; i < entries.Count; i++)
            {
                int idx = i;
                SavedMelody entry = entries[idx];

                Transform parent = GetSlotByPosition(entry.Position);

                var btn = Instantiate(_buttonPrefab, parent);

                if (btn.transform is RectTransform rt)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }

                var reorderItem = btn.GetComponent<DraggableMelodyButton>();
                reorderItem.Initialize(entry, this, parent);

                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = string.IsNullOrEmpty(entry.Melody.Name) ? $"Melody {idx + 1}" : entry.Melody.Name;
                }

                btn.onClick.AddListener(() =>
                {
                    manager.SetCurrentMelody(entry.Melody);
                });
            }
        }

        private void EnsureSlotComponents()
        {
            for (int i = 0; i < _positionSlots.Count; i++)
            {
                var slot = _positionSlots[i];
                var drop = slot.GetComponent<MelodyDropSlot>();
                drop.Initialize(this, i);
            }

            _trashDropZone.Initialize(this);
        }

        public int GetSlotIndex(Transform slot)
        {
            return _positionSlots.IndexOf(slot);
        }

        public void HandleDrop(DraggableMelodyButton item, int targetIndex)
        {
            int sourceIndex = GetSlotIndex(item.CurrentSlot);

            if (targetIndex == sourceIndex)
            {
                item.ResetToDragStart();
                return;
            }

            int emptyIndex = -1;
            for (int i = targetIndex; i < _positionSlots.Count; i++)
            {
                if (GetItemInSlot(i) == null)
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex < 0)
            {
                item.ResetToDragStart();
                return;
            }

            for (int i = emptyIndex; i > targetIndex; i--)
            {
                var shiftItem = GetItemInSlot(i - 1);
                if (shiftItem != null)
                {
                    shiftItem.SetSlot(_positionSlots[i], i);
                }
            }

            item.SetSlot(_positionSlots[targetIndex], targetIndex);
            MelodyManager.Instance.SavePositions();
        }

        private DraggableMelodyButton GetItemInSlot(int slotIndex)
        {
            var slot = _positionSlots[slotIndex];
            return slot.childCount > 0 ? slot.GetComponentInChildren<DraggableMelodyButton>() : null;
        }

        private Transform GetSlotByPosition(int position)
        {
            if (position < 0 || position >= _positionSlots.Count) return null;
            return _positionSlots[position];
        }

        private void ClearSlots()
        {
            for (int i = 0; i < _positionSlots.Count; i++)
            {
                var slot = _positionSlots[i];
                if (slot == null) continue;

                for (int j = slot.childCount - 1; j >= 0; j--)
                {
                    Destroy(slot.GetChild(j).gameObject);
                }
            }
        }

        public void HandleTrashDrop(DraggableMelodyButton item)
        {
            var entry = item.Entry;
            Destroy(item.gameObject);
            MelodyManager.Instance.RemoveMelody(entry);
            Build();
        }
    }
}
