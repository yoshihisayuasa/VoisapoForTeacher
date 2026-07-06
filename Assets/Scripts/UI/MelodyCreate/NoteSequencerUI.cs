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
    /// 編集状態は MelodyDraft が持ち、DraftChanged のたびに全ボックスを描き直す。
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

        private Coroutine _previewSoundCoroutine;
        private PianoNote _previewingNote;
        private const float PreviewSoundDuration = 0.5F;

        private void Start()
        {
            InitChordBoxes();
            InitMelodyBoxes();

            _extendButton.onClick.AddListener(() => MelodyCreateManager.Instance.Extend());
            _deleteButton.onClick.AddListener(() => MelodyCreateManager.Instance.DeleteLast());
            _clearButton.onClick.AddListener(OnClearClicked);

            PianoController.Instance.OnRootKeyPressedAsObservable
                .Subscribe(OnPianoKeyClicked)
                .AddTo(this);

            MelodyCreateManager.Instance.DraftChanged
                .Subscribe(_ => Render())
                .AddTo(this);

            Render();
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

        // ── 入力ハンドラ ─────────────────────────────────────────────────

        private void OnPianoKeyClicked(PianoNote key)
        {
            MelodyCreateManager.Instance.AddNote(key);
            StartPreviewSound(key);
        }

        private void OnClearClicked()
        {
            MelodyCreateManager.Instance.ClearAll();
            _templateSelector?.ResetSelection();
        }

        // ── 描画 ─────────────────────────────────────────────────────────

        private void Render()
        {
            var manager = MelodyCreateManager.Instance;
            RenderChordBoxes(manager.ChordNotes);
            RenderMelodyBoxes(manager.ChordBeats, manager.MelodyNotes);
            RenderButtons(manager);
        }

        private void RenderChordBoxes(IReadOnlyList<DraftNote> chordNotes)
        {
            for (int i = 0; i < _chordBoxes.Count; i++)
            {
                _chordBoxes[i].SetEntry(chordNotes[i]);
            }
        }

        // メロディ欄は「コード延長分の矢印 → 各音符（延長分は矢印）→ 残りは空」の順で埋める。
        private void RenderMelodyBoxes(int chordBeats, IReadOnlyList<DraftNote> melodyNotes)
        {
            int index = 0;

            for (int b = 1; b < chordBeats && index < MelodyBoxCount; b++)
            {
                _melodyBoxes[index++].ShowArrow();
            }

            foreach (var note in melodyNotes)
            {
                if (index >= MelodyBoxCount) break;
                _melodyBoxes[index++].SetEntry(note);

                for (int b = 1; b < note.Beats && index < MelodyBoxCount; b++)
                {
                    _melodyBoxes[index++].ShowArrow();
                }
            }

            while (index < MelodyBoxCount)
            {
                _melodyBoxes[index++].SetEntry(null);
            }
        }

        private void RenderButtons(MelodyCreateManager manager)
        {
            _extendButton.interactable = manager.CanExtend;
            _deleteButton.interactable = manager.HasAnyInput;
            _clearButton.interactable = manager.HasAnyInput;
        }

        // ── プレビュー音 ─────────────────────────────────────────────────

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
