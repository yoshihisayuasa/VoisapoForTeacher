using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public static class RoomIdHolder
    {
        public static RoomId Current { get; private set; }

        public static void Set(RoomId roomId) => Current = roomId;

        public static void Clear() => Current = null;

        /// <summary>ルームIDを発行していない（＝接続なしの対面レッスン）とき、対象を非表示にする。</summary>
        public static void HideUnlessIssued(GameObject target)
        {
            if (Current == null)
            {
                target.SetActive(false);
            }
        }

    }
}
