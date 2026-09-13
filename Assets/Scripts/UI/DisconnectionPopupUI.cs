using Assets.Scripts.UI.Modal;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// ログアウト系イベントで確認ポップアップを表示する。
    /// 相手が退室したとき（先生シーンなら生徒、生徒シーンなら先生）と、
    /// 自分の接続が切れたときに、それぞれの本文を表示する。
    /// </summary>
    public sealed class DisconnectionPopupUI : MonoBehaviour
    {
        private static readonly LocalizedMessage ParticipantLeftBody = new(
            japanese: "相手がログアウトしました。",
            english: "The other participant has logged out");

        private static readonly LocalizedMessage DisconnectedBody = new(
            japanese: "接続が切れました。インターネット接続を確認してください。",
            english: "Connection lost. Please check your internet connection.");

        [SerializeField] private ConnectionStatusObserver _observer;

        private void Start()
        {
            _observer.OnParticipantLeft
                .Subscribe(_ => ConfirmModalUI.Show(ParticipantLeftBody.ForCurrentLanguage()))
                .AddTo(this);

            _observer.OnSelfDisconnected
                .Subscribe(_ => ConfirmModalUI.Show(DisconnectedBody.ForCurrentLanguage()))
                .AddTo(this);
        }

    }
}
