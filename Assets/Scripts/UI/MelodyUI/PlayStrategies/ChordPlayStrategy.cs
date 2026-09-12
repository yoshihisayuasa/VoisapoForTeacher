using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 和音のみを既定の拍数ぶん鳴らして終わる。
    /// </summary>
    public sealed class ChordPlayStrategy : MelodyPlayStrategy
    {
        public ChordPlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            yield return PlayChordOnce(piano, melody.ChordAt(pressedKey), settings);

            // 鳴り切ったらセッションを閉じる。ここで終えないと、開始時に立てた
            // 「先生が鳴らしている」表示を戻す通知（OnPlayEnded）が誰にも届かない。
            Context.FinishMelody();
        }

        // 離鍵では止めない（既定拍数で鳴り切る）。
        public override void OnKeyUp()
        {
        }

        // メロディパートを持たないため、区切るフレーズがない。
        public override void OnPlaybackFinished()
        {
        }
    }
}
