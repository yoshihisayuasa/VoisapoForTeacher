using AsseScripts.Domain;
using Assets.Scripts.UI.Piano;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// ステップ入力シーケンサーUI。
    /// コード3ボックスとメロディボックスをシーン起動時に全て生成する。
    /// カーソル位置に鍵盤入力・→・削除を行う。
    /// </summary>
    public sealed class NoteSequencerUI : MonoBehaviour
    {
        [SerializeField] private Transform _chordContainer;
        [SerializeField] private Transform _noteContainer;
        [SerializeField] private GameObject _boxPrefab;
        [SerializeField] private Button _extendButton;
        [SerializeField] private Button _deleteButton;

        [SerializeField] private int _melodyBoxCount = 26;

        private readonly List<NoteBoxUI> _chordBoxes = new();
        private readonly List<NoteBoxUI> _melodyBoxes = new();
        private readonly List<DomainPianoNote> _enteredChordNotes = new();

        // 次に入力されるメロディボックスのインデックス
        private int _cursorIndex = 0;

        private bool _isChordMode = true;

        private void Start()
        {
            InitChordBoxes();
            InitMelodyBoxes();

            _extendButton.onClick.AddListener(OnExtendClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);

            PianoController.Instance.OnAnyKeyClickAsObservable
                .Subscribe(OnPianoKeyClicked)
                .AddTo(this);

            RefreshCursor();
            RefreshExtendButton();
        }

        // ── 初期化 ──────────────────────────────────────────────────────

        private void InitChordBoxes()
        {
            for (int i = 0; i < Chord.Length; i++)
            {
                var box = CreateBox(_chordContainer, isChordBox: true);
                _chordBoxes.Add(box);
            }
        }

        private void InitMelodyBoxes()
        {
            for (int i = 0; i < _melodyBoxCount; i++)
            {
                var box = CreateBox(_noteContainer, isChordBox: false);
                _melodyBoxes.Add(box);
            }
        }

        // ── ピアノ入力 ───────────────────────────────────────────────────

        private void OnPianoKeyClicked(DomainPianoNote key)
        {
            if (_isChordMode)
            {
                InputToChordBox(key);
            }
            else
            {
                AddMelodyNote(key);
            }

            RefreshExtendButton();
        }

        private void InputToChordBox(DomainPianoNote key)
        {
            _enteredChordNotes.Add(key);
            SortAndApplyChordNotes();

            if (_enteredChordNotes.Count >= Chord.Length)
            {
                ExitChordMode();
            }
        }

        private void SortAndApplyChordNotes()
        {
            // 高い音が上、低い音が下になるよう降順ソート
            _enteredChordNotes.Sort((a, b) => b.Index.CompareTo(a.Index));

            // 入力済み音を下詰めで表示する
            int offset = Chord.Length - _enteredChordNotes.Count;
            for (int i = 0; i < Chord.Length; i++)
            {
                int noteIdx = i - offset;
                if (noteIdx >= 0)
                {
                    var step = new NoteStep(_enteredChordNotes[noteIdx]);
                    _chordBoxes[i].SetEntry(step);
                    MelodyCreateManager.Instance.SetChordNote(i, _enteredChordNotes[noteIdx]);
                }
                else
                {
                    _chordBoxes[i].SetEntry(null);
                }
            }
        }

        private void AddMelodyNote(DomainPianoNote key)
        {
            if (_cursorIndex >= _melodyBoxCount) return;

            var entry = new NoteStep(key);
            MelodyCreateManager.Instance.AddMelodyStep(entry);
            _melodyBoxes[_cursorIndex].SetEntry(entry);

            _cursorIndex++;
            RefreshCursor();
        }

        // ── ボタンハンドラ ───────────────────────────────────────────────

        private void OnExtendClicked()
        {
            if (_isChordMode) return;
            if (_cursorIndex >= _melodyBoxCount) return;

            var entry = new ExtendStep();
            MelodyCreateManager.Instance.AddMelodyStep(entry);
            _melodyBoxes[_cursorIndex].SetEntry(entry);

            _cursorIndex++;
            RefreshCursor();
            RefreshExtendButton();
        }

        private void OnDeleteClicked()
        {
            if (_cursorIndex == 0) return;

            MelodyCreateManager.Instance.RemoveLastMelodyStep();

            _cursorIndex--;
            _melodyBoxes[_cursorIndex].SetEntry(null);
            RefreshCursor();
            RefreshExtendButton();
        }

        // ── 状態管理 ─────────────────────────────────────────────────────

        private void ExitChordMode()
        {
            _isChordMode = false;
        }

        private void RefreshCursor()
        {
            for (int i = 0; i < _melodyBoxes.Count; i++)
            {
                _melodyBoxes[i].SetCursor(i == _cursorIndex);
            }
        }

        private void RefreshExtendButton()
        {
            _extendButton.interactable = !_isChordMode && _cursorIndex < _melodyBoxCount;
        }

        // ── ヘルパー ─────────────────────────────────────────────────────

        private NoteBoxUI CreateBox(Transform parent, bool isChordBox)
        {
            var obj = Instantiate(_boxPrefab, parent);
            var box = obj.GetComponent<NoteBoxUI>();
            box.Initialize(isChordBox);
            return box;
        }
    }
}
