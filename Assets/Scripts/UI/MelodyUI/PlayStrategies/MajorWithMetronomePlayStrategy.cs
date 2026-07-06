using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 和音を鳴らしたあと、停止されるまでメトロノームだけを刻み続ける。
    /// </summary>
    public sealed class MajorWithMetronomePlayStrategy : MelodyPlayStrategyBase
    {
        public MajorWithMetronomePlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override bool SupportAutoKeyChange => false;
        public override bool CanDelete => false;
        public override bool StopOnKeyUp => false;

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            var chordKeys = melody.ChordKeysAt(pressedKey);
            yield return PlayChordOnce(piano, chordKeys, melody.Chord.Beats, settings);

            Context.NotifyMelodyBegan();
            float beatSec = BPMManager.Instance.SecondPerBeat;
            while (true)
            {
                if (settings.PlayMetronome)
                {
                    Context.PlayMetronomeBeat();
                }
                yield return new WaitForSeconds(beatSec);
            }
        }
    }
}
