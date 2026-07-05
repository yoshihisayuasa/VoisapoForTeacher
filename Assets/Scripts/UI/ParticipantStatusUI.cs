using R3;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 相手参加者（先生シーンなら生徒、生徒シーンなら先生）の在室状態を
    /// 常時表示するステータステキスト。文言はシーンごとに SerializeField で差し替える。
    /// </summary>
    public sealed class ParticipantStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _waitingText = "Waiting for the other participant";
        [SerializeField] private string _loggedInText = "The other participant has logged in";
        [SerializeField] private string _loggedOutText = "The other participant has logged out";
        [SerializeField] private string _selfDisconnectedText = "You have logged out";
        [SerializeField] private Color _loggedInColor = Color.blue;

        private Color _defaultColor;

        private void Start()
        {
            _defaultColor = _statusText.color;
            _statusText.text = _waitingText;

            _observer.OnParticipantJoined
                .Subscribe(_ => Show(_loggedInText, _loggedInColor))
                .AddTo(this);

            _observer.OnParticipantLeft
                .Subscribe(_ => Show(_loggedOutText, _defaultColor))
                .AddTo(this);

            _observer.OnSelfDisconnected
                .Subscribe(_ => Show(_selfDisconnectedText, _defaultColor))
                .AddTo(this);
        }

        private void Show(string message, Color color)
        {
            _statusText.text = message;
            _statusText.color = color;
        }
    }
}
