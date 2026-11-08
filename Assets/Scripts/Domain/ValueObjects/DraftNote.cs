using System;
using Assets.Scripts.Domain.Entities;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// MelodyDraft が保持する音符。押した鍵の絶対音と拍数を持つ不変オブジェクト。
    /// 保存時は <see cref="ToNote"/> でルート音からのインターバルに変換する。
    /// </summary>
    public sealed class DraftNote : ValueObject<DraftNote>
    {
        private const int MinBeats = 1;

        public PianoNote Key { get; }
        public int Beats { get; }

        public string DisplayText => Key.DisplayText;

        public DraftNote(PianoNote key, int beats = MinBeats)
        {
            if (beats < MinBeats)
            {
                throw new ArgumentOutOfRangeException(nameof(beats), $"拍数は{MinBeats}以上でなければなりません。");
            }
            Key = key;
            Beats = beats;
        }

        /// <summary>
        /// 1拍伸ばした音符を返す。
        /// </summary>
        public DraftNote Extend() => new(Key, Beats + 1);

        /// <summary>
        /// 1拍縮めた音符を返す。すでに最短（1拍）なら false を返し、shrunk は null になる。
        /// </summary>
        public bool TryShrink(out DraftNote shrunk)
        {
            if (Beats <= MinBeats)
            {
                shrunk = null;
                return false;
            }

            shrunk = new DraftNote(Key, Beats - 1);
            return true;
        }

        /// <summary>
        /// root からのインターバルで表した音符に変換する。
        /// </summary>
        public Note ToNote(PianoNote root) => new(Key.Index - root.Index, Beats);

        protected override bool EqualsCore(DraftNote other)
        {
            return Key == other.Key && Beats == other.Beats;
        }

        protected override int GetHashCodeCore()
        {
            return (Key.GetHashCode() * 397) ^ Beats;
        }
    }
}
