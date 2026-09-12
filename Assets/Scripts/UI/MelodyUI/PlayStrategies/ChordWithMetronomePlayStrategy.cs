using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 和音を鳴らしたあと、停止されるまでメトロノームだけを刻み続ける。
    /// </summary>
    public sealed class ChordWithMetronomePlayStrategy : MelodyPlayStrategy
    {
        public ChordWithMetronomePlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            yield return PlayChordOnce(piano, melody.ChordAt(pressedKey), settings);

            Context.NotifyMelodyBegan();
            while (true)
            {
                yield return TickBeat(settings);
            }
        }

        // 離鍵では止めない（停止操作まで刻み続ける）。
        public override void OnKeyUp()
        {
        }

        // 自分では終わらない種別なので、停止までが1フレーズになる。
        public override void OnPlaybackFinished()
        {
            Context.NotifyMelodyEnded();
        }
    }
}
