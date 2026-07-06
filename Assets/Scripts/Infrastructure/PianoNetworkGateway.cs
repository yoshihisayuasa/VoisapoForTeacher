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
    /// 送信可否（先生のみ・入室中のみ）の判定はここで行う。
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
        }

        private readonly Subject<PianoNote> _keyDownReceived = new();
        private readonly Subject<PianoNote> _keyUpReceived = new();
        private readonly Subject<Melody> _melodyReceived = new();
        private readonly Subject<int> _bpmReceived = new();
        private readonly Subject<Unit> _stopReceived = new();
        private readonly Subject<int> _soundSetReceived = new();
        private readonly Subject<bool> _soundPlayStateReceived = new();
        private readonly Subject<Unit> _studentJoined = new();
        private readonly Subject<bool> _studentPresenceChanged = new();

        public Observable<PianoNote> KeyDownReceived => _keyDownReceived;
        public Observable<PianoNote> KeyUpReceived => _keyUpReceived;
        public Observable<Melody> MelodyReceived => _melodyReceived;
        public Observable<int> BpmReceived => _bpmReceived;
        public Observable<Unit> StopReceived => _stopReceived;
        public Observable<int> SoundSetReceived => _soundSetReceived;
        public Observable<bool> SoundPlayStateReceived => _soundPlayStateReceived;

        /// <summary>生徒が後から入室した（状態の再送が必要になった）ことを通知する。</summary>
        public Observable<Unit> StudentJoined => _studentJoined;

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
            _studentJoined.OnNext(Unit.Default);
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
            RaiseToOthers(EventCode.MelodySelection, MelodyJsonLoader.SerializeMelody(melody));
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
                    var melody = MelodyJsonLoader.DeserializeMelody((string)photonEvent.CustomData);
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
            _studentJoined.Dispose();
            _studentPresenceChanged.Dispose();
        }
    }
}
