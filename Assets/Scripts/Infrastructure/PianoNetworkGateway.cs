using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using AsseScripts.Domain;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace AsseScripts.Infrastructure
{
    /// <summary>
    /// 送信先の生徒を表す不透明トークン。Photon の Player 型を UI 層へ漏らさないための包み。
    /// </summary>
    public readonly struct RemotePeer
    {
        internal Player Player { get; }

        internal RemotePeer(Player player)
        {
            Player = player;
        }
    }

    /// <summary>
    /// 鍵盤入力・先生状態のPhoton通信を1箇所に集約するゲートウェイ（インフラ層）。
    /// 送信メソッドと受信ストリーム（ドメイン型）を公開するだけで、
    /// 受信が誰にどう影響するかは知らない（采配は UI 層の NetworkEventRouter が行う）。
    /// 送信可否（先生のみ・入室中のみ）の判定はここで行う。
    /// </summary>
    public sealed class PianoNetworkGateway : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        [Tooltip("このGameObjectに付与したPhotonView")] private PhotonView _photonView;

        private readonly Subject<PianoNote> _keyDownReceived = new();
        private readonly Subject<PianoNote> _keyUpReceived = new();
        private readonly Subject<Melody> _melodyReceived = new();
        private readonly Subject<int> _bpmReceived = new();
        private readonly Subject<Unit> _stopReceived = new();
        private readonly Subject<int> _soundSetReceived = new();
        private readonly Subject<bool> _soundPlayStateReceived = new();
        private readonly Subject<RemotePeer> _studentJoined = new();
        private readonly Subject<bool> _studentPresenceChanged = new();

        public Observable<PianoNote> KeyDownReceived => _keyDownReceived;
        public Observable<PianoNote> KeyUpReceived => _keyUpReceived;
        public Observable<Melody> MelodyReceived => _melodyReceived;
        public Observable<int> BpmReceived => _bpmReceived;
        public Observable<Unit> StopReceived => _stopReceived;
        public Observable<int> SoundSetReceived => _soundSetReceived;
        public Observable<bool> SoundPlayStateReceived => _soundPlayStateReceived;

        /// <summary>後から入室した生徒。状態の再送先として通知する。</summary>
        public Observable<RemotePeer> StudentJoined => _studentJoined;

        /// <summary>生徒の在室状態の変化（入室で true、退室時は残員から判定）。</summary>
        public Observable<bool> StudentPresenceChanged => _studentPresenceChanged;

        /// <summary>
        /// 自分（先生）以外のプレイヤー（生徒）が同じルームに居るか。
        /// </summary>
        public bool HasConnectedStudent =>
            PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1;

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!AppMode.IsTeacher) return;

            _studentPresenceChanged.OnNext(true);
            _studentJoined.OnNext(new RemotePeer(newPlayer));
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!AppMode.IsTeacher) return;

            _studentPresenceChanged.OnNext(HasConnectedStudent);
        }

        // ── 送信（先生のみ） ──

        public void SendMelodySelection(Melody melody)
        {
            if (!CanSend() || melody == null) return;
            _photonView.RPC("SelectMelodyReciver", RpcTarget.Others, MelodyJsonLoader.SerializeMelody(melody));
        }

        public void SendMelodySelectionTo(RemotePeer peer, Melody melody)
        {
            if (!CanSend() || melody == null) return;
            _photonView.RPC("SelectMelodyReciver", peer.Player, MelodyJsonLoader.SerializeMelody(melody));
        }

        public void SendBpm(int bpm)
        {
            if (!CanSend()) return;
            _photonView.RPC("BpmReciver", RpcTarget.Others, bpm);
        }

        public void SendBpmTo(RemotePeer peer, int bpm)
        {
            if (!CanSend()) return;
            _photonView.RPC("BpmReciver", peer.Player, bpm);
        }

        public void SendStop()
        {
            if (!CanSend()) return;
            _photonView.RPC("StopMelodyReciver", RpcTarget.Others);
        }

        public void SendSoundSetSelection(int index)
        {
            if (!CanSend()) return;
            _photonView.RPC("SelectSoundSetReciver", RpcTarget.Others, index);
        }

        public void SendSoundSetSelectionTo(RemotePeer peer, int index)
        {
            if (!CanSend()) return;
            _photonView.RPC("SelectSoundSetReciver", peer.Player, index);
        }

        public void SendKeyDown(PianoNote note)
        {
            if (!CanSend()) return;
            _photonView.RPC("PlayKeyReciver", RpcTarget.Others, note.Index);
        }

        public void SendKeyUp(PianoNote note)
        {
            if (!CanSend()) return;
            _photonView.RPC("StopKeyReciver", RpcTarget.Others, note.Index);
        }

        public void SendSoundPlayState(bool isSoundPlay)
        {
            if (!CanSend()) return;
            // 先生がfalseならば生徒はtrue、先生がtrueならば生徒はfalse（反転して送信）
            _photonView.RPC("SoundPlayStateReciver", RpcTarget.Others, !isSoundPlay);
        }

        public void SendSoundPlayStateTo(RemotePeer peer, bool isSoundPlay)
        {
            if (!CanSend()) return;
            // 送信側で反転（先生がfalseならば生徒はtrue）
            _photonView.RPC("SoundPlayStateReciver", peer.Player, !isSoundPlay);
        }

        private static bool CanSend()
        {
            return AppMode.IsTeacher && PhotonNetwork.InRoom;
        }

        // ── 受信（RPC をドメイン型のストリームへ流すだけ） ──

        [PunRPC]
        private void SelectMelodyReciver(string json)
        {
            var melody = MelodyJsonLoader.DeserializeMelody(json);
            if (melody != null)
            {
                _melodyReceived.OnNext(melody);
            }
        }

        [PunRPC]
        private void BpmReciver(int bpm)
        {
            _bpmReceived.OnNext(bpm);
        }

        [PunRPC]
        private void StopMelodyReciver()
        {
            _stopReceived.OnNext(Unit.Default);
        }

        [PunRPC]
        private void SelectSoundSetReciver(int index)
        {
            _soundSetReceived.OnNext(index);
        }

        [PunRPC]
        private void PlayKeyReciver(int index)
        {
            _keyDownReceived.OnNext(new PianoNote((PianoNoteEnum)index));
        }

        [PunRPC]
        private void StopKeyReciver(int index)
        {
            _keyUpReceived.OnNext(new PianoNote((PianoNoteEnum)index));
        }

        [PunRPC]
        private void SoundPlayStateReciver(bool isSoundPlay)
        {
            _soundPlayStateReceived.OnNext(isSoundPlay);
        }

        private void OnDestroy()
        {
            _keyDownReceived.Dispose();
            _keyUpReceived.Dispose();
            _melodyReceived.Dispose();
            _bpmReceived.Dispose();
            _stopReceived.Dispose();
            _soundSetReceived.Dispose();
            _soundPlayStateReceived.Dispose();
            _studentJoined.Dispose();
            _studentPresenceChanged.Dispose();
        }
    }
}
