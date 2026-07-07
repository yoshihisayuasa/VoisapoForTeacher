using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LessonRoom
{
    public sealed class RoomIDPublisher : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roomIdText;
        [SerializeField] private Button _startButton;
        private readonly string _nextSceneName = "TeacherMain";
        [SerializeField] private PhotonRoomCreator _photonRoomCreator;

        private void Start()
        {
            _roomIdText.text = string.Empty;
            _startButton.onClick.AddListener(OnStartClicked);

            _photonRoomCreator.OnRoomCreated
                .Subscribe(OnRoomCreated)
                .AddTo(this);

            _photonRoomCreator.OnConnectionError
                .Subscribe(_ => _roomIdText.text = "Connection failed. Please check your internet connection.")
                .AddTo(this);
        }

        private void OnRoomCreated(RoomId roomId)
        {
            _roomIdText.text = "Room ID : " + roomId.Value;
            RoomIdHolder.Set(roomId);
        }

        private void OnStartClicked()
        {
            SceneManager.LoadScene(_nextSceneName);
        }
    }
}
