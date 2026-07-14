using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 音量値オブジェクト（範囲外は0.0〜1.0に丸めて保持する）
    /// </summary>
    public sealed class Volume : ValueObject<Volume>
    {
        public float Value { get; }

        public Volume(float value)
        {
            Value = Math.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// 音量に倍率を掛けた新しい音量を返す（結果は0.0〜1.0に丸められる）。
        /// </summary>
        public Volume Scale(float multiplier)
        {
            return new Volume(Value * multiplier);
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