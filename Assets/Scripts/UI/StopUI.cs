using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.Infrastructure;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// STOPボタンのUI。押下（またはスペースキー）で再生中メロディ/メトロノームの停止を
    /// MelodyPlayer に委譲し、先生は停止を生徒へ送信する（色は一切変更しない）。
    /// 先生シーンにのみ存在するため、生徒向けのガードは持たない。
    /// </summary>
    public sealed class StopUI : MonoBehaviour
    {
        [SerializeField] private Button _stopButton;

        [SerializeField]
        [Tooltip("停止を生徒へ送るPhotonゲートウェイ。先生シーンで割り当てる")]
        private PianoNetworkGateway _network;

        private void OnEnable()
        {
            _stopButton.onClick.AddListener(Stop);
        }

        private void OnDisable()
        {
            _stopButton.onClick.RemoveListener(Stop);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame)
            {
                Stop();
            }
        }

        private void Stop()
        {
            MelodyPlayer.Instance.FinishMelody();
            _network.SendStop();
        }
    }
}
