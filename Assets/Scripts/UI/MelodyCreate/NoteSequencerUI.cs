using AsseScripts.Domain;
using Assets.Scripts.Domain;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using R3;
using System.Collections;
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

        private static int MelodyBoxCount => MelodyDraft.MaxMelodySteps;

        private readonly List<NoteBoxUI> _chordBoxes = new();
        private readonly List<NoteBoxUI> _melodyBoxes = new();
        private readonly List<DomainPianoNote> _enteredChordNotes = new();

        // 次に入力されるメロディボックスのインデックス
        private int _cursorIndex = 0;

        private bool _isChordMode = true;

        private Coroutine _previewSoundCoroutine;
        private DomainPianoNote _previewingNote;
        private const float PreviewSoundDuration = 0.5F;

        private void Start()
        {
            InitChordBoxes();
            InitMelodyBoxes();

            _extendButton.onClick.AddListener(OnExtendClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);

            PianoController.Instance.OnAnyKeyClickAsObservable
                .Subscribe(OnPianoKeyClicked)
                .AddTo(this);

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
            for (int i = 0; i < MelodyBoxCount; i++)
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
                AddCordNote(key);
            }
            else
            {
                AddMelodyNote(key);
            }

            RefreshExtendButton();
            StartPreviewSound(key);
        }

        private void StartPreviewSound(DomainPianoNote key)
        {
            if (_previewSoundCoroutine != null)
            {
                StopCoroutine(_previewSoundCoroutine);
                if (_previewingNote != null)
                {
                    PianoController.Instance.Stop(_previewingNote, false);
                }
            }

            _previewingNote = key;
            PianoController.Instance.Play(key, true, 1f);
            _previewSoundCoroutine = StartCoroutine(StopPreviewSoundAfterDelay(key));
        }

        private IEnumerator StopPreviewSoundAfterDelay(DomainPianoNote key)
        {
            yield return new WaitForSeconds(PreviewSoundDuration);
            PianoController.Instance.Stop(key, false);
            _previewSoundCoroutine = null;
            _previewingNote = null;
        }

        private void AddCordNote(DomainPianoNote key)
        {
            _enteredChordNotes.Add(key);
            SortAndApplyChordNotes();

            if (_enteredChordNotes.Count >= Chord.Length)
            {
                _isChordMode = false;
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
                    _chordBoxes[i].SetEntry(new DraftNote(_enteredChordNotes[noteIdx]));
                }
                else
                {
                    _chordBoxes[i].SetEntry(null);
                }
            }

            MelodyCreateManager.Instance.AddChordNotes(_enteredChordNotes);
        }

        private void AddMelodyNote(DomainPianoNote key)
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
            if (_isChordMode) return;
            if (_cursorIndex >= MelodyBoxCount) return;

            var draft = MelodyCreateManager.Instance.Draft;
            if (draft.MelodyNotes.Count == 0)
            {
                MelodyCreateManager.Instance.ExtendChord();
            }
            else
            {
                MelodyCreateManager.Instance.ExtendLastMelodyNote();
            }

            _melodyBoxes[_cursorIndex].ShowArrow();
            _cursorIndex++;
            RefreshExtendButton();
        }

        private void OnDeleteClicked()
        {
            if (_isChordMode)
            {
                DeleteLastChordNote();
                RefreshExtendButton();
                return;
            }

            var draft = MelodyCreateManager.Instance.Draft;

            if (_cursorIndex == 0)
            {
                _isChordMode = true;
                DeleteLastChordNote();
            }
            else if (draft.MelodyNotes.Count == 0)
            {
                // 和音延長ボックスを1つ戻す
                MelodyCreateManager.Instance.ShrinkChord();
                _cursorIndex--;
                _melodyBoxes[_cursorIndex].SetEntry(null);
            }
            else
            {
                var lastNote = draft.MelodyNotes[^1];
                if (lastNote.Beats > 1)
                {
                    MelodyCreateManager.Instance.ShrinkLastMelodyNote();
                    _cursorIndex--;
                    _melodyBoxes[_cursorIndex].SetEntry(null);
                }
                else
                {
                    MelodyCreateManager.Instance.RemoveLastMelodyNote();
                    _cursorIndex--;
                    _melodyBoxes[_cursorIndex].SetEntry(null);
                }
            }

            RefreshExtendButton();
        }

        private void DeleteLastChordNote()
        {
            if (_enteredChordNotes.Count == 0) return;
            _enteredChordNotes.RemoveAt(_enteredChordNotes.Count - 1);
            SortAndApplyChordNotes();
        }

        // ── 状態管理 ─────────────────────────────────────────────────────

        private void RefreshExtendButton()
        {
            _extendButton.interactable = !_isChordMode && _cursorIndex < MelodyBoxCount;
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
