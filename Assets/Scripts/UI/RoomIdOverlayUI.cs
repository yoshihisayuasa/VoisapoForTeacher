using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class RoomIdOverlayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roomIdText;

        private void Start()
        {
            LessonModeHolder.ShowRoomId(_roomIdText);
        }
    }
}
