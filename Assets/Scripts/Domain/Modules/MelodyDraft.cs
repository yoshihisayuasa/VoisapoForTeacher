using System;
using System.Collections.Generic;
using System.Linq;
using AsseScripts.Domain;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// メロディ作成中の可変下書き。
    /// コード音・コード拍数・メロディ音列を個別に管理する。
    /// 保存時、メロディ1音目をルートとしてインターバルに変換する。
    /// </summary>
    public sealed class MelodyDraft
    {
        private readonly DraftNote[] _chordNotes = new DraftNote[Chord.Length];
        private int _chordBeats = 1;
        private readonly List<DraftNote> _melodyNotes = new();

        public IReadOnlyList<DraftNote> MelodyNotes => _melodyNotes;

        public const int MaxMelodySteps = 26;
        public int MelodyStepCount => _melodyNotes.Count;
        public bool CanAddMelodyNote => MelodyStepCount < MaxMelodySteps;

        private PianoNote _root;
        public PianoNote Root => _root;

        // ── コード操作 ──────────────────────────────────────────────────────

        public void AddChordNote(int index, PianoNote key)
        {
            if (index < 0 || index >= Chord.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _chordNotes[index] = new DraftNote(key);
        }

        public void ClearChordNote(int index)
        {
            if (index < 0 || index >= Chord.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _chordNotes[index] = null;
        }

        public void ExtendChord()
        {
            _chordBeats++;
        }

        public void ShrinkChord()
        {
            _chordBeats = Math.Max(1, _chordBeats - 1);
        }

        // ── メロディ操作 ─────────────────────────────────────────────────────

        public void AddMelodyNote(DraftNote note)
        {
            if (!CanAddMelodyNote) return;
            _melodyNotes.Add(note);
            if (_root == null)
            {
                _root = note.Key;
            }
        }

        public void ExtendLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            var last = _melodyNotes[^1];
            _melodyNotes[^1] = last.WithBeats(last.Beats + 1);
        }

        public void ShrinkLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            var last = _melodyNotes[^1];
            if (last.Beats <= 1) return;
            _melodyNotes[^1] = last.WithBeats(last.Beats - 1);
        }

        public void RemoveLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            _melodyNotes.RemoveAt(_melodyNotes.Count - 1);
            _root = _melodyNotes.Count > 0 ? _melodyNotes[0].Key : null;
        }

        public void ClearAll()
        {
            for (int i = 0; i < Chord.Length; i++)
            {
                _chordNotes[i] = null;
            }
            _chordBeats = 1;
            _melodyNotes.Clear();
            _root = null;
        }

        // ── 検証 ────────────────────────────────────────────────────────────

        public bool IsChordComplete => AllChordNotesSet();

        public bool CanPreview => IsChordComplete && _melodyNotes.Count > 0;

        private bool AllChordNotesSet()
        {
            for (int i = 0; i < Chord.Length; i++)
            {
                if (_chordNotes[i] == null) return false;
            }
            return true;
        }

        // ── ビルド ───────────────────────────────────────────────────────────

        /// <summary>
        /// メロディ1音目をルートとして全ステップをインターバルに変換し Melody を生成する。
        /// </summary>
        public Melody Build(string name)
        {
            if (_root == null)
            {
                throw new InvalidOperationException("メロディに音符がありません");
            }

            var chordIntervals = _chordNotes
                .Where(n => n != null)
                .Select(n => new Interval(n.Key.Index - _root.Index))
                .ToList();

            var chord = new Chord(chordIntervals, _chordBeats);

            var notes = _melodyNotes
                .Select(n => new Note(n.Key.Index - _root.Index, n.Beats))
                .ToList();

            return new Melody(name, chord, notes);
        }
    }
}
