using AsseScripts.Domain;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 和音のみを既定の拍数ぶん鳴らして終わる。
    /// </summary>
    public sealed class MajorChordPlayStrategy : MelodyPlayStrategyBase
    {
        public MajorChordPlayStrategy(IMelodyPlaybackContext context) : base(context)
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
        }
    }
}
