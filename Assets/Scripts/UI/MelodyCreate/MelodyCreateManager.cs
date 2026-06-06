using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.MelodyUI;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

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

        public MelodyDraft Draft { get; private set; }

        public Melody CurrentMelody { get; private set; }

        private readonly Subject<Unit> _draftChanged = new();
        public Observable<Unit> DraftChanged => _draftChanged;

        public bool IsChordComplete => Draft.IsChordComplete;
        public bool CanPreview => Draft.CanPreview;
        public DomainPianoNote DraftRoot => Draft.Root;

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
            for (int i = 0; i < Chord.Length; i++)
            {
                if (i < notes.Count)
                {
                    Draft.AddChordNote(i, notes[i]);
                }
                else
                {
                    Draft.ClearChordNote(i);
                }
            }
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
            if (Draft.MelodyNotes.Count == 0)
                Draft.ExtendChord();
            else
                Draft.ExtendLastMelodyNote();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ShrinkLastStep()
        {
            if (Draft.MelodyNotes.Count == 0)
            {
                Draft.ShrinkChord();
            }
            else if (Draft.MelodyNotes[^1].Beats > 1)
            {
                Draft.ShrinkLastMelodyNote();
            }
            else
            {
                Draft.RemoveLastMelodyNote();
            }
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
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
