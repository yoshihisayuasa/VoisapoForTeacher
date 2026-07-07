using Assets.Scripts.Domain.ValueObjects;
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

    /// <summary>
    /// 根音に和音を適用した、実際に鳴らす鍵盤と拍数のひとまとまり。
    /// </summary>
    public readonly struct ChordVoicing
    {
        public IReadOnlyList<PianoNote> Keys { get; }
        public int Beats { get; }

        public ChordVoicing(IReadOnlyList<PianoNote> keys, int beats)
        {
            Keys = keys;
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
        public MelodyKind Kind { get; }
        public Chord Chord { get; }

        public IReadOnlyList<Note> Notes { get; }
        public readonly int Length;
        private readonly Interval _minInterval;
        private readonly Interval _maxInterval;


        public Melody(string name, Chord chord, List<Note> notes)
        {
            Name = name;
            Kind = ResolveKind(name);
            Chord = chord;
            Notes = notes;
            (_minInterval, _maxInterval) = CalculateIntervalRange();
        }

        /// <summary>
        /// 表示名から再生種別を解決する。名前と種別の対応はここ1箇所だけが知る。
        /// </summary>
        private static MelodyKind ResolveKind(string name) => name switch
        {
            "Single"           => MelodyKind.Single,
            "Major& Metronome" => MelodyKind.MajorWithMetronome,
            "Major Code"       => MelodyKind.MajorChord,
            _                  => MelodyKind.Standard,
        };
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
        /// <summary>
        /// 根音を与えたとき、和音が使う鍵盤と拍数を返す。
        /// 鍵盤範囲内であることは再生前の IsPlayableAt（音域は和音も含む）が保証するため、ここでは検証しない。
        /// </summary>
        public ChordVoicing ChordAt(PianoNote rootKey)
        {
            var keys = new List<PianoNote>(Chord.Intervals.Count);
            foreach (var interval in Chord.Intervals)
            {
                keys.Add(rootKey + interval);
            }
            return new ChordVoicing(keys, Chord.Beats);
        }
    }
}
