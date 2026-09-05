using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Photon のルーム接続状態を購読可能なイベントとして公開する。
    /// コールバックの意味は役割に依存しない（先生・生徒どちらのシーンでも使える）：
    /// 相手が入室した / 相手が退室した / 自分の接続が切れた、の3種。
    ///
    /// これは表示用（ポップアップ・ステータス文言・ボタンの活性）の窓口。
    /// 再生や同期の状態を既定へ戻す処理は PianoNetworkGateway 側のストリームが担当する。
    /// </summary>
    public class ConnectionStatusObserver : MonoBehaviourPunCallbacks
    {
        private readonly Subject<Unit> _onParticipantJoined = new();
        public Observable<Unit> OnParticipantJoined => _onParticipantJoined;

        private readonly Subject<Unit> _onParticipantLeft = new();
        public Observable<Unit> OnParticipantLeft => _onParticipantLeft;

        private readonly Subject<Unit> _onSelfDisconnected = new();
        public Observable<Unit> OnSelfDisconnected => _onSelfDisconnected;

        private void OnDestroy()
        {
            _onParticipantJoined.Dispose();
            _onParticipantLeft.Dispose();
            _onSelfDisconnected.Dispose();
        }

        public override void OnJoinedRoom()
        {
            // 自分より先に相手が入室済みの場合、OnPlayerEnteredRoom は発火しないため、
            // ここで参加を通知する（生徒が、先に入室している先生の部屋に入るケース）。
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                _onParticipantJoined.OnNext(Unit.Default);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _onParticipantJoined.OnNext(Unit.Default);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _onParticipantLeft.OnNext(Unit.Default);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _onSelfDisconnected.OnNext(Unit.Default);
        }
    }
}
