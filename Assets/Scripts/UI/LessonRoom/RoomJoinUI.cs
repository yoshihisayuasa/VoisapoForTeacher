using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Modal;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LessonRoom
{
    public sealed class RoomJoinUI : MonoBehaviour
    {
        private const string _loginLabel = "Login";
        private const string _logoutLabel = "Logout";

        private static readonly LocalizedMessage RoomNotFoundBody = new(
            japanese: "ログインできませんでした。IDを確認してください。",
            english: "Login failed. Please check the ID.");

        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private PhotonRoomJoiner _photonRoomJoiner;

        private bool _isJoined;
        private bool _isBusy = false;

        private void Start()
        {
            _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            _inputField.characterLimit = RoomId.Digits;

            _joinButton.onClick.AddListener(OnButtonClicked);
            _inputField.onValueChanged.AddListener(_ => RefreshButton());
            _inputField.onSubmit.AddListener(_ => OnButtonClicked());

            _photonRoomJoiner.OnRoomJoined
                .Subscribe(_ => OnJoined())
                .AddTo(this);

            _photonRoomJoiner.OnRoomLeft
                .Subscribe(_ => OnLeft())
                .AddTo(this);

            _photonRoomJoiner.OnRoomNotFound
                .Subscribe(_ => OnRoomNotFound())
                .AddTo(this);

            _photonRoomJoiner.OnConnectionError
                .Subscribe(_ => OnDisconnected())
                .AddTo(this);

            RefreshButton();
        }

        private bool CanInteract()
        {
            if (_isBusy)
            {
                return false;
            }

            return _isJoined || _inputField.text.Length == RoomId.Digits;
        }

        private void RefreshButton()
        {
            _joinButton.interactable = CanInteract();
            _buttonLabel.text = _isJoined ? _logoutLabel : _loginLabel;
            _inputField.interactable = !_isJoined;
        }

        private void OnButtonClicked()
        {
            if (!CanInteract())
            {
                return;
            }

            _isBusy = true;
            RefreshButton();

            if (_isJoined)
            {
                _photonRoomJoiner.Leave();
            }
            else
            {
                _photonRoomJoiner.Join(_inputField.text);
            }
        }

        private void OnJoined()
        {
            _isJoined = true;
            _isBusy = false;
            RefreshButton();
        }

        private void OnLeft()
        {
            _isJoined = false;
            _isBusy = false;
            RefreshButton();
        }

        private void OnRoomNotFound()
        {
            _isBusy = false;
            RefreshButton();
            ConfirmModalUI.Show(RoomNotFoundBody.ForCurrentLanguage());
        }

        private void OnDisconnected()
        {
            _isJoined = false;
            _isBusy = false;
            RefreshButton();
        }
    }
}
