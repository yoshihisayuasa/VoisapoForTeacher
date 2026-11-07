using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class BPM : ValueObject<BPM>
    {
        private const int Min = 10;

        /// <summary>
        /// 1回の増減で変わる量。
        /// </summary>
        private const int Step = 10;

        /// <summary>
        /// 鍵盤音のフェードアウトは1拍の0.6倍の長さで行う。
        /// </summary>
        private const float KeyFadeOutBeatRatio = 0.6f;

        public int Value { get; }

        public float SecondPerBeat => 60f / Value;

        public float KeyFadeOutSeconds => SecondPerBeat * KeyFadeOutBeatRatio;

        public BPM(int value)
        {
            if (value < Min)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"BPMは{Min}以上でなければなりません。");
            }
            Value = value;
        }

        public BPM Increment() => new(Value + Step);

        public BPM Decrement() => new(Math.Max(Min, Value - Step));

        public override string ToString() => Value.ToString();

        protected override bool EqualsCore(BPM other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}
