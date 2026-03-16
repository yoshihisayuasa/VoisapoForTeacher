using Scripts.UI.Melody;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DomainMelody = Scripts.Domain.Melody;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 起動時に MelodyManager のリストからメロディーボタンを自動生成
    /// </summary>
    public sealed class MelodyListBuilder : MonoBehaviour
    {
        [SerializeField] private Button _buttonPrefab;  // メロディー選択用ボタンのプレハブ
        [SerializeField] private List<Transform> _positionSlots; // 透明スロット(0〜26)の並び
        [SerializeField] private MelodyTrashDropZone _trashDropZone;

        private void Start()
        {
            // MelodyManager.Start() のJSON読み込み完了を待ってから生成
            StartCoroutine(BuildWhenReady());
        }

        private System.Collections.IEnumerator BuildWhenReady()
        {
            // インスタンス生成待ち
            while (MelodyManager.Instance == null)
            {
                yield return null;
            }

            // Startが走るまで1フレーム待機（JSONロード完了待ち）
            yield return null;

            EnsureSlotComponents();
            Build();
        }

        public void Build()
        {
            var manager = MelodyManager.Instance;
            ClearSlots();

            var melodies = manager.GetAllMelodies();
            for (int i = 0; i < melodies.Count; i++)
            {
                int idx = i;
                DomainMelody melody = melodies[idx];

                Transform parent = GetSlotByPosition(melody.Position);

                var btn = Instantiate(_buttonPrefab, parent);

                if (btn.transform is RectTransform rt)
                {
                    rt.anchoredPosition = Vector2.zero; // スロットの中心に配置
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }

                var reorderItem = btn.GetComponent<MelodyReorderItem>();
                reorderItem.Initialize(melody, this, parent);

                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = string.IsNullOrEmpty(melody.Name) ? $"Melody {idx + 1}" : melody.Name;
                }

                btn.onClick.AddListener(() =>
                {
                    manager.SetCurrentMelody(melody);
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

        public void HandleDrop(MelodyReorderItem item, int targetIndex)
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

        private MelodyReorderItem GetItemInSlot(int slotIndex)
        {
            var slot = _positionSlots[slotIndex];

            if (slot.childCount > 0)
            {
                return slot.GetComponentInChildren<MelodyReorderItem>();
            }
            else
            {
                return null;
            }
        }
        private Transform GetSlotByPosition(int position)
        {
            if (position < 0 || position >= _positionSlots.Count)
            {
                return null;
            }
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
        public void HandleTrashDrop(MelodyReorderItem item)
        {
            var melody = item.Melody;
            Destroy(item.gameObject);
            MelodyManager.Instance.RemoveMelody(melody);
            Build();
        }
    }
}