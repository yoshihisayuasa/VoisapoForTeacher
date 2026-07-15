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

    public static class MelodyKinds
    {
        /// <summary>
        /// 組み込みメロディの表示名から再生種別を解決する。
        /// 種別をデータに持たない旧フォーマットの移行で使う。対応する名前が無ければ Standard（通常再生）。
        /// </summary>
        public static MelodyKind FromDisplayName(string name) => name switch
        {
            "Single"           => MelodyKind.Single,
            "Major& Metronome" => MelodyKind.ChordWithMetronome,
            "Major Code"       => MelodyKind.Chord,
            _                  => MelodyKind.Standard,
        };
    }
}
