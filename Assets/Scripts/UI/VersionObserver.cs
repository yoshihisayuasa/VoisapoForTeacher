using AsseScripts.Domain;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 接続相手（先生⇔生徒）のアプリバージョンを確認し、必要バージョン未満なら通知する。
    /// 役割に依存せず「相手プレイヤー」を見るだけなので、先生・生徒どちらのシーンにも配置できる。
    /// 各クライアントは自分のバージョンを <see cref="VersionKey"/> で公開する。
    /// </summary>
    public sealed class VersionObserver : MonoBehaviourPunCallbacks
    {
        /// <summary>各クライアントが自分のアプリバージョンを公開するプロパティキー。</summary>
        public const string VersionKey = "appVersion";

        [SerializeField]
        [Tooltip("これ未満の相手バージョンなら警告する")]
        private string _minRequiredVersion = "1.0.0";

        private AppVersion _minRequired;

        private readonly Subject<bool> _onPeerVersionOutdated = new();
        public Observable<bool> OnPeerVersionOutdated => _onPeerVersionOutdated;

        private void Awake()
        {
            if (!AppVersion.TryCreate(_minRequiredVersion, out _minRequired))
            {
                Debug.LogError($"無効な必要バージョン: {_minRequiredVersion}");
                _minRequired = new AppVersion("1.0.0");
            }
        }

        private void OnDestroy()
        {
            _onPeerVersionOutdated.Dispose();
        }

        // 入室時：既に部屋にいる相手（生徒から見た先生など）を確認する。
        public override void OnJoinedRoom()
        {
            foreach (var player in PhotonNetwork.PlayerListOthers)
            {
                _onPeerVersionOutdated.OnNext(IsOutdated(player));
            }
        }

        // 後から入室してきた相手（先生から見た生徒など）を確認する。
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _onPeerVersionOutdated.OnNext(IsOutdated(newPlayer));
        }

        // 相手が退室したら警告状態を解除する。
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _onPeerVersionOutdated.OnNext(false);
        }

        private bool IsOutdated(Player player)
        {
            if (!player.CustomProperties.TryGetValue(VersionKey, out var raw))
                return true;
            if (!AppVersion.TryCreate(raw.ToString(), out var peerVersion))
                return true;
            return _minRequired.IsNewerThan(peerVersion);
        }
    }
}
