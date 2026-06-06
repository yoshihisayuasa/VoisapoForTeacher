using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class RoomIdOverlayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roomIdText;

        private void Start()
        {
            if (RoomIdHolder.Current == null) return;
            _roomIdText.text = "Room ID : " + RoomIdHolder.Current.Value;
        }
    }
}
