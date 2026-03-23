using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Domain
{
    /// <summary>
    /// 単音（相対インターバルと拍数）
    /// </summary>
    /// 
    public readonly struct Chord
    {
        public const int Length = 3;
        public IReadOnlyList<Interval> Intervals { get; }
        public int Beats { get; }

        public Chord(IReadOnlyList<Interval> intervals, int beats)
        {
            if (intervals.Count != Length)
            {
                throw new ArgumentException($"和音は{Length}音で構成する必要があります");
            }
            Intervals = intervals;
            Beats = beats;
        }
    }

    public readonly struct Note
    {
        public Interval Interval { get; }
        public int Beats { get; }

        public Note(int interval, int beats)
        {
            Interval = new Interval(interval);
            Beats = beats;
        }
    }

    /// <summary>
    /// メロディ（値オブジェクト集合）
    /// </summary>
    public class Melody
    {
        public string Name { get; }
        public int Position { get; private set; }
        public Chord Chord { get; }

        public IReadOnlyList<Note> Notes { get; }
        public readonly int Length;
        public readonly Interval MinInterval;
        public readonly Interval MaxInterval;


        public Melody(string name, Chord chord, List<Note> notes, int position)
        {
            Name = name;
            Chord = chord;
            Notes = notes;
            Position = position;
            (MinInterval, MaxInterval) = CalculateIntervalRange();
        }

        public void SetPosition(int position)
        {
            Position = position;
        }
        private (Interval min, Interval max) CalculateIntervalRange()
        {
            var chordValues = Chord.Intervals.Select(i => i.Value);
            var noteValues = Notes.Select(n => n.Interval.Value);
            var all = chordValues.Concat(noteValues);

            return (new Interval(all.Min()), new Interval(all.Max()));
        }
    }
}
