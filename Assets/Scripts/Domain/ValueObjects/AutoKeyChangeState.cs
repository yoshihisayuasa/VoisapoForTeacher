namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 自動キー変更の方向。
    /// </summary>
    public enum AutoKeyChangeState
    {
        None,
        Up,
        Down,
    }

    /// <summary>
    /// 自動キー変更の移調ルール。
    /// ループ1周ごとは半音±1、再生中の Up↔Down 反転時は半音±2 移調する。
    /// </summary>
    public static class AutoKeyChangeStateExtensions
    {
        public static bool IsActive(this AutoKeyChangeState state)
        {
            return state != AutoKeyChangeState.None;
        }

        /// <summary>
        /// ループ再生1周ごとの移調量。
        /// </summary>
        public static Interval NextRootStep(this AutoKeyChangeState state)
        {
            int step = state switch
            {
                AutoKeyChangeState.Up   => 1,
                AutoKeyChangeState.Down => -1,
                _                       => 0,
            };
            return new Interval(step);
        }

        /// <summary>
        /// previous からの Up↔Down 反転かどうか。
        /// </summary>
        public static bool IsOppositeOf(this AutoKeyChangeState current, AutoKeyChangeState previous)
        {
            return (previous == AutoKeyChangeState.Up && current == AutoKeyChangeState.Down) ||
                   (previous == AutoKeyChangeState.Down && current == AutoKeyChangeState.Up);
        }

        /// <summary>
        /// 反転時に即時移調する量。
        /// </summary>
        public static Interval DirectionSwapStep(this AutoKeyChangeState state)
        {
            return new Interval(state == AutoKeyChangeState.Up ? +2 : -2);
        }
    }
}
