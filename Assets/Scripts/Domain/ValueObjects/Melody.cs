using Assets.Scripts.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;

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
        public int Position { get; private set; }
        public IReadOnlyList<Note> Notes { get; }
        public readonly int Length;
        public readonly Interval MinInterval;
        public readonly Interval MaxInterval;


        public Melody(string name, List<Note> notes, int position)
        {
            Name = name;
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
            int min = Notes.Min(n => n.Interval.Value);
            int max = Notes.Max(n => n.Interval.Value);
         
            return (new Interval(min), new Interval(max));
        }
    }
}
