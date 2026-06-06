using UnityEngine;

namespace AsseScripts.Domain
{
    public sealed class RoomId : ValueObject<RoomId>
    {
        private const int Min = 1000;
        private const int Max = 9999;

        public string Value { get; }

        private RoomId(string value) => Value = value;

        public static RoomId Generate() => new(Random.Range(Min, Max + 1).ToString());

        public override string ToString() => Value;

        public override int CompareTo(RoomId other)
        {
            if (other is null)
            {
                return 1;
            }
            return string.Compare(Value, other.Value, System.StringComparison.Ordinal);
        }

        protected override bool EqualsCore(RoomId other) => other != null && Value == other.Value;

        protected override int GetHashCodeCore() => Value.GetHashCode();
    }
}
