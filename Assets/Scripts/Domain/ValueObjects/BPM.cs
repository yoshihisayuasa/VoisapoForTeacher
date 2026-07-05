using System;

namespace AsseScripts.Domain
{
    public sealed class BPM : ValueObject<BPM>
    {
        public int Value { get; } = 120;

        public BPM(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "BPMは1以上でなければなりません。");
            Value = value;
        }

        public BPM Increment(int step = 10) => new(Value + step);
        public BPM Decrement(int step = 10) => new(Math.Max(10, Value - step));

        public override int CompareTo(BPM other)
        {
            if (other is null)
            {
                return 1;
            }
            return Value.CompareTo(other.Value);
        }

        protected override bool EqualsCore(BPM other)
        {
            return other != null && Value == other.Value;
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