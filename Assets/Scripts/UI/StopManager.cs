using UnityEngine;
using UnityEngine.UI;
using AsseScripts.UI.Piano;
using Assets.Scripts.UI.MelodyUI;
using AsseScripts.Infrastructure;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// STOPボタン: 再生中メロディ/メトロノームのコルーチンとサウンドのみを停止（色は一切変更しない）
    /// </summary>
    public sealed class StopManager : MonoBehaviour
    {
        [SerializeField] private Button _stopButton;

        [SerializeField]
        [Tooltip("停止を生徒へ送るPhotonゲートウェイ。先生シーンで割り当てる")]
        private PianoNetworkGateway _network;

        private void OnEnable()
        {
            if (_stopButton != null)
            {
                _stopButton.onClick.AddListener(OnClickStop);
            }
        }

        private void OnDisable()
        {
            if(_stopButton != null)
            {
                _stopButton.onClick.RemoveListener(OnClickStop);
            }
        }

        private void OnClickStop()
        {
            var player = MelodyPlayer.Instance;
            if (player == null)
            {
                return;
            }
            player.StopMelody(false, shouldDelayRecordStop: true);

            // 先生のみ送信。生徒側は受信してローカル停止する（送信は権限で弾かれる）。
            _network.SendStop();
        }
    }
}
