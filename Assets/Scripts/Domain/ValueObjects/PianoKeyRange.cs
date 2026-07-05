using System;

namespace AsseScripts.Domain
{
    /// <summary>
    /// 鍵盤の範囲（Min ≦ Max）。
    /// 順序の正規化はコンストラクタが保証するため、逆転した範囲は生成できない。
    /// </summary>
    public sealed class PianoKeyRange : ValueObject<PianoKeyRange>
    {
        public PianoNote Min { get; }
        public PianoNote Max { get; }

        public PianoKeyRange(PianoNote key1, PianoNote key2)
        {
            if (key1 is null) throw new ArgumentNullException(nameof(key1));
            if (key2 is null) throw new ArgumentNullException(nameof(key2));

            bool ordered = key1.Index <= key2.Index;
            Min = ordered ? key1 : key2;
            Max = ordered ? key2 : key1;
        }

        public bool IsMin(PianoNote key) => key == Min;
        public bool IsMax(PianoNote key) => key == Max;

        /// <summary>
        /// この範囲が keyCount 鍵の鍵盤に収まっているか。
        /// </summary>
        public bool IsWithinKeyboard(int keyCount)
        {
            return 0 <= Min.Index && Max.Index < keyCount;
        }

        public override int CompareTo(PianoKeyRange other)
        {
            if (other is null) return 1;
            int minComparison = Min.CompareTo(other.Min);
            return minComparison != 0 ? minComparison : Max.CompareTo(other.Max);
        }

        protected override bool EqualsCore(PianoKeyRange other)
        {
            return other != null && Min == other.Min && Max == other.Max;
        }

        protected override int GetHashCodeCore()
        {
            return (Min.Index * 397) ^ Max.Index;
        }
    }
}
