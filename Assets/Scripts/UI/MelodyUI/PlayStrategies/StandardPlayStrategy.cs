using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 標準再生：和音パートに続けてメロディパートを1音ずつ鳴らす。
    /// 自動転調に対応する唯一の種別。
    /// </summary>
    public sealed class StandardPlayStrategy : MelodyPlayStrategyBase
    {
        public StandardPlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override bool SupportAutoKeyChange => true;
        public override bool StopOnKeyUp => false;

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            // ── 和音パート ──
            Context.BeginChordSection();
            yield return PlayChordOnce(piano, melody.ChordAt(pressedKey), settings);
            Context.EndChordSection();

            // ── メロディパート ──
            Context.NotifyMelodyBegan();
            foreach (var note in melody.NotesAt(pressedKey))
            {
                piano.Play(note.Key, settings.PlayPiano, VolumeManager.Instance.Volume);
                yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                piano.StopAndMarkPlayed(note.Key);
            }
        }
    }
}
