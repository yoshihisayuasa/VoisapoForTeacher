
namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class Interval  
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

    }
}
