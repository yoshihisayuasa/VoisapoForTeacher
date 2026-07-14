namespace Assets.Scripts.Domain.StaticValues
{
    /// <summary>
    /// フレーズ録音のルール。
    /// </summary>
    public static class RecordingRules
    {
        /// <summary>1フレーズあたりの録音上限秒数。</summary>
        public const int MaxPhraseSec = 30;

        /// <summary>録音のサンプリングレート（Hz）。</summary>
        public const int SampleRate = 44100;
    }
}
