using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Domain.Entities;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// メロディ作成中の可変下書き。
    /// コード音（入力順）・コード拍数・メロディ音列を管理する。
    /// 保存時、メロディ1音目をルートとしてインターバルに変換する。
    /// </summary>
    public sealed class MelodyDraft
    {
        // コード音は「取り消しは入力の逆順」のため入力順で保持し、
        // 表示用スロット（高い音から順・不足分は null）は別途組み立てる。
        private readonly List<PianoNote> _chordEntries = new();
        private readonly DraftNote[] _chordNotes = new DraftNote[Chord.Length];
        private int _chordBeats = 1;
        private readonly List<DraftNote> _melodyNotes = new();

        // List/配列をそのまま返すと IReadOnlyList からダウンキャストして書き換えられるため、
        // 読み取り専用ビューで包んで公開する（中身は内部コレクションに追従する）。
        public IReadOnlyList<DraftNote> MelodyNotes { get; }
        public IReadOnlyList<DraftNote> ChordNotes { get; }
        public int ChordBeats => _chordBeats;

        public MelodyDraft()
        {
            MelodyNotes = _melodyNotes.AsReadOnly();
            ChordNotes = Array.AsReadOnly(_chordNotes);
        }

        public const int MaxMelodySteps = 26;

        /// <summary>
        /// 使用済みステップ数。コード延長分の矢印と、メロディ音符（延長の矢印含む）を合算する。
        /// </summary>
        public int UsedSteps => (_chordBeats - 1) + _melodyNotes.Sum(n => n.Beats);

        public bool CanAddMelodyNote => UsedSteps < MaxMelodySteps;

        private PianoNote _root;
        public PianoNote Root => _root;

        // ── 入力操作 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 鍵盤入力を1音追加する。コードが未完成ならコード音、完成済みならメロディ音として扱う。
        /// </summary>
        public void AddNote(PianoNote key)
        {
            if (!IsChordComplete)
            {
                AddChordEntry(key);
            }
            else
            {
                AddMelodyNote(new DraftNote(key));
            }
        }

        /// <summary>
        /// 直近の入力を1つ取り消す。メロディ → コード拍 → コード音の順に遡る。
        /// </summary>
        public void DeleteLast()
        {
            if (_melodyNotes.Count > 0)
            {
                ShrinkLastStep();
            }
            else if (_chordBeats > 1)
            {
                ShrinkChord();
            }
            else
            {
                RemoveLastChordEntry();
            }
        }

        // ── コード操作 ──────────────────────────────────────────────────────

        public bool HasAnyInput => _chordEntries.Count > 0;

        private void SetChordNotes(IEnumerable<PianoNote> notes)
        {
            _chordEntries.Clear();
            _chordEntries.AddRange(notes.OrderByDescending(n => n.Index));
            RebuildChordSlots();
        }

        private void ExtendChord()
        {
            _chordBeats++;
        }

        private void ShrinkChord()
        {
            _chordBeats = Math.Max(1, _chordBeats - 1);
        }

        private void AddChordEntry(PianoNote key)
        {
            _chordEntries.Add(key);
            RebuildChordSlots();
        }

        private void RemoveLastChordEntry()
        {
            if (_chordEntries.Count == 0)
            {
                return;
            }
            _chordEntries.RemoveAt(_chordEntries.Count - 1);
            RebuildChordSlots();
        }

        private void RebuildChordSlots()
        {
            var sorted = _chordEntries.OrderByDescending(n => n.Index).ToList();
            for (int i = 0; i < Chord.Length; i++)
            {
                _chordNotes[i] = i < sorted.Count ? new DraftNote(sorted[i]) : null;
            }
        }

        // ── メロディ操作 ─────────────────────────────────────────────────────

        private void AddMelodyNote(DraftNote note)
        {
            if (!CanAddMelodyNote) return;
            _melodyNotes.Add(note);
            if (_root == null)
            {
                _root = note.Key;
            }
        }

        private void ExtendLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            var last = _melodyNotes[^1];
            _melodyNotes[^1] = last.WithBeats(last.Beats + 1);
        }

        private void ShrinkLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            var last = _melodyNotes[^1];
            if (last.Beats <= 1) return;
            _melodyNotes[^1] = last.WithBeats(last.Beats - 1);
        }

        private void RemoveLastMelodyNote()
        {
            if (_melodyNotes.Count == 0) return;
            _melodyNotes.RemoveAt(_melodyNotes.Count - 1);
            _root = _melodyNotes.Count > 0 ? _melodyNotes[0].Key : null;
        }

        public void ClearAll()
        {
            _chordEntries.Clear();
            for (int i = 0; i < Chord.Length; i++)
            {
                _chordNotes[i] = null;
            }
            _chordBeats = 1;
            _melodyNotes.Clear();
            _root = null;
        }

        /// <summary>
        /// テンプレートの内容で下書き全体を置き換える。root をテンプレートのルート音として展開する。
        /// </summary>
        public void LoadFrom(Melody template, PianoNote root)
        {
            ClearAll();

            SetChordNotes(template.Chord.Intervals.Select(interval => root + interval));
            for (int b = 1; b < template.Chord.Beats; b++)
            {
                ExtendChord();
            }

            foreach (var note in template.Notes)
            {
                AddMelodyNote(new DraftNote(root + note.Interval));
                for (int b = 1; b < note.Beats; b++)
                {
                    ExtendLastMelodyNote();
                }
            }
        }

        // ── 複合操作 ─────────────────────────────────────────────────────────

        public bool CanExtend => IsChordComplete && UsedSteps < MaxMelodySteps;

        public void Extend()
        {
            if (!CanExtend)
            {
                return;
            }
            if (_melodyNotes.Count == 0) ExtendChord();
            else ExtendLastMelodyNote();
        }

        public void ShrinkLastStep()
        {
            if (_melodyNotes.Count == 0) ShrinkChord();
            else if (_melodyNotes[^1].Beats > 1) ShrinkLastMelodyNote();
            else RemoveLastMelodyNote();
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
