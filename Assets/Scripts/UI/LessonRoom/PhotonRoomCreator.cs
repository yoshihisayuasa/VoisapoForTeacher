using System;
using AsseScripts.Domain;
using Assets.Scripts.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI.LessonRoom
{
    public sealed class PhotonRoomCreator : MonoBehaviourPunCallbacks
    {
        private const string _gameVersion = "0.1";

        private RoomId _pendingRoomId;

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
                { VersionObserver.VersionKey, Application.version }
            });

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void OnDestroy()
        {
            _onRoomCreated.Dispose();
            _onConnectionError.Dispose();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (cause == DisconnectCause.DisconnectByClientLogic)
            {
                PhotonNetwork.ConnectUsingSettings();
                return;
            }

            _onConnectionError.OnNext(cause.ToString());

            Observable.Timer(TimeSpan.FromSeconds(3))
                .Subscribe(_ => PhotonNetwork.ConnectUsingSettings())
                .AddTo(this);
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
            PhotonNetwork.CreateRoom(_pendingRoomId.Value);
        }
    }
}
