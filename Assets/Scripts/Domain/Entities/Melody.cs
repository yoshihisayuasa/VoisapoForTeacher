using Assets.Scripts.Domain.ValueObjects;
using AsseScripts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Domain.Entities
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
        public Chord Chord { get; }

        public IReadOnlyList<Note> Notes { get; }
        public readonly int Length;
        private readonly Interval _minInterval;
        private readonly Interval _maxInterval;


        public Melody(string name, Chord chord, List<Note> notes)
        {
            Name = name;
            Chord = chord;
            Notes = notes;
            (_minInterval, _maxInterval) = CalculateIntervalRange();
        }
        private (Interval min, Interval max) CalculateIntervalRange()
        {
            var chordValues = Chord.Intervals.Select(i => i.Value);
            var noteValues = Notes.Select(n => n.Interval.Value);
            var all = chordValues.Concat(noteValues);

            return (new Interval(all.Min()), new Interval(all.Max()));
        }

        /// <summary>
        /// 根音を与えたとき、このメロディが使う鍵盤範囲（音域）を返す。
        /// </summary>
        public PianoKeyRange KeyRangeAt(PianoNote rootKey)
        {
            return new PianoKeyRange(rootKey + _minInterval, rootKey + _maxInterval);
        }

        /// <summary>
        /// 根音を rootKey にしたとき、このメロディが keyCount 鍵の鍵盤内で演奏可能か。
        /// </summary>
        public bool IsPlayableAt(PianoNote rootKey, int keyCount)
        {
            return KeyRangeAt(rootKey).IsWithinKeyboard(keyCount);
        }
    }
}
