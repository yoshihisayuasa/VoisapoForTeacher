using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 録音した1フレーズ。声だけを持っていても「何を弾いていたときの声か」が分からなくなるため、
    /// そのとき弾いていたフレーズとテンポを一緒に抱え、鍵盤の動きごと聴き直せるようにする。
    ///
    /// 聴き直しでピアノの音は出さず、鍵盤の色だけを動かす。先生の端末で鳴らした音はマイクに
    /// 回り込んでクリップに入っているため、重ねると回り込みの遅れぶんずれた同じ音が2つ聴こえる。
    ///
    /// 鍵盤を鳴らす相手（PianoController）はUI層にあるため、このクラスもUI層に置く。
    /// </summary>
    public sealed class RecordedPhrase
    {
        private readonly AudioClip _clip;
        private readonly PlayedPhrase _phrase;

        // 録音時のテンポ。再生時に BPMManager を読み直すと、録音後にテンポを変えただけで
        // 鍵盤の動きが声からずれていく。声はもう録れた速さで固まっているので、そちらに合わせる。
        private readonly BPM _bpm;

        public RecordedPhrase(AudioClip clip, PlayedPhrase phrase, BPM bpm)
        {
            _clip = clip;
            _phrase = phrase;
            _bpm = bpm;
        }

        /// <summary>
        /// 声とメロディの鍵盤の動きを同時に走らせる。録音区間はメロディパートと一致しているため、
        /// 同時に始めれば最後まで重なる。和音パートは録音に入っていないので飛ばす。
        /// </summary>
        public IEnumerator Replay(PianoController piano, AudioSource source)
        {
            source.clip = _clip;
            source.Play();

            foreach (var note in _phrase.Notes)
            {
                piano.Play(note.Key, isPlaySound: false);
                yield return new WaitForSeconds(_bpm.SecondPerBeat * note.Beats);
                piano.StopAndMarkPlayed(note.Key);
            }
        }
    }
}
