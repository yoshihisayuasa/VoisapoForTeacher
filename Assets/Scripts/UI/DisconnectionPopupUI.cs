using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class DisconnectionPopupUI : MonoBehaviour
    {
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _studentLeftHeader = "Student has logged out";
        [SerializeField] private string _studentLeftBody = "";
        [SerializeField] private string _disconnectedHeader = "Connection lost";
        [SerializeField] private string _disconnectedBody = "Please check your internet connection.";

        private void Start()
        {
            _observer.OnStudentLeft
                .Subscribe(_ => ShowPopup(_studentLeftHeader, _studentLeftBody))
                .AddTo(this);

            _observer.OnTeacherDisconnected
                .Subscribe(_ => ShowPopup(_disconnectedHeader, _disconnectedBody))
                .AddTo(this);
        }

        private static void ShowPopup(string header, string body)
        {
            SimpleModalWindow.Create(ignorable: false)
                .SetHeader(header)
                .SetBody(body)
                .AddButton("OK", null, ModalButtonType.Success)
                .Show();
        }
    }
}
