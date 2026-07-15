
namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// MelodyDraft が保持する音符。押した鍵の絶対音と拍数を持つ不変オブジェクト。
    /// インターバルへの変換は MelodyDraft.Build() で行う。
    /// </summary>
    public sealed class DraftNote : ValueObject<DraftNote>
    {
        public PianoNote Key { get; }
        public int Beats { get; }

        public string DisplayText => Key.DisplayText;

        public DraftNote(PianoNote key, int beats = 1)
        {
            Key = key;
            Beats = beats;
        }

        public DraftNote WithBeats(int beats) => new(Key, beats);

        protected override bool EqualsCore(DraftNote other)
        {
            return Key == other.Key && Beats == other.Beats;
        }

        protected override int GetHashCodeCore()
        {
            return (Key.Index * 397) ^ Beats;
        }
    }
}
