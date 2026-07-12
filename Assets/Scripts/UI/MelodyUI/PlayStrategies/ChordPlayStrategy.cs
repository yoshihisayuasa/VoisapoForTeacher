using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 和音のみを既定の拍数ぶん鳴らして終わる。
    /// </summary>
    public sealed class ChordPlayStrategy : MelodyPlayStrategyBase
    {
        public ChordPlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override bool SupportAutoKeyChange => false;
        public override bool StopOnKeyUp => false;

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            yield return PlayChordOnce(piano, melody.ChordAt(pressedKey), settings);
        }
    }
}
