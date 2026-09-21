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

        /// <summary>
        /// 先生の再生が終わってから録音を締めるまでの秒数。
        /// 生徒の声は Zoom を経由して遅れて先生のスピーカーから出るため、
        /// 弾き終わりで即座に切ると歌い終わりが欠ける。
        /// 遅延は回線しだいで揺れるので長めに取る（伸びても末尾に余白が入るだけ）。
        /// 内訳：Zoom 0.5 ＋ マイク入力 0.1 ＋ Bluetooth スピーカー 0.3 ＋ 余裕 0.1。
        /// </summary>
        public const float CaptureTailSec = 5.0f;
    }
}
