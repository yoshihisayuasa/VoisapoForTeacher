namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 再生戦略から MelodyPlayer 本体への通知・依頼の窓口。
    /// 戦略が MelodyPlayer の内部状態を直接触らないための境界。
    /// </summary>
    public interface IMelodyPlaybackContext
    {
        /// <summary>メロディパート開始を購読者へ通知する。</summary>
        void NotifyMelodyBegan();

        /// <summary>
        /// 和音セクションに入った。この区間だけ Up↔Down 反転時の即時移調（±2半音）を受け付ける。
        /// 呼ぶのは自動転調対応（SupportAutoKeyChange）の戦略のみ。対応しない戦略は呼ばない。
        /// </summary>
        void BeginChordSection();

        /// <summary>和音セクションを抜けた（即時移調を受け付けなくなる）。</summary>
        void EndChordSection();

        /// <summary>メトロノームを1拍鳴らす。</summary>
        void PlayMetronomeBeat();
    }
}
