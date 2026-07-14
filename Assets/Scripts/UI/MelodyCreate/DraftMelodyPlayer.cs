using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ下書きのプレビュー再生を MelodyPlayer に委譲する。
    /// </summary>
    public sealed class DraftMelodyPlayer : MonoBehaviour
    {
        private void Start()
        {
            // 「演奏済み」色を残すのは演奏画面の機能（FinishMelody）。鍵盤は DontDestroyOnLoad で
            // 生き続けるため、作成画面では入場時とプレビュー終了のたびに全色をリセットする。
            var piano = PianoController.Instance;
            piano.StopAllKeys(true);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(_ => piano.StopAllKeys(true))
                .AddTo(this);
        }

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
