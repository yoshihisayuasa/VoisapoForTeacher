
namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class Interval : ValueObject<Interval>
    {
        public int Value { get; }
        public Interval(int value)
        {
            Value = value;
        }

        public static Interval operator +(Interval a, Interval b)
        {
            return new Interval(a.Value + b.Value);
        }
        public static Interval operator -(Interval a, Interval b)
        {
            return new Interval(a.Value - b.Value);
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
