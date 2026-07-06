using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.UI.Piano;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.Infrastructure;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// STOPボタンのUI。押下で再生中メロディ/メトロノームの停止を MelodyPlayer に委譲し、
    /// 先生は停止を生徒へ送信する（色は一切変更しない）。先生シーンにのみ存在する。
    /// </summary>
    public sealed class StopUI : MonoBehaviour
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
            player.FinishMelody();
            _network.SendStop();
        }
    }
}
