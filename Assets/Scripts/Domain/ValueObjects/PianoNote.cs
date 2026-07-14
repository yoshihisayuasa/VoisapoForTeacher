namespace Assets.Scripts.Domain.ValueObjects
{
    using System;

    public sealed class PianoNote : ValueObject<PianoNote>
    {
        /// <summary>ルート音の既定値（C4）。</summary>
        public static PianoNote DefaultRoot => new(PianoNoteEnum.C4);

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

        /// <summary>
        /// interval だけ離れた音を返す。鍵盤外の音もあえて表現できる。
        /// 音域チェック（PianoKeyRange.IsWithinKeyboard）は根音＋インターバルの結果が
        /// 鍵盤に収まるかで演奏可否を判定するため、ここで端に丸めたり例外にしたりすると
        /// チェック自体が成立しなくなる。
        /// </summary>
        public static PianoNote operator +(PianoNote baseNote, Interval interval)
        {
            return new PianoNote((PianoNoteEnum)(baseNote.Index + interval.Value));
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
    }
}
