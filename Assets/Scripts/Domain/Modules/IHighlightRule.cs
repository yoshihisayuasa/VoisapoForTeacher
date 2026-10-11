using Assets.Scripts.Domain.ValueObjects;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// メロディが鍵盤に出す目印の決め方。メロディごとに1つ持ち、種別を見て分岐しないための入り口。
    /// 下端は常に音域の最低音で、上端だけがメロディによって変わる。
    /// </summary>
    public interface IHighlightRule
    {
        /// <summary>
        /// 目印の上端を、音域の最低音・最高音（根音からのインターバル）から決める。
        /// </summary>
        Interval Top(Interval lowest, Interval highest);
    }
}
