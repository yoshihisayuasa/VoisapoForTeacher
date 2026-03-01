using Assets.Scripts.Domain.ValueObjects;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scripts.Domain
{
    /// <summary>
    /// 単音（相対インターバルと拍数）
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
        public readonly int  CordLength = 3; 
        public string Name { get; }
        public int Position { get; }
        public IReadOnlyList<Note> Notes { get; }
        public readonly int Length;
        public readonly Interval MinInterval;
        public readonly Interval MaxInterval;


        public Melody(string name, List<Note> notes, int position)
        {
            Name = name;
            Notes = notes;
            Position = position;
            Length = CalculateNoteLength(notes);
            TryGetIntervalRange(out MinInterval, out MaxInterval);
        }

        // 拍数の総数を算出（Beats==0で終端）
        private int CalculateNoteLength(List<Note> notes)
        {
            if (notes == null || notes.Count == 0) return 0;

            int total = 0;
            foreach (var note in notes)
            {
                if (note.Beats == 0) break;
                total++;
            }
            return total;
        }

        /// <summary>
        /// 有効ノート（Beats==0で終端）の相対インターバル最小/最大を算出。
        /// </summary>
        private void TryGetIntervalRange(out Interval minInterval, out Interval maxInterval)
        {
            int minIntervalInt = int.MaxValue;
            int maxIntervalInt = int.MinValue;

            foreach (var note in Notes)
            {
                if (note.Beats == 0) break;
                if (note.Interval.Value < minIntervalInt) minIntervalInt = note.Interval.Value;
                if (note.Interval.Value > maxIntervalInt) maxIntervalInt = note.Interval.Value;
            }

            minInterval = new Interval(minIntervalInt);
            maxInterval = new Interval(maxIntervalInt);
        }
    }
}
