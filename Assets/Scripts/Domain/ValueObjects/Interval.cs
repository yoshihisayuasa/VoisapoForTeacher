
namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class Interval : ValueObject<Interval>
    {
        /// <summary>1オクターブ（12半音）。</summary>
        public static Interval Octave => new(12);

        public int Value { get; }
        public Interval(int value)
        {
            Value = value;
        }

        public static Interval operator +(Interval a, Interval b)
        {
            return new Interval(a.Value + b.Value);
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
