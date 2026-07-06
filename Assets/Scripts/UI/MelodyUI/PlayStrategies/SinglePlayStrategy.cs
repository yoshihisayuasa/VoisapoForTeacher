using AsseScripts.Domain;
using AsseScripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 単音再生：押している間だけ根音を鳴らす（離鍵で停止）。
    /// </summary>
    public sealed class SinglePlayStrategy : MelodyPlayStrategyBase
    {
        public SinglePlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override bool SupportAutoKeyChange => false;
        public override bool CanDelete => false;
        public override bool StopOnKeyUp => true;

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            Context.NotifyMelodyBegan();
            piano.Play(pressedKey, settings.PlayPiano, VolumeManager.Instance.Volume);
            yield break;
        }
    }
}
