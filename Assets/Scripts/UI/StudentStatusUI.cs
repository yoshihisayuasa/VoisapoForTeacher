using R3;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class StudentStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _waitingText = "Waiting for student login";
        [SerializeField] private string _loggedInText = "Student has logged in";
        [SerializeField] private string _loggedOutText = "Student has logged out";
        [SerializeField] private Color _loggedInColor = Color.blue;

        private Color _defaultColor;

        private void Start()
        {
            _defaultColor = _statusText.color;
            _statusText.text = _waitingText;

            _observer.OnStudentJoined
                .Subscribe(_ => Show(_loggedInText, _loggedInColor))
                .AddTo(this);

            _observer.OnStudentLeft
                .Subscribe(_ => Show(_loggedOutText, _defaultColor))
                .AddTo(this);
        }

        private void Show(string message, Color color)
        {
            _statusText.text = message;
            _statusText.color = color;
        }
    }
}
