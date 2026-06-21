using Assets.Scripts;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
using AsseScripts.Domain;
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
            if (PlaySideManager.Instance == null) return;

            PlaySideManager.Instance.OnStateChanged
                .Subscribe(SendSoundPlayState)
                .AddTo(this);

            MelodyManager.Instance.MelodyChanged
                .Subscribe(SendMelodySelection)
                .AddTo(this);

            if (SoundSourceSwitcher.Instance != null)
            {
                SoundSourceSwitcher.Instance.OnChanged
                    .Subscribe(SendSoundSetSelection)
                    .AddTo(this);
            }
        }

        /// <summary>
        /// 後から入室した生徒に、先生の現在の状態（選択メロディ・音再生状態）を再送する。
        /// </summary>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!AppMode.IsTeacher) return;

            var melody = MelodyManager.Instance.CurrentMelody;
            if (melody != null)
            {
                _photonView.RPC("SelectMelodyReciver", newPlayer, MelodyJsonLoader.SerializeMelody(melody));
            }

            if (PlaySideManager.Instance != null)
            {
                // 送信側で反転（先生がfalseならば生徒はtrue）
                _photonView.RPC("SoundPlayStateReciver", newPlayer, !PlaySideManager.Instance.IsSoundPlay);
            }

            if (SoundSourceSwitcher.Instance != null)
            {
                _photonView.RPC("SelectSoundSetReciver", newPlayer, SoundSourceSwitcher.Instance.CurrentIndex);
            }
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
            PianoController.Instance.PressKeyFromRemote(new PianoNote((PianoNoteEnum)index));
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
            PianoController.Instance.ReleaseKeyFromRemote(new PianoNote((PianoNoteEnum)index));
        }

        [PunRPC]
        private void SoundPlayStateReciver(bool isSoundPlay)
        {
            PlaySideManager.Instance.ApplyRemoteSoundPlayState(isSoundPlay);
        }
    }
}
