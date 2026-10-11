using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 最低音とその1オクターブ上を目印にする。和音だけを鳴らすメロディ用。
    /// 鳴らす音（根音〜5度）ではなく、和音を頼りに生徒が歌う音域の目安を示す。
    /// </summary>
    public sealed class OctaveHighlightRule : IHighlightRule
    {
        public Interval Top(Interval lowest, Interval highest)
        {
            return lowest + Interval.Octave;
        }
    }
}
