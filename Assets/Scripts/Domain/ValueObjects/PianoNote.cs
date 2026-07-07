namespace Assets.Scripts.Domain.ValueObjects
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class PianoNote : ValueObject<PianoNote>
    {
        public PianoNoteEnum Note { get; }

        public int Index => (int)Note;
        public bool IsSharp => Note.ToString().Contains("Sharp");

        public PianoNote(PianoNoteEnum note)
        {
            Note = note;
        }

        /// <summary>
        /// delta だけ移動した鍵盤を返す。keyCount 鍵の鍵盤からはみ出す場合は端に収める。
        /// </summary>
        public PianoNote MovedBy(int delta, int keyCount)
        {
            int idx = Math.Clamp(Index + delta, 0, keyCount - 1);
            return new PianoNote((PianoNoteEnum)idx);
        }

        protected override bool EqualsCore(PianoNote other)
        {
            return Index == other.Index;
        }

        public override int CompareTo(PianoNote other)
        {
            if (other is null) return 1;
            return Index.CompareTo(other.Index);
        }

        protected override int GetHashCodeCore()
        {
            return Index.GetHashCode();
        }

        public static PianoNote operator +(PianoNote baseNote, PianoNote intervalNote)
        {

            int idx = baseNote.Index + intervalNote.Index;

            return new PianoNote((PianoNoteEnum)idx);
        }

        public static PianoNote operator -(PianoNote baseNote, PianoNote intervalNote)
        {

            int idx = baseNote.Index - intervalNote.Index;

            return new PianoNote((PianoNoteEnum)idx);
        }

        public static PianoNote operator +(PianoNote baseNote, Interval interval)
        {
            int idx = baseNote.Index + interval.Value;

            return new PianoNote((PianoNoteEnum)idx);
        }
        public static PianoNote operator -(PianoNote baseNote, Interval interval)
        {
            int idx = baseNote.Index - interval.Value;

            return new PianoNote((PianoNoteEnum)idx);
        }



    }
}