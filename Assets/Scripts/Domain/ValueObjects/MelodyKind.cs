namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// メロディの再生種別。再生方式の分岐はこの型を根拠に行う（表示名の文字列比較をしない）。
    /// </summary>
    public enum MelodyKind
    {
        Standard,
        Single,
        ChordWithMetronome,
        Chord,
    }
}
