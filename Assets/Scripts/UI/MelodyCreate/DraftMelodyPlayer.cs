using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
using UnityEngine;
using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ下書きのプレビュー再生を MelodyPlayer に委譲する。
    /// </summary>
    public sealed class DraftMelodyPlayer : MonoBehaviour
    {
        public void Play(Melody melody, PianoNote root)
        {
            if (melody == null || root == null) return;

            PianoController.Instance.SelectKey(root);
            var settings = PlayModeSettings.FromFlags(isTeacherSide: true, earphoneOn: false);
            MelodyPlayer.Instance.PlayMelody(melody, settings);
        }

        public void Stop()
        {
            MelodyPlayer.Instance.StopMelodyAndReset();
        }
    }
}
