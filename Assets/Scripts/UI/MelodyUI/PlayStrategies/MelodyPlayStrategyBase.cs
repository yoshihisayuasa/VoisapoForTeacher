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
    /// 再生戦略の共通基盤。和音セクションの再生手順を提供する。
    /// </summary>
    public abstract class MelodyPlayStrategyBase : IMelodyPlayStrategy
    {
        protected IMelodyPlaybackContext Context { get; }

        protected MelodyPlayStrategyBase(IMelodyPlaybackContext context)
        {
            Context = context;
        }

        public abstract bool SupportAutoKeyChange { get; }
        public abstract bool CanDelete { get; }
        public abstract bool StopOnKeyUp { get; }

        public abstract IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings);

        /// <summary>
        /// 和音を beats 拍ぶん鳴らして止める共通手順。
        /// </summary>
        protected IEnumerator PlayChordOnce(PianoController piano, IReadOnlyList<PianoNote> chordKeys,
                                            int beats, PlayModeSettings settings)
        {
            foreach (var key in chordKeys)
            {
                piano.Play(key, settings.PlayCode, VolumeManager.Instance.Volume);
            }

            float beatSec = BPMManager.Instance.SecondPerBeat;
            for (int b = 0; b < beats; b++)
            {
                if (settings.PlayMetronome)
                {
                    Context.PlayMetronomeBeat();
                }
                yield return new WaitForSeconds(beatSec);
            }

            foreach (var key in chordKeys)
            {
                piano.Stop(key);
            }
        }
    }
}
