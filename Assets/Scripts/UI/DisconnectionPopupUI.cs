using Assets.Scripts.UI.Modal;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// ログアウト系イベントで確認ポップアップを表示する。
    /// 相手が退室したとき（先生シーンなら生徒、生徒シーンなら先生）と、
    /// 自分の接続が切れたときに、それぞれ設定された本文を表示する。
    /// 文言はシーンごとに <see cref="_participantLeftBody"/> / <see cref="_disconnectedBody"/> で差し替える。
    /// </summary>
    public sealed class DisconnectionPopupUI : MonoBehaviour
    {
        [SerializeField] private ConnectionStatusObserver _observer;

        [SerializeField] private string _participantLeftBody = "The other participant has logged out";
        [SerializeField] private string _disconnectedBody = "Connection lost. Please check your internet connection.";

        private void Start()
        {
            _observer.OnParticipantLeft
                .Subscribe(_ => ConfirmModalUI.Show(_participantLeftBody))
                .AddTo(this);

            _observer.OnSelfDisconnected
                .Subscribe(_ => ConfirmModalUI.Show(_disconnectedBody))
                .AddTo(this);
        }

    }
}
