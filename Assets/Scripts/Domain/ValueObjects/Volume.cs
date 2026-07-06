using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 音量値オブジェクト（0.0〜1.0のみ許容）
    /// </summary>
    public sealed class Volume : ValueObject<Volume>
    {
        public float Value { get; }

        public Volume(float value)
        {
            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(nameof(value), "音量は0.0〜1.0の範囲でなければなりません。");
            Value = value;
        }

        protected override bool EqualsCore(Volume other) => Value.Equals(other.Value);
        protected override int GetHashCodeCore() => Value.GetHashCode();

        public override int CompareTo(Volume other)
        {
            if (other is null) return 1;
            return Value.CompareTo(other.Value);
        }
    }
}