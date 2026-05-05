using AsseScripts.Domain;
using Assets.Scripts.UI.Melody;
using UnityEngine;
using DomainMelody = AsseScripts.Domain.Melody;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ下書きのプレビュー再生を MelodyPlayer に委譲する。
    /// </summary>
    public sealed class DraftMelodyPlayer : MonoBehaviour
    {
        public void Play(DomainMelody melody, DomainPianoNote root)
        {
            if (melody == null || root == null) return;

            var settings = MelodyPlayer.PlayModeSettings.FromFlags(isTeacherSide: true, earphoneOn: false);
            MelodyPlayer.Instance.PlayMelody(melody, root, settings);
        }

        public void Stop()
        {
            MelodyPlayer.Instance.StopMelody(true, shouldDelayRecordStop: false);
        }
    }
}
