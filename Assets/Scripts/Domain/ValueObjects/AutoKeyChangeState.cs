using System;

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
        /// 自動キー変更が有効（<see cref="IsActive"/>）なときだけ呼ぶ。
        /// </summary>
        public static Interval NextRootStep(this AutoKeyChangeState state)
        {
            int step = state switch
            {
                AutoKeyChangeState.Up   => 1,
                AutoKeyChangeState.Down => -1,
                _                       => throw new InvalidOperationException($"自動キー変更が無効な状態で移調量を求めた: {state}"),
            };
            return new Interval(step);
        }

        /// <summary>
        /// previous からの Up↔Down 反転なら、即時移調する量を返す。
        /// 反転でなければ false を返し、step は null になる。
        /// </summary>
        public static bool TryGetDirectionSwapStep(this AutoKeyChangeState current, AutoKeyChangeState previous, out Interval step)
        {
            if (!current.IsOppositeOf(previous))
            {
                step = null;
                return false;
            }

            step = new Interval(current == AutoKeyChangeState.Up ? +2 : -2);
            return true;
        }

        private static bool IsOppositeOf(this AutoKeyChangeState current, AutoKeyChangeState previous)
        {
            return (previous == AutoKeyChangeState.Up && current == AutoKeyChangeState.Down) ||
                   (previous == AutoKeyChangeState.Down && current == AutoKeyChangeState.Up);
        }
    }
}
