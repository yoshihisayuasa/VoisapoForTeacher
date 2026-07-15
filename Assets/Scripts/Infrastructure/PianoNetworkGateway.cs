using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using R3;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 鍵盤入力・先生状態のPhoton通信を1箇所に集約するゲートウェイ（インフラ層）。
    /// 送信メソッドと受信ストリーム（ドメイン型）を公開するだけで、
    /// 受信が誰にどう影響するかは知らない（采配は UI 層の NetworkEventRouter が行う）。
    /// 送信可否の判定はここで行う。先生の「意図」（状態・設定）は先生のみ、
    /// 演奏の「事実」（周キー・バトン）は音源側の端末（どちらの役割でも）が入室中に送る。
    ///
    /// 通信は RaiseEvent（イベントコード方式）を使う。PhotonView / ViewID には依存しないため、
    /// 先生・生徒シーン間で ViewID を一致させる必要がなく、エディタの ID 振り直しの影響を受けない。
    ///
    /// ルームは先生1人＋生徒1人の前提（PhotonRoomCreator が MaxPlayers=2 で強制）。
    /// そのため宛先は常に「自分以外の全員」で足り、個別指定の送信は持たない。
    /// </summary>
    public sealed class PianoNetworkGateway : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        /// <summary>
        /// 先生・生徒ビルド間の通信プロトコル（イベントコード）。
        /// Photon の予約領域（200以降）を避け、1〜199 の範囲で定義する。
        /// </summary>
        private static class EventCode
        {
            public const byte KeyDown = 1;
            public const byte KeyUp = 2;
            public const byte MelodySelection = 3;
            public const byte Bpm = 4;
            public const byte Stop = 5;
            public const byte SoundSetSelection = 6;
            public const byte SoundPlayState = 7;
            public const byte AutoKeyChange = 8;
            public const byte LoopKey = 9;
            public const byte PlaybackBaton = 10;
        }

        private readonly Subject<PianoNote> _keyDownReceived = new();
        private readonly Subject<PianoNote> _keyUpReceived = new();
        private readonly Subject<Melody> _melodyReceived = new();
        private readonly Subject<int> _bpmReceived = new();
        private readonly Subject<Unit> _stopReceived = new();
        private readonly Subject<int> _soundSetReceived = new();
        private readonly Subject<bool> _soundPlayStateReceived = new();
        private readonly Subject<AutoKeyChangeState> _autoKeyChangeReceived = new();
        private readonly Subject<PianoNote> _loopKeyReceived = new();
        private readonly Subject<PianoNote> _playbackBatonReceived = new();
        private readonly Subject<Unit> _studentJoined = new();
        private readonly Subject<bool> _studentPresenceChanged = new();
        private readonly Subject<Unit> _selfLeftRoom = new();

        public Observable<PianoNote> KeyDownReceived => _keyDownReceived;
        public Observable<PianoNote> KeyUpReceived => _keyUpReceived;
        public Observable<Melody> MelodyReceived => _melodyReceived;
        public Observable<int> BpmReceived => _bpmReceived;
        public Observable<Unit> StopReceived => _stopReceived;
        public Observable<int> SoundSetReceived => _soundSetReceived;
        public Observable<bool> SoundPlayStateReceived => _soundPlayStateReceived;
        public Observable<AutoKeyChangeState> AutoKeyChangeReceived => _autoKeyChangeReceived;

        /// <summary>音源側（トークン保持者）がループ各周の開始時に演奏したキー。描画専用。</summary>
        public Observable<PianoNote> LoopKeyReceived => _loopKeyReceived;

        /// <summary>再生権限の委譲（バトン）。受け取った側が次の音源側としてこのキーから再生を引き継ぐ。</summary>
        public Observable<PianoNote> PlaybackBatonReceived => _playbackBatonReceived;

        /// <summary>生徒が後から入室した（状態の再送が必要になった）ことを通知する。</summary>
        public Observable<Unit> StudentJoined => _studentJoined;

        /// <summary>生徒の在室状態の変化（入室で true、退室時は残員から判定）。</summary>
        public Observable<bool> StudentPresenceChanged => _studentPresenceChanged;

        /// <summary>自分がルームから出た（自発ログアウト・回線切断の両方）。ルーム由来の状態が無効になったことを意味する。</summary>
        public Observable<Unit> SelfLeftRoom => _selfLeftRoom;

        /// <summary>
        /// 自分（先生）以外のプレイヤー（生徒）が同じルームに居るか。
        /// </summary>
        public bool HasConnectedStudent =>
            PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1;

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!AppMode.IsTeacher) return;

            _studentPresenceChanged.OnNext(true);
            _studentJoined.OnNext(Unit.Default);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!AppMode.IsTeacher) return;

            _studentPresenceChanged.OnNext(HasConnectedStudent);
        }

        public override void OnLeftRoom()
        {
            _selfLeftRoom.OnNext(Unit.Default);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _selfLeftRoom.OnNext(Unit.Default);
        }

        // ── 送信（先生のみ） ──

        public void SendMelodySelection(Melody melody)
        {
            if (!CanSend() || melody == null) return;
            RaiseToOthers(EventCode.MelodySelection, MelodyNetworkSerializer.Serialize(melody));
        }

        public void SendBpm(int bpm)
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.Bpm, bpm);
        }

        public void SendStop()
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.Stop, null);
        }

        public void SendSoundSetSelection(int index)
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.SoundSetSelection, index);
        }

        public void SendKeyDown(PianoNote note)
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.KeyDown, note.Index);
        }

        public void SendKeyUp(PianoNote note)
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.KeyUp, note.Index);
        }

        public void SendSoundPlayState(bool isSoundPlay)
        {
            if (!CanSend()) return;
            // 先生がfalseならば生徒はtrue、先生がtrueならば生徒はfalse（反転して送信）
            RaiseToOthers(EventCode.SoundPlayState, !isSoundPlay);
        }

        public void SendAutoKeyChange(AutoKeyChangeState state)
        {
            if (!CanSend()) return;
            RaiseToOthers(EventCode.AutoKeyChange, (int)state);
        }

        // ── 送信（音源側＝トークン保持者。演奏の「事実」なので先生・生徒どちらからも送る） ──
        // エコー防止は役割ではなく由来で行う：これらはローカル発イベントの購読からのみ呼ばれ、
        // 受信は別ストリームに流れるため再送信は起きない。

        public void SendLoopKey(PianoNote note)
        {
            if (!PhotonNetwork.InRoom) return;
            RaiseToOthers(EventCode.LoopKey, note.Index);
        }

        public void SendPlaybackBaton(PianoNote note)
        {
            if (!PhotonNetwork.InRoom) return;
            RaiseToOthers(EventCode.PlaybackBaton, note.Index);
        }

        private static bool CanSend()
        {
            return AppMode.IsTeacher && PhotonNetwork.InRoom;
        }

        private static void RaiseToOthers(byte code, object payload)
        {
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(code, payload, options, SendOptions.SendReliable);
        }

        // ── 受信（イベントをドメイン型のストリームへ流すだけ） ──
        // MonoBehaviourPunCallbacks の OnEnable が AddCallbackTarget を呼ぶため、
        // IOnEventCallback を実装するだけで OnEvent が届く。

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case EventCode.KeyDown:
                    _keyDownReceived.OnNext(ToNote(photonEvent.CustomData));
                    break;

                case EventCode.KeyUp:
                    _keyUpReceived.OnNext(ToNote(photonEvent.CustomData));
                    break;

                case EventCode.MelodySelection:
                    var melody = MelodyNetworkSerializer.Deserialize((string)photonEvent.CustomData);
                    if (melody != null)
                    {
                        _melodyReceived.OnNext(melody);
                    }
                    break;

                case EventCode.Bpm:
                    _bpmReceived.OnNext((int)photonEvent.CustomData);
                    break;

                case EventCode.Stop:
                    _stopReceived.OnNext(Unit.Default);
                    break;

                case EventCode.SoundSetSelection:
                    _soundSetReceived.OnNext((int)photonEvent.CustomData);
                    break;

                case EventCode.SoundPlayState:
                    _soundPlayStateReceived.OnNext((bool)photonEvent.CustomData);
                    break;

                case EventCode.AutoKeyChange:
                    _autoKeyChangeReceived.OnNext((AutoKeyChangeState)(int)photonEvent.CustomData);
                    break;

                case EventCode.LoopKey:
                    _loopKeyReceived.OnNext(ToNote(photonEvent.CustomData));
                    break;

                case EventCode.PlaybackBaton:
                    _playbackBatonReceived.OnNext(ToNote(photonEvent.CustomData));
                    break;

                // Photon 内部イベント（コード200以降）等は対象外なので無視する。
                default:
                    break;
            }
        }

        private static PianoNote ToNote(object payload)
        {
            return new PianoNote((PianoNoteEnum)(int)payload);
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
            _autoKeyChangeReceived.Dispose();
            _loopKeyReceived.Dispose();
            _playbackBatonReceived.Dispose();
            _studentJoined.Dispose();
            _studentPresenceChanged.Dispose();
            _selfLeftRoom.Dispose();
        }
    }
}
