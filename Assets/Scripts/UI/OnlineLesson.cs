using Assets.Scripts.Domain.ValueObjects;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// オンラインレッスン。発行済みのルームIDを必ず持つ。
    /// </summary>
    public sealed class OnlineLesson : ILessonMode
    {
        private const string _roomIdLabel = "Room ID : ";

        private readonly RoomId _roomId;

        public OnlineLesson(RoomId roomId)
        {
            _roomId = roomId;
        }

        public void HideInPersonOnly(GameObject target)
        {
            target.SetActive(false);
        }

        public void HideOnlineOnly(GameObject target)
        {
        }

        public void ShowRoomId(TMP_Text text)
        {
            text.text = _roomIdLabel + _roomId.Value;
        }
    }
}
