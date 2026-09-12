using Assets.Scripts.Domain.ValueObjects;

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
        /// メロディを1回弾き終えたことを購読者へ通知する（録音はここまでを1フレーズとして切り出す）。
        /// 自動転調では周ごとに呼ばれ、周の途中で止まった演奏では呼ばれない。
        /// </summary>
        void NotifyMelodyEnded();

        /// <summary>
        /// 和音セクションに入った。この区間だけ Up↔Down 反転時の即時移調（±2半音）を受け付ける。
        /// 呼ぶのは自動転調ループを持つ戦略（Standard）のみ。対応しない戦略は呼ばない。
        /// </summary>
        void BeginChordSection();

        /// <summary>和音セクションを抜けた（即時移調を受け付けなくなる）。</summary>
        void EndChordSection();

        /// <summary>メトロノームを1拍鳴らす。</summary>
        void PlayMetronomeBeat();

        /// <summary>いま鳴らしている側（先生/生徒）を購読者へ通知する。周ごとに呼ぶ。</summary>
        void NotifyTeacherPlayStatus();

        /// <summary>演奏として終了する（演奏済み色は保持、録音停止は遅延）。離鍵停止・自動転調の自然終了で使う。</summary>
        void FinishMelody();

        /// <summary>自動転調ループを続けてよいか（トークン保持かつ転調が有効）。</summary>
        bool ShouldContinueAutoKeyChange();

        /// <summary>周境界で、もう音源側でなければトークンを返上してバトンを渡す。渡したら true。</summary>
        bool TryPassBatonAtBoundary();

        /// <summary>いま鳴らしているルートから、転調方向に沿った次のルート音を返す。</summary>
        PianoNote NextAutoKeyChangeRoot();

        /// <summary>
        /// 次の周のルートを確定する（再生位置の更新・鍵盤の選択・描画側への送信）。
        /// 3つは常にこの順で揃っている必要があるため、呼び出し側に並べさせず1操作にまとめる。
        /// </summary>
        void AdvanceTo(PianoNote nextKey);

        /// <summary>引き継ぎ待ちのバトンが保留されているか。</summary>
        bool HasPendingBaton { get; }
    }
}
