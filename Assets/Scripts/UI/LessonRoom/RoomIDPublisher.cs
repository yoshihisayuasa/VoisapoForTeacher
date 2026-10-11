using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LessonRoom
{
    /// <summary>
    /// スタート画面。ルームIDは発行ボタンを押したときだけ発行する（もう一度押すと再発行）。
    /// 発行せずに（または発行が終わる前に）Start を押せば、接続なしの対面レッスンとして始まる。
    /// </summary>
    public sealed class RoomIDPublisher : MonoBehaviour
    {
        private const string _roomIdLabel = "Room ID : ";

        // 未発行のとき、ID が入る場所を桁数ぶんの下線で示す。
        private static readonly string _unissuedPlaceholder = new('_', RoomId.Digits);

        private static readonly LocalizedMessage ConnectingText = new(
            japanese: "接続中...",
            english: "Connecting...");

        private static readonly LocalizedMessage ConnectionFailedText = new(
            japanese: "接続できませんでした。インターネット接続を確認してください。",
            english: "Connection failed. Please check your internet connection.");

        [SerializeField] private TMP_Text _roomIdText;
        [SerializeField] private Button _issueButton;
        [SerializeField] private Button _startButton;
        private readonly string _nextSceneName = "TeacherMain";
        [SerializeField] private PhotonRoomCreator _photonRoomCreator;

        private void Start()
        {
            // この画面に来た時点で前回のルームは無効（PhotonRoomCreator が接続を手放す）。
            RoomIdHolder.Clear();

            _roomIdText.text = _roomIdLabel + _unissuedPlaceholder;
            _issueButton.onClick.AddListener(OnIssueClicked);
            _startButton.onClick.AddListener(OnStartClicked);

            _photonRoomCreator.OnRoomCreated
                .Subscribe(OnRoomCreated)
                .AddTo(this);

            _photonRoomCreator.OnConnectionError
                .Subscribe(_ => OnConnectionError())
                .AddTo(this);
        }

        private void OnIssueClicked()
        {
            _roomIdText.text = ConnectingText.ForCurrentLanguage();

            // 再発行のときは、ここで前のIDが無効になる。
            RoomIdHolder.Clear();

            _photonRoomCreator.Issue();
        }

        private void OnRoomCreated(RoomId roomId)
        {
            _roomIdText.text = _roomIdLabel + roomId.Value;
            RoomIdHolder.Set(roomId);
        }

        private void OnConnectionError()
        {
            _roomIdText.text = ConnectionFailedText.ForCurrentLanguage();
            RoomIdHolder.Clear();
        }

        private void OnStartClicked()
        {
            SceneManager.LoadScene(_nextSceneName);
        }
    }
}
