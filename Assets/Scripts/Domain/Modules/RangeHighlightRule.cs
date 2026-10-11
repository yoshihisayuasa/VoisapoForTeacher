using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 音域の両端（最低音と最高音）を目印にする。メロディパートを持つ通常のメロディ用。
    /// </summary>
    public sealed class RangeHighlightRule : IHighlightRule
    {
        public Interval Top(Interval lowest, Interval highest)
        {
            return highest;
        }
    }
}
