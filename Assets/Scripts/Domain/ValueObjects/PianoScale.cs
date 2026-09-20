using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// ピアノ鍵盤の表示倍率（範囲外は Min〜Max に丸めて保持する）
    /// </summary>
    public sealed class PianoScale : ValueObject<PianoScale>
    {
        public const float Min = 0.5f;
        public const float Max = 2.0f;

        /// <summary>保存された倍率が無いときに使う既定の倍率。</summary>
        public const float Default = 1.0f;

        public float Value { get; }

        public PianoScale(float value)
        {
            Value = Math.Clamp(value, Min, Max);
        }

        /// <summary>
        /// 現在の倍率に増減を加えた新しい倍率を返す（結果は Min〜Max に丸められる）。
        /// </summary>
        public PianoScale Zoom(float delta)
        {
            return new PianoScale(Value + delta);
        }

        public PianoScale ZoomIn(float step)
        {
            return new PianoScale(Value * (1f + step));
        }

        public PianoScale ZoomOut(float step)
        {
            return new PianoScale(Value / (1f + step));
        }

        protected override bool EqualsCore(PianoScale other) => Value.Equals(other.Value);
        protected override int GetHashCodeCore() => Value.GetHashCode();
    }
}
