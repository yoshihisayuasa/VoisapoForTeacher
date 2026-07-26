namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// Record &amp; Play の録音状態。
    /// Disabled → (クリック) → Standby → (メロディ再生開始) → Recording → (再生終了) → Standby。
    /// どの状態からもクリックで Disabled に戻る。
    /// </summary>
    public enum RecordingState
    {
        Disabled,
        Standby,
        Recording,
    }
}
