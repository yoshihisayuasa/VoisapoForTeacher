using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 2つの音の距離を半音単位で表す。正は上、負は下。
    /// メロディは根音からのインターバルで音を持つため、根音を変えるだけで移調できる。
    /// </summary>
    public sealed class Interval : ValueObject<Interval>, IComparable<Interval>
    {
        /// <summary>1オクターブ（12半音）。</summary>
        public static Interval Octave { get; } = new(12);

        public int Value { get; }
        public Interval(int value)
        {
            Value = value;
        }

        public static Interval operator +(Interval a, Interval b)
        {
            return new Interval(a.Value + b.Value);
        }

        /// <summary>音の高さの順に並べる（低いほうが小さい）。</summary>
        public int CompareTo(Interval other)
        {
            return Value.CompareTo(other.Value);
        }

        protected override bool EqualsCore(Interval other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}
