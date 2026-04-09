using AsseScripts.Domain;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// 1つのステップボックス。押した鍵の名前を表示する。
    /// </summary>
    public sealed class NoteBoxUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _background;

        [SerializeField] private Color _chordDefaultColor = new Color(0.75f, 0.88f, 1f);
        [SerializeField] private Color _chordSelectedColor = new Color(0.2f, 0.55f, 1f);
        [SerializeField] private Color _noteColor = new Color(0.78f, 1f, 0.78f);
        [SerializeField] private Color _extendColor = new Color(1f, 0.93f, 0.7f);
        [SerializeField] private Color _cursorColor = new Color(1f, 1f, 0.5f);

        public StepEntry Entry { get; private set; }
        public bool IsChordBox { get; private set; }

        public event Action<NoteBoxUI> Clicked;

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => Clicked?.Invoke(this));
            }
        }

        public void Initialize(bool isChordBox)
        {
            IsChordBox = isChordBox;
            Entry = null;
            RefreshDisplay();
        }

        public void SetEntry(StepEntry entry)
        {
            Entry = entry;
            RefreshDisplay();
        }

        public void SetSelected(bool selected)
        {
            if (!IsChordBox) return;
            _background.color = selected ? _chordSelectedColor : _chordDefaultColor;
        }

        public void SetCursor(bool isCursor)
        {
            if (IsChordBox || Entry != null) return;
            _background.color = isCursor ? _cursorColor : Color.gray;
        }

        private void RefreshDisplay()
        {
            if (Entry == null)
            {
                _label.text = "?";
                _background.color = IsChordBox ? _chordDefaultColor : Color.gray;
                return;
            }

            if (Entry is NoteStep noteStep)
            {
                _label.text = FormatKeyName(noteStep.Key);
                _background.color = IsChordBox ? _chordDefaultColor : _noteColor;
            }
            else if (Entry is ExtendStep)
            {
                _label.text = "→";
                _background.color = _extendColor;
            }
        }

        private static string FormatKeyName(PianoNote key)
        {
            return key.Note.ToString().Replace("Sharp", "#");
        }
    }
}
