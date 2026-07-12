using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Domain.Entities
{
    /// <summary>
    /// 和音（根音からの相対インターバルの組と拍数）
    /// </summary>
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

    /// <summary>
    /// 根音に単音を適用した、実際に鳴らす鍵盤と拍数のひとまとまり。
    /// </summary>
    public readonly struct NoteVoicing
    {
        public PianoNote Key { get; }
        public int Beats { get; }

        public NoteVoicing(PianoNote key, int beats)
        {
            Key = key;
            Beats = beats;
        }
    }

    /// <summary>
    /// 単音（根音からの相対インターバルと拍数）
    /// </summary>
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

        /// <summary>削除不可（保護されている）メロディか。組み込みメロディの保護に使う。</summary>
        public bool IsProtected { get; }

        /// <summary>ユーザーが自作したメロディか。テンプレート由来なら false。将来の課金判定に使う。</summary>
        public bool IsUserCreated { get; }

        public IReadOnlyList<Note> Notes { get; }
        private readonly Interval _minInterval;
        private readonly Interval _maxInterval;


        public Melody(string name, MelodyKind kind, Chord chord, List<Note> notes, bool isProtected, bool isUserCreated)
        {
            Name = name;
            Kind = kind;
            Chord = chord;
            Notes = notes;
            IsProtected = isProtected;
            IsUserCreated = isUserCreated;
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

        /// <summary>
        /// 根音を与えたとき、メロディ各音が使う鍵盤と拍数を演奏順に返す。
        /// 鍵盤範囲内であることは再生前の IsPlayableAt が保証するため、ここでは検証しない。
        /// </summary>
        public IReadOnlyList<NoteVoicing> NotesAt(PianoNote rootKey)
        {
            var notes = new List<NoteVoicing>(Notes.Count);
            foreach (var note in Notes)
            {
                notes.Add(new NoteVoicing(rootKey + note.Interval, note.Beats));
            }
            return notes;
        }
    }
}
