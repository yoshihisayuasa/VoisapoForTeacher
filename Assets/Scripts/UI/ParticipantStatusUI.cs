using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 相手参加者（先生シーンなら生徒、生徒シーンなら先生）の在室状態を
    /// 常時表示するステータステキスト。文言はシーンごとに SerializeField で差し替える。
    /// 日本語・英語を一組ずつ持ち、端末の言語設定で出し分ける。
    /// </summary>
    public sealed class ParticipantStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _waitingTextJapanese = "相手の参加を待っています";
        [FormerlySerializedAs("_waitingText")]
        [SerializeField] private string _waitingTextEnglish = "Waiting for the other participant";

        [SerializeField] private string _loggedInTextJapanese = "相手がログインしました";
        [FormerlySerializedAs("_loggedInText")]
        [SerializeField] private string _loggedInTextEnglish = "The other participant has logged in";

        [SerializeField] private string _loggedOutTextJapanese = "相手がログアウトしました";
        [FormerlySerializedAs("_loggedOutText")]
        [SerializeField] private string _loggedOutTextEnglish = "The other participant has logged out";

        [SerializeField] private string _selfDisconnectedTextJapanese = "ログアウトしました";
        [FormerlySerializedAs("_selfDisconnectedText")]
        [SerializeField] private string _selfDisconnectedTextEnglish = "You have logged out";

        [SerializeField] private Color _loggedInColor = Color.blue;

        private Color _defaultColor;

        private void Start()
        {
            _defaultColor = _statusText.color;
            Show(_waitingTextJapanese, _waitingTextEnglish, _defaultColor);

            _observer.OnParticipantJoined
                .Subscribe(_ => Show(_loggedInTextJapanese, _loggedInTextEnglish, _loggedInColor))
                .AddTo(this);

            _observer.OnParticipantLeft
                .Subscribe(_ => Show(_loggedOutTextJapanese, _loggedOutTextEnglish, _defaultColor))
                .AddTo(this);

            _observer.OnSelfDisconnected
                .Subscribe(_ => Show(_selfDisconnectedTextJapanese, _selfDisconnectedTextEnglish, _defaultColor))
                .AddTo(this);
        }

        private void Show(string japanese, string english, Color color)
        {
            _statusText.text = new LocalizedMessage(japanese, english).ForCurrentLanguage();
            _statusText.color = color;
        }
    }
}
