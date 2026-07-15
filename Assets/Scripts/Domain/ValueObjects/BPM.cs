using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class BPM : ValueObject<BPM>
    {
        public int Value { get; }

        public BPM(int value)
        {
            if (value < 10)
                throw new ArgumentOutOfRangeException(nameof(value), "BPMは10以上でなければなりません。");
            Value = value;
        }

        public BPM Increment(int step = 10) => new(Value + step);
        public BPM Decrement(int step = 10) => new(Math.Max(10, Value - step));

        protected override bool EqualsCore(BPM other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }

        public float SecondPerBeat => 60f / Value;

        /// <summary>
        /// 鍵盤音のフェードアウトは1拍の0.6倍の長さで行う。
        /// </summary>
        private const float KeyFadeOutBeatRatio = 0.6f;

        public float KeyFadeOutSeconds => SecondPerBeat * KeyFadeOutBeatRatio;
    }
}