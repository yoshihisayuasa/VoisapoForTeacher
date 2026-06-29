using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
using AsseScripts.Domain;
using AsseScripts.UI;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace AsseScripts.Infrastructure
{
    /// <summary>
    /// 鍵盤入力・先生状態のPhoton通信を1箇所に集約するゲートウェイ（インフラ層）。
    /// 送信は先生ビルドのみ。生徒ビルドは受信して各マネージャへ委譲する。
    /// </summary>
    public sealed class PianoNetworkGateway : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        [Tooltip("このGameObjectに付与したPhotonView")] private PhotonView _photonView;

        private void Start()
        {
            if (!AppMode.IsTeacher) return;

            SoundPlayManager.Instance.OnStateChanged
                .Subscribe(SendSoundPlayState)
                .AddTo(this);

            MelodyManager.Instance.MelodyChanged
                .Subscribe(SendMelodySelection)
                .AddTo(this);

            BPMManager.Instance.BpmChanged
                .Subscribe(SendBpm)
                .AddTo(this);

            if (SoundSourceSwitcher.Instance != null)
            {
                SoundSourceSwitcher.Instance.OnChanged
                    .Subscribe(SendSoundSetSelection)
                    .AddTo(this);
            }

            // シーン再生成時、既に生徒が居れば接続ありとして初期化する。
            SoundPlayManager.Instance.SetStudentConnected(HasConnectedStudent());
        }

        /// <summary>
        /// 自分（先生）以外のプレイヤー（生徒）が同じルームに居るか。
        /// </summary>
        private static bool HasConnectedStudent()
        {
            return PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1;
        }

        /// <summary>
        /// 後から入室した生徒に、先生の現在の状態（選択メロディ・音再生状態）を再送する。
        /// </summary>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!AppMode.IsTeacher) return;

            // 生徒ありに切り替える。これで実効状態が先生の意図（トグル）に戻る。
            SoundPlayManager.Instance.SetStudentConnected(true);

            var melody = MelodyManager.Instance.CurrentMelody;
            if (melody != null)
            {
                _photonView.RPC("SelectMelodyReciver", newPlayer, MelodyJsonLoader.SerializeMelody(melody));
            }

            // 送信側で反転（先生がfalseならば生徒はtrue）
            _photonView.RPC("SoundPlayStateReciver", newPlayer, !SoundPlayManager.Instance.IsSoundPlay);

            _photonView.RPC("BpmReciver", newPlayer, BPMManager.Instance.Value);

            if (SoundSourceSwitcher.Instance != null)
            {
                _photonView.RPC("SelectSoundSetReciver", newPlayer, SoundSourceSwitcher.Instance.CurrentIndex);
            }
        }

        /// <summary>
        /// 生徒が退室したとき、相手が居なくなれば先生自身が鳴らすよう実効状態を更新する。
        /// </summary>
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!AppMode.IsTeacher) return;

            SoundPlayManager.Instance.SetStudentConnected(HasConnectedStudent());
        }

        public void SendMelodySelection(Melody melody)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            if (melody == null) return;
            _photonView.RPC("SelectMelodyReciver", RpcTarget.Others, MelodyJsonLoader.SerializeMelody(melody));
        }

        [PunRPC]
        private void SelectMelodyReciver(string json)
        {
            var melody = MelodyJsonLoader.DeserializeMelody(json);
            if (melody != null)
            {
                MelodyManager.Instance.SetCurrentMelody(melody);
            }
        }

        public void SendBpm(int bpm)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            _photonView.RPC("BpmReciver", RpcTarget.Others, bpm);
        }

        [PunRPC]
        private void BpmReciver(int bpm)
        {
            BPMManager.Instance.SetValue(bpm);
        }

        public void SendStop()
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            _photonView.RPC("StopMelodyReciver", RpcTarget.Others);
        }

        [PunRPC]
        private void StopMelodyReciver()
        {
            MelodyPlayer.Instance.StopMelody(false, shouldDelayRecordStop: true);
        }

        public void SendSoundSetSelection(int index)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            _photonView.RPC("SelectSoundSetReciver", RpcTarget.Others, index);
        }

        [PunRPC]
        private void SelectSoundSetReciver(int index)
        {
            if (SoundSourceSwitcher.Instance != null)
            {
                SoundSourceSwitcher.Instance.SwitchTo(index);
            }
        }

        public void SendKeyDown(PianoNote note)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            _photonView.RPC("PlayKeyReciver", RpcTarget.Others, note.Index);
        }

        public void SendKeyUp(PianoNote note)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            _photonView.RPC("StopKeyReciver", RpcTarget.Others, note.Index);
        }

        [PunRPC]
        private void PlayKeyReciver(int index)
        {
            PianoController.Instance.PressKey(new PianoNote((PianoNoteEnum)index));
        }

        public void SendSoundPlayState(bool isSoundPlay)
        {
            if (!AppMode.IsTeacher) return;
            if (!PhotonNetwork.InRoom) return;
            // 先生がfalseならば生徒はtrue、先生がtrueならば生徒はfalse（反転して送信）
            _photonView.RPC("SoundPlayStateReciver", RpcTarget.Others, !isSoundPlay);
        }

        [PunRPC]
        private void StopKeyReciver(int index)
        {
            PianoController.Instance.ReleaseKey(new PianoNote((PianoNoteEnum)index));
        }

        [PunRPC]
        private void SoundPlayStateReciver(bool isSoundPlay)
        {
            SoundPlayManager.Instance.SetState(isSoundPlay);
        }
    }
}
