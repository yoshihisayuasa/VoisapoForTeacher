using AsseScripts.Domain;
using AsseScripts.UI;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// MelodyDraft の内容をピアノで試聴再生する。
    /// 和音を鳴らした後、メロディを順に鳴らす。
    /// </summary>
    public sealed class DraftMelodyPlayer : MonoBehaviour
    {
        private const float BeatSec = 0.5F;

        private Coroutine _playCoroutine;

        public void Play(MelodyDraft draft)
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
            }
            _playCoroutine = StartCoroutine(PlaySequence(draft));
        }

        public void Stop()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }
        }

        private IEnumerator PlaySequence(MelodyDraft draft)
        {
            var piano = PianoController.Instance;

            // 和音を鳴らす
            for (int i = 0; i < Chord.Length; i++)
            {
                if (draft.Steps[i] is NoteStep ns)
                {
                    piano.Play(ns.Key, true, VolumeManager.Instance.Volume);
                }
            }

            // メロディ先頭の ExtendStep 分だけ和音の拍数を延長する
            int chordBeats = 1;
            int melodyStart = Chord.Length;
            while (melodyStart < draft.Steps.Count && draft.Steps[melodyStart] is ExtendStep)
            {
                chordBeats++;
                melodyStart++;
            }

            yield return new WaitForSeconds(BeatSec * chordBeats);

            for (int i = 0; i < Chord.Length; i++)
            {
                if (draft.Steps[i] is NoteStep ns)
                {
                    piano.Stop(ns.Key, false);
                }
            }

            // メロディを鳴らす
            int idx = melodyStart;
            while (idx < draft.Steps.Count)
            {
                if (draft.Steps[idx] is NoteStep noteStep)
                {
                    int beats = 1;
                    int next = idx + 1;
                    while (next < draft.Steps.Count && draft.Steps[next] is ExtendStep)
                    {
                        beats++;
                        next++;
                    }

                    piano.Play(noteStep.Key, true, VolumeManager.Instance.Volume);
                    yield return new WaitForSeconds(BeatSec * beats);
                    piano.Stop(noteStep.Key, false);

                    idx = next;
                }
                else
                {
                    idx++;
                }
            }

            _playCoroutine = null;
        }
    }
}
