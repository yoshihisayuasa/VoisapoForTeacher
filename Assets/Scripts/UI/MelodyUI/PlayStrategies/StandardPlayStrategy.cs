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
    /// 自動転調と削除に対応する唯一の種別。
    /// </summary>
    public sealed class StandardPlayStrategy : MelodyPlayStrategyBase
    {
        public StandardPlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override bool SupportAutoKeyChange => true;
        public override bool CanDelete => true;
        public override bool StopOnKeyUp => false;

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            // ── 和音パート ──
            var chordKeys = melody.ChordKeysAt(pressedKey);

            Context.BeginChordSection();
            yield return PlayChordOnce(piano, chordKeys, melody.Chord.Beats, settings);
            Context.EndChordSection();

            // ── メロディパート ──
            Context.NotifyMelodyBegan();
            foreach (var note in melody.Notes)
            {
                var key = pressedKey + note.Interval;
                piano.Play(key, settings.PlayPiano, VolumeManager.Instance.Volume);
                yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                piano.StopAndMarkPlayed(key);
            }
        }
    }
}
