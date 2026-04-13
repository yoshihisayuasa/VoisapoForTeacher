using System;
using System.Collections.Generic;
using Assets.Scripts.Domain.ValueObjects;

namespace AsseScripts.Domain
{
    /// <summary>
    /// メロディ作成中の可変下書き。
    /// Steps[0-2] がコード用 NoteStep、Steps[3+] がメロディ用ステップ列。
    /// 各 NoteStep は押した鍵の絶対音を保持する。
    /// 保存時、メロディ1音目をルートとしてインターバルに変換する。
    /// </summary>
    public sealed class MelodyDraft
    {
        public string Name { get; set; } = string.Empty;

        private readonly List<IStepEntry> _steps = new();
        public IReadOnlyList<IStepEntry> Steps => _steps;

        public const int MaxMelodySteps = 26;

        public int MelodyStepCount => _steps.Count - Chord.Length;

        public bool CanAddMelodyStep => MelodyStepCount < MaxMelodySteps;

        public MelodyDraft()
        {
            for (int i = 0; i < Chord.Length; i++)
            {
                _steps.Add(null);
            }
        }

        public void SetChordNote(int index, PianoNote key)
        {
            if (index < 0 || index >= Chord.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _steps[index] = new NoteStep(key);
        }

        public void ClearChordNote(int index)
        {
            if (index < 0 || index >= Chord.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _steps[index] = null;
        }

        public void AddMelodyStep(IStepEntry entry)
        {
            if (!CanAddMelodyStep) return;
            _steps.Add(entry);
        }

        public void RemoveLastMelodyStep()
        {
            if (_steps.Count > Chord.Length)
            {
                _steps.RemoveAt(_steps.Count - 1);
            }
        }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            AllChordNotesSet() &&
            HasAtLeastOneMelodyNote();

        /// <summary>
        /// 名前が未設定でもプレビュー再生できる状態か。
        /// </summary>
        public bool CanPreview => AllChordNotesSet() && HasAtLeastOneMelodyNote();

        private bool AllChordNotesSet()
        {
            for (int i = 0; i < Chord.Length; i++)
            {
                if (_steps[i] == null) return false;
            }
            return true;
        }

        private bool HasAtLeastOneMelodyNote()
        {
            for (int i = Chord.Length; i < _steps.Count; i++)
            {
                if (_steps[i] is NoteStep) return true;
            }
            return false;
        }

        /// <summary>
        /// メロディ1音目をルートとして全ステップをインターバルに変換し Melody を生成する。
        /// </summary>
        public Melody Build(int position)
        {
            var root = FindMelodyRoot();
            if (root == null)
            {
                throw new InvalidOperationException("メロディに音符がありません");
            }

            var chordIntervals = new List<Interval>();
            for (int i = 0; i < Chord.Length; i++)
            {
                if (_steps[i] is NoteStep step)
                {
                    chordIntervals.Add(new Interval(step.Key.Index - root.Index));
                }
            }

            // メロディ先頭の ExtendStep を和音拍数として解釈する
            int chordBeats = 0;
            int melodyStart = Chord.Length;
            while (melodyStart < _steps.Count && _steps[melodyStart] is ExtendStep)
            {
                chordBeats++;
                melodyStart++;
            }
            chordBeats = chordBeats + 1;

            var chord = new Chord(chordIntervals, chordBeats);

            var notes = new List<Note>();
            for (int i = melodyStart; i < _steps.Count; i++)
            {
                if (_steps[i] is NoteStep noteStep)
                {
                    notes.Add(new Note(noteStep.Key.Index - root.Index, 1));
                }
                else if (_steps[i] is ExtendStep && notes.Count > 0)
                {
                    var last = notes[^1];
                    notes[^1] = new Note(last.Interval.Value, last.Beats + 1);
                }
            }

            return new Melody(Name, chord, notes, position);
        }

        private PianoNote FindMelodyRoot()
        {
            for (int i = Chord.Length; i < _steps.Count; i++)
            {
                if (_steps[i] is NoteStep ns) return ns.Key;
            }
            return null;
        }
    }
}
