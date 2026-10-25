using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LessonRoom
{
    /// <summary>
    /// スタート画面。ルームIDは発行ボタンを押したときだけ発行する（もう一度押すと再発行）。
    /// 対面レッスンはいつでも始められる（発行済みでも接続を手放して始める）。
    /// オンラインレッスンは ID の発行が終わってから始められる。
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
        [FormerlySerializedAs("_startButton")]
        [SerializeField] private Button _inPersonStartButton;
        [SerializeField] private Button _onlineStartButton;
        private readonly string _nextSceneName = "TeacherMain";
        [SerializeField] private PhotonRoomCreator _photonRoomCreator;

        // 発行が終わったときに作る。オンラインの開始ボタンは発行が終わるまで押せないので、
        // 押された時点では必ず入っている。
        private OnlineLesson _onlineLesson;

        private void Start()
        {
            _roomIdText.text = _roomIdLabel + _unissuedPlaceholder;
            _onlineStartButton.interactable = false;

            _issueButton.onClick.AddListener(OnIssueClicked);
            _inPersonStartButton.onClick.AddListener(() => StartInPersonAsync().Forget());
            _onlineStartButton.onClick.AddListener(OnOnlineStartClicked);

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
            _onlineStartButton.interactable = false;

            _photonRoomCreator.Issue();
        }

        private void OnRoomCreated(RoomId roomId)
        {
            _roomIdText.text = _roomIdLabel + roomId.Value;
            _onlineLesson = new OnlineLesson(roomId);
            _onlineStartButton.interactable = true;
        }

        private void OnConnectionError()
        {
            _roomIdText.text = ConnectionFailedText.ForCurrentLanguage();
            _onlineStartButton.interactable = false;
        }

        private async UniTaskVoid StartInPersonAsync()
        {
            // 切断を待つ間に発行や開始を重ねて押されないようにする。
            _issueButton.interactable = false;
            _inPersonStartButton.interactable = false;
            _onlineStartButton.interactable = false;

            await _photonRoomCreator.AbandonAsync();

            LessonModeHolder.Set(new InPersonLesson());
            SceneManager.LoadScene(_nextSceneName);
        }

        private void OnOnlineStartClicked()
        {
            LessonModeHolder.Set(_onlineLesson);
            SceneManager.LoadScene(_nextSceneName);
        }
    }
}
