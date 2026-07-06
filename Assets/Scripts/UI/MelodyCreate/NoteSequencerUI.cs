using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Button _clearButton;
        [SerializeField] private MelodyTemplateSelectorUI _templateSelector;

        private static int MelodyBoxCount => MelodyDraft.MaxMelodySteps;

        private readonly List<NoteBoxUI> _chordBoxes = new();
        private readonly List<NoteBoxUI> _melodyBoxes = new();
        private readonly List<PianoNote> _enteredChordNotes = new();

        // 次に入力されるメロディボックスのインデックス
        private int _cursorIndex = 0;

        private Coroutine _previewSoundCoroutine;
        private PianoNote _previewingNote;
        private const float PreviewSoundDuration = 0.5F;

        private void Start()
        {
            InitChordBoxes();
            InitMelodyBoxes();

            _extendButton.onClick.AddListener(OnExtendClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);
            _clearButton.onClick.AddListener(OnClearClicked);

            PianoController.Instance.OnRootKeyPressedAsObservable
                .Subscribe(OnPianoKeyClicked)
                .AddTo(this);

            MelodyCreateManager.Instance.TemplateLoaded
                .Subscribe(RebuildFromTemplate)
                .AddTo(this);

            RefreshButtons();
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
            for (int i = 0; i < MelodyBoxCount; i++)
            {
                var box = CreateBox(_noteContainer, isChordBox: false);
                _melodyBoxes.Add(box);
            }
        }

        // ── ピアノ入力 ───────────────────────────────────────────────────

        private void OnPianoKeyClicked(PianoNote key)
        {
            if (!MelodyCreateManager.Instance.IsChordComplete)
            {
                AddCordNote(key);
            }
            else
            {
                AddMelodyNote(key);
            }

            RefreshButtons();
            StartPreviewSound(key);
        }

        private void StartPreviewSound(PianoNote key)
        {
            if (_previewSoundCoroutine != null)
            {
                StopCoroutine(_previewSoundCoroutine);
                if (_previewingNote != null)
                {
                    PianoController.Instance.Stop(_previewingNote);
                }
            }

            _previewingNote = key;
            PianoController.Instance.Play(key, true, 1f);
            _previewSoundCoroutine = StartCoroutine(StopPreviewSoundAfterDelay(key));
        }

        private IEnumerator StopPreviewSoundAfterDelay(PianoNote key)
        {
            yield return new WaitForSeconds(PreviewSoundDuration);
            PianoController.Instance.Stop(key);
            _previewSoundCoroutine = null;
            _previewingNote = null;
        }

        private void AddCordNote(PianoNote key)
        {
            _enteredChordNotes.Add(key);
            SortAndApplyChordNotes();
        }

        private void SortAndApplyChordNotes()
        {
            MelodyCreateManager.Instance.AddChordNotes(_enteredChordNotes);

            var chordNotes = MelodyCreateManager.Instance.ChordNotes;
            for (int i = 0; i < Chord.Length; i++)
            {
                _chordBoxes[i].SetEntry(chordNotes[i]);
            }
        }

        private void AddMelodyNote(PianoNote key)
        {
            if (_cursorIndex >= MelodyBoxCount) return;

            var note = new DraftNote(key);
            MelodyCreateManager.Instance.AddMelodyNote(note);
            _melodyBoxes[_cursorIndex].SetEntry(note);

            _cursorIndex++;
        }

        // ── ボタンハンドラ ───────────────────────────────────────────────

        private void OnExtendClicked()
        {
            if (!MelodyCreateManager.Instance.IsChordComplete) return;
            if (_cursorIndex >= MelodyBoxCount) return;

            MelodyCreateManager.Instance.Extend();
            _melodyBoxes[_cursorIndex].ShowArrow();
            _cursorIndex++;
            RefreshButtons();
        }

        private void OnDeleteClicked()
        {
            if (!MelodyCreateManager.Instance.IsChordComplete)
            {
                DeleteLastChordNote();
                RefreshButtons();
                return;
            }

            if (_cursorIndex == 0)
            {
                DeleteLastChordNote();
            }
            else
            {
                MelodyCreateManager.Instance.ShrinkLastStep();
                _cursorIndex--;
                _melodyBoxes[_cursorIndex].SetEntry(null);
            }

            RefreshButtons();
        }

        private void OnClearClicked()
        {
            MelodyCreateManager.Instance.ClearAll();
            _templateSelector?.ResetSelection();

            foreach (var box in _chordBoxes)
            {
                box.SetEntry(null);
            }
            foreach (var box in _melodyBoxes)
            {
                box.SetEntry(null);
            }

            _enteredChordNotes.Clear();
            _cursorIndex = 0;

            RefreshButtons();
        }

        private void DeleteLastChordNote()
        {
            if (_enteredChordNotes.Count == 0) return;
            _enteredChordNotes.RemoveAt(_enteredChordNotes.Count - 1);
            SortAndApplyChordNotes();
        }

        // ── 状態管理 ─────────────────────────────────────────────────────

        private void RefreshButtons()
        {
            bool isChordComplete = MelodyCreateManager.Instance.IsChordComplete;
            bool hasAnyInput = _enteredChordNotes.Count > 0;
            _extendButton.interactable = isChordComplete && _cursorIndex < MelodyBoxCount;
            _deleteButton.interactable = hasAnyInput;
            _clearButton.interactable = hasAnyInput;
        }

        // ── テンプレート読み込み ─────────────────────────────────────────────

        private void RebuildFromTemplate(Melody template)
        {
            foreach (var box in _chordBoxes) box.SetEntry(null);
            foreach (var box in _melodyBoxes) box.SetEntry(null);
            _enteredChordNotes.Clear();
            _cursorIndex = 0;

            var manager = MelodyCreateManager.Instance;

            var chordNotes = manager.ChordNotes;
            for (int i = 0; i < Chord.Length; i++)
            {
                _chordBoxes[i].SetEntry(chordNotes[i]);
                if (chordNotes[i] != null) _enteredChordNotes.Add(chordNotes[i].Key);
            }

            for (int b = 1; b < manager.ChordBeats; b++)
            {
                if (_cursorIndex >= MelodyBoxCount) break;
                _melodyBoxes[_cursorIndex].ShowArrow();
                _cursorIndex++;
            }

            foreach (var draftNote in manager.MelodyNotes)
            {
                if (_cursorIndex >= MelodyBoxCount) break;
                _melodyBoxes[_cursorIndex].SetEntry(draftNote);
                _cursorIndex++;

                for (int b = 1; b < draftNote.Beats; b++)
                {
                    if (_cursorIndex >= MelodyBoxCount) break;
                    _melodyBoxes[_cursorIndex].ShowArrow();
                    _cursorIndex++;
                }
            }

            RefreshButtons();
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
