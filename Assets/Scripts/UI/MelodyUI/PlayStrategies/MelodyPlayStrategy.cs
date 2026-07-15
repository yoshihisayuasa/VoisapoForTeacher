using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// メロディ種別（MelodyKind）ごとの再生手順の抽象基底。和音セクションの共通手順も提供する。
    /// </summary>
    public abstract class MelodyPlayStrategy
    {
        protected IMelodyPlaybackContext Context { get; }

        protected MelodyPlayStrategy(IMelodyPlaybackContext context)
        {
            Context = context;
        }

        public abstract IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings);

        /// <summary>離鍵時の振る舞い。各種別が実装する（停止する種別だけ Context.FinishMelody を呼ぶ）。</summary>
        public abstract void OnKeyUp();

        /// <summary>
        /// 和音を拍数ぶん鳴らして止める共通手順。
        /// </summary>
        protected IEnumerator PlayChordOnce(PianoController piano, ChordVoicing chord, PlayModeSettings settings)
        {
            foreach (var key in chord.Keys)
            {
                piano.Play(key, settings.PlayCode);
            }

            for (int b = 0; b < chord.Beats; b++)
            {
                yield return TickBeat(settings);
            }

            foreach (var key in chord.Keys)
            {
                piano.Stop(key);
            }
        }

        /// <summary>
        /// 設定に応じてメトロノームを鳴らし、1拍ぶん待つ。
        /// 拍の長さは毎拍読み直すため、再生中の BPM 変更に追従する。
        /// </summary>
        protected IEnumerator TickBeat(PlayModeSettings settings)
        {
            if (settings.PlayMetronome)
            {
                Context.PlayMetronomeBeat();
            }
            yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat);
        }
    }
}
