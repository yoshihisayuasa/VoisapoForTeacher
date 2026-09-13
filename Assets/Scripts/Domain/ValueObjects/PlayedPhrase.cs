using Assets.Scripts.Domain.Entities;
using System.Collections.Generic;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 弾き終えた1フレーズ＝「どのメロディを、どのキーで弾いたか」。
    /// 自動転調では周ごとにキーが変わるため、この2つは切り離すと後から復元できない。
    /// メロディ各音の鍵盤の並びまでここで解決し、受け取った側にメロディと根音を
    /// 組み合わせ直させない。
    /// </summary>
    public readonly struct PlayedPhrase
    {
        private readonly Melody _melody;
        private readonly PianoNote _key;

        public PlayedPhrase(Melody melody, PianoNote key)
        {
            _melody = melody;
            _key = key;
        }

        /// <summary>メロディ各音が使う鍵盤と拍数を演奏順に。</summary>
        public IReadOnlyList<NoteVoicing> Notes => _melody.NotesAt(_key);
    }
}
