using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.MelodyUI;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DomainPianoNote = AsseScripts.Domain.PianoNote;
using DomainPianoNoteEnum = AsseScripts.Domain.PianoNoteEnum;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ作成シーンのオーケストレーター。
    /// コード音・メロディステップの編集ロジックを一元管理する。
    /// </summary>
    public sealed class MelodyCreateManager : MonoBehaviour
    {
        public static MelodyCreateManager Instance { get; private set; }

        [SerializeField] private string _mainSceneName = "Main";
        [SerializeField] private DraftMelodyPlayer _draftMelodyPlayer;

        public MelodyDraft Draft { get; private set; }

        public Melody CurrentMelody { get; private set; }

        private readonly Subject<Unit> _draftChanged = new();
        public Observable<Unit> DraftChanged => _draftChanged;

        public bool IsChordComplete => Draft.IsChordComplete;
        public bool CanPreview => Draft.CanPreview;
        public IReadOnlyList<DraftNote> ChordNotes => Draft.ChordNotes;
        public IReadOnlyList<DraftNote> MelodyNotes => Draft.MelodyNotes;
        public int ChordBeats => Draft.ChordBeats;

        private void RebuildCurrentMelody()
        {
            if (Draft.CanPreview)
            {
                CurrentMelody = Draft.Build(string.Empty);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Draft = new MelodyDraft();


        }

        public void AddChordNotes(IReadOnlyList<DomainPianoNote> notes)
        {
            Draft.SetChordNotes(notes);
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void AddMelodyNote(DraftNote note)
        {
            Draft.AddMelodyNote(note);
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ClearAll()
        {
            Draft.ClearAll();
            CurrentMelody = null;
            _draftChanged.OnNext(Unit.Default);
        }

        public void Extend()
        {
            Draft.Extend();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ShrinkLastStep()
        {
            Draft.ShrinkLastStep();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void LoadTemplate(Melody template)
        {
            Draft.ClearAll();
            CurrentMelody = null;

            var root = new DomainPianoNote(DomainPianoNoteEnum.C4);

            var chordNotes = template.Chord.Intervals
                .Select(interval => root + interval);

            Draft.SetChordNotes(chordNotes);

            for (int b = 1; b < template.Chord.Beats; b++)
            {
                Draft.ExtendChord();
            }

            foreach (var note in template.Notes)
            {
                var draftNote = new DraftNote(root + note.Interval);
                Draft.AddMelodyNote(draftNote);
                for (int b = 1; b < note.Beats; b++)
                { 
                    Draft.ExtendLastMelodyNote();
                }
            }

            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void Preview()
        {
            if (CurrentMelody == null || Draft.Root == null)
            {
                return;
            }
            _draftMelodyPlayer.Play(CurrentMelody, Draft.Root);
        }

        public void SaveWithName(string name)
        {
            if (!Draft.CanPreview)
            {
                Debug.LogWarning("メロディが無効です: 和音3音・メロディ1音以上が必要です");
                return;
            }

            if (MelodyManager.Instance.ContainsMelodyWithName(name))
            {
                SimpleModalWindow.Create(ignorable: false)
                    .SetHeader("Error")
                    .SetBody($"A melody named \"{name}\" already exists.")
                    .AddButton("OK", () => { }, ModalButtonType.Success)
                    .Show();
                return;
            }

            int position = MelodyManager.Instance.MelodyCount;
            Melody melody = Draft.Build(name);
            MelodyManager.Instance.AddMelody(new SavedMelody(melody, position));

            LoadMainScene();
        }

        public void Home()
        {
            LoadMainScene();
        }

        private void LoadMainScene()
        {
            MelodyPlayer.Instance.StopMelody(true, shouldDelayRecordStop: false);
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
