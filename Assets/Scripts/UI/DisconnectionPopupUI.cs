using Assets.Scripts.UI.Modal;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class DisconnectionPopupUI : MonoBehaviour
    {
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _studentLeftBody = "Student has logged out";
        [SerializeField] private string _disconnectedBody = "Connection lost. Please check your internet connection.";

        private void Start()
        {
            _observer.OnStudentLeft
                .Subscribe(_ => ConfirmModalUI.Show(_studentLeftBody))
                .AddTo(this);

            _observer.OnTeacherDisconnected
                .Subscribe(_ => ConfirmModalUI.Show(_disconnectedBody))
                .AddTo(this);
        }

    }
}
