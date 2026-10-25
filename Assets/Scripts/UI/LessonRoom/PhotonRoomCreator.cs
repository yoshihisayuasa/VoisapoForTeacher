using Assets.Scripts.Domain.StaticValues;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI.LessonRoom
{
    /// <summary>
    /// 先生が発行を求めたときだけ Photon に接続してルームを作る。
    /// Photon の同時接続数（CCU）はルーム数ではなく接続数で数えられるため、
    /// 対面レッスン（ID を発行しない）ではそもそも接続しない。
    /// </summary>
    public sealed class PhotonRoomCreator : MonoBehaviourPunCallbacks
    {
        private const string _gameVersion = "0.1";

        private RoomId _pendingRoomId;

        private UniTaskCompletionSource _abandonment;

        private readonly Subject<RoomId> _onRoomCreated = new();
        public Observable<RoomId> OnRoomCreated => _onRoomCreated;

        private readonly Subject<string> _onConnectionError = new();
        public Observable<string> OnConnectionError => _onConnectionError;

        private void Start()
        {
            PhotonNetwork.GameVersion = _gameVersion;

            // 生徒が入室時に確認できるよう、先生のアプリバージョンを公開する。
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                { VersionObserver.VersionKey, CurrentAppVersion.Value.ToString() }
            });

            // レッスンから戻ってきたときは前回の接続を手放し、同時接続数の枠を空ける。
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }

        private void OnDestroy()
        {
            // 発行が終わる前に画面を離れた場合、接続だけが残って同時接続数の枠を使い続けてしまう。
            // ルームに入れていない接続はここで手放す。
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
            }

            _onRoomCreated.Dispose();
            _onConnectionError.Dispose();
        }

        /// <summary>
        /// ルームIDの発行を求める。結果は OnRoomCreated か OnConnectionError で通知する。
        /// 発行済みのときは今のルームを捨てて新しいIDで作り直す（再発行）。
        /// 失敗時の再試行は自動では行わず、もう一度これを呼んでもらう
        /// （勝手に再接続すると、対面で始めた先生が接続枠を使い続けてしまうため）。
        /// </summary>
        public void Issue()
        {
            // 再発行。ルームを出るとマスターサーバーへ戻り、OnConnectedToMaster で新しいルームを作る。
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }

            // 接続中・切断中に呼ばれた場合、Photon はこの要求を断るだけで何も起きない。
            // 接続中なら進行中の発行がそのまま続き、切断中ならもう一度押してもらう。
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// 発行済みのルームごと接続を手放し、切断が終わるまで待つ。ID を発行したあとで対面レッスンを選んだとき、
        /// ルームに入ったまま同時接続数の枠を使い続けないようにするため。
        /// 切断は非同期で、待たずにシーンを移ると切断の通知がレッスン画面に届き
        /// 「接続が切れました」と出てしまうので、終わってから移ってもらう。
        /// </summary>
        public UniTask AbandonAsync()
        {
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                return UniTask.CompletedTask;
            }

            _abandonment = new UniTaskCompletionSource();

            // 前回のレッスンから戻ってきたときの切断がまだ終わっていなければ、Photon はこの要求を断る。
            // その場合も、進行中の切断が終われば OnDisconnected が呼ばれる。
            PhotonNetwork.Disconnect();
            return _abandonment.Task;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _abandonment?.TrySetResult();

            // Start や AbandonAsync で接続を手放した結果。エラーではない。
            if (cause == DisconnectCause.DisconnectByClientLogic)
            {
                return;
            }

            _onConnectionError.OnNext(cause.ToString());
        }

        public override void OnConnectedToMaster()
        {
            CreateRoom();
        }

        public override void OnCreatedRoom()
        {
            var props = new Hashtable { { "isNewTeacher", true } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            _onRoomCreated.OnNext(_pendingRoomId);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            CreateRoom();
        }

        private void CreateRoom()
        {
            _pendingRoomId = RoomId.Generate();

            // レッスンは先生1人＋生徒1人の前提。運用ルールではなく Photon 自体に上限を守らせる。
            var options = new RoomOptions { MaxPlayers = LessonRules.MaxParticipants };
            PhotonNetwork.CreateRoom(_pendingRoomId.Value, options);
        }
    }
}
