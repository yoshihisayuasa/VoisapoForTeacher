using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 単音再生：押している間だけ根音を鳴らす（離鍵で停止）。
    /// </summary>
    public sealed class SinglePlayStrategy : MelodyPlayStrategy
    {
        public SinglePlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            Context.NotifyMelodyBegan();
            piano.Play(pressedKey, settings.PlayPiano);
            yield break;
        }

        // 押している間だけ根音を鳴らす種別なので、離鍵で演奏を終える。
        public override void OnKeyUp()
        {
            Context.FinishMelody();
        }
    }
}
