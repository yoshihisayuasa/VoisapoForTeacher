namespace Assets.Scripts.Domain.ValueObjects
{
    using System;

    public sealed class RoomId : ValueObject<RoomId>
    {
        public const int Digits = 4;

        private const int Min = 1000;
        private const int Max = 9999;

        private static readonly Random _random = new();

        public string Value { get; }

        private RoomId(string value) => Value = value;

        public static RoomId Generate() => new(_random.Next(Min, Max + 1).ToString());

        protected override bool EqualsCore(RoomId other) => Value == other.Value;

        protected override int GetHashCodeCore() => Value.GetHashCode();
    }
}
