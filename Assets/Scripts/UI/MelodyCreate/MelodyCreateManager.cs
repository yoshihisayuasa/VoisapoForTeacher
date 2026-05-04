using AsseScripts.Domain;
using Assets.Scripts.Domain;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Melody;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DomainMelody = AsseScripts.Domain.Melody;
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

        public DomainMelody CurrentMelody { get; private set; }

        private readonly Subject<Unit> _draftChanged = new();
        public Observable<Unit> DraftChanged => _draftChanged;

        public bool CanPreview => Draft.CanPreview;
        public DomainPianoNote DraftRoot => Draft.Root;

        private void RebuildCurrentMelody()
        {
            if (Draft.CanPreview)
            {
                CurrentMelody = Draft.Build(string.Empty, 0);
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

            MelodyManager.Instance.ClearCurrentMelody();
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

        public void ExtendLastMelodyNote()
        {
            Draft.ExtendLastMelodyNote();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ShrinkLastMelodyNote()
        {
            Draft.ShrinkLastMelodyNote();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void RemoveLastMelodyNote()
        {
            Draft.RemoveLastMelodyNote();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ExtendChord()
        {
            Draft.ExtendChord();
            RebuildCurrentMelody();
            _draftChanged.OnNext(Unit.Default);
        }

        public void ShrinkChord()
        {
            Draft.ShrinkChord();
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
            
            int position = MelodyManager.Instance.MelodyCount;
            var melody = Draft.Build(name, position);
            MelodyManager.Instance.AddMelody(melody);

            LoadMainScene();
        }

        public void Home()
        {
            LoadMainScene();
        }

        private void LoadMainScene()
        {
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
