using Assets.Scripts.Domain.ValueObjects;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// 1つのステップボックス。押した鍵の名前を表示する。
    /// </summary>
    public sealed class NoteBoxUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public DraftNote Entry { get; private set; }
        public bool IsChordBox { get; private set; }

        public void Initialize(bool isChordBox)
        {
            IsChordBox = isChordBox;
            Entry = null;
            RefreshDisplay();
        }

        public void SetEntry(DraftNote entry)
        {
            Entry = entry;
            RefreshDisplay();
        }

        public void ShowArrow()
        {
            Entry = null;
            _label.text = "→";
        }

        private void RefreshDisplay()
        {
            _label.text = Entry?.DisplayText ?? "";
        }
    }
}
