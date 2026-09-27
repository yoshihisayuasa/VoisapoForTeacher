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
        /// 対面限定にする場合は0.5秒にする
        /// </summary>
        public const float CaptureTailSec = 1.0f;

        /// <summary>
        /// 録音の頭から音の出だしを探す範囲（秒）。
        /// メロディは録音の起点で鳴り始めるので、出だしはスピーカーとマイクの遅れぶん後ろにしかない。
        /// これより後ろで見つかった音は伴奏の回り込みではない（話し声など）とみなし、頭を削らない。
        /// </summary>
        public const float OnsetSearchSec = 1.0f;

        /// <summary>
        /// 出だしとみなす音量。録音全体の最大音量に対する割合。
        /// 絶対値で決めると、マイクの感度や部屋の広さで当たり外れが出る。
        /// </summary>
        public const float OnsetThresholdRatio = 0.1f;

        /// <summary>
        /// 録音全体の最大音量がこれに届かなければ無音とみなし、頭を削らない。
        /// 無音に割合の閾値を当てると、雑音の山を出だしと誤る。
        /// </summary>
        public const float SilenceLevel = 0.02f;

        /// <summary>
        /// 出だしから手前に残す秒数。閾値を超えた瞬間で切ると、発音の立ち上がりが欠けて聴こえる。
        /// </summary>
        public const float OnsetPreRollSec = 0.01f;
    }
}
