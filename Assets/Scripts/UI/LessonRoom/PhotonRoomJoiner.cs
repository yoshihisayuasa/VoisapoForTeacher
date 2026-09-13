using Assets.Scripts.Infrastructure;
using R3;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;

namespace Assets.Scripts.UI.LessonRoom
{
    public sealed class PhotonRoomJoiner : MonoBehaviourPunCallbacks
    {
        private const string _gameVersion = "0.1";

        private string _input;
        private bool _wantsToJoin;

        private readonly Subject<Unit> _onRoomJoined = new();
        public Observable<Unit> OnRoomJoined => _onRoomJoined;

        private readonly Subject<Unit> _onRoomNotFound = new();
        public Observable<Unit> OnRoomNotFound => _onRoomNotFound;

        private readonly Subject<Unit> _onRoomLeft = new();
        public Observable<Unit> OnRoomLeft => _onRoomLeft;

        private readonly Subject<string> _onConnectionError = new();
        public Observable<string> OnConnectionError => _onConnectionError;

        private void Awake()
        {
            PhotonNetwork.GameVersion = _gameVersion;
        }

        private void OnDestroy()
        {
            _onRoomJoined.Dispose();
            _onRoomNotFound.Dispose();
            _onRoomLeft.Dispose();
            _onConnectionError.Dispose();
        }

        public void Join(string input)
        {
            _input = input;
            _wantsToJoin = true;

            var props = new Hashtable
            {
                { VersionObserver.VersionKey, CurrentAppVersion.Value.ToString() }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinRoom(_input);
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void Leave()
        {
            _wantsToJoin = false;
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// バックグラウンドに入ったら自分から切断する。
        /// 接続を残すと、アプリが止まって音が鳴らないのに先生側では在室に見え続け、
        /// iOS ではサーバーのタイムアウト待ちで双方の表示が遅れる。即座に切ることで
        /// 先生側には退室、生徒側には復帰時に切断を、確実に表示させる。
        /// LeaveRoom はサーバーとの往復が要り停止前に終わらないため Disconnect を使う。
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                return;
            }

            PhotonNetwork.Disconnect();
        }

        public override void OnConnectedToMaster()
        {
            // LeaveRoom後のマスター再接続でも本コールバックは発火するため、
            // 明示的に入室を要求しているときだけ入室する（自動再入室を防ぐ）。
            if (!_wantsToJoin) return;
            PhotonNetwork.JoinRoom(_input);
        }

        public override void OnJoinedRoom()
        {
            _onRoomJoined.OnNext(Unit.Default);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            _onRoomNotFound.OnNext(Unit.Default);
        }

        public override void OnLeftRoom()
        {
            _onRoomLeft.OnNext(Unit.Default);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _onConnectionError.OnNext(cause.ToString());
        }
    }
}
