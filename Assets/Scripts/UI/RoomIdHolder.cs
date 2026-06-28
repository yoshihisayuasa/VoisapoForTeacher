using AsseScripts.Domain;

namespace Assets.Scripts.UI
{
    public static class RoomIdHolder
    {
        public static RoomId Current { get; private set; }

        public static void Set(RoomId roomId) => Current = roomId;

        public static void Clear() => Current = null;

    }
}
