using UnityEngine;
using UnityEngine.UI;
using AsseScripts.UI.Piano;
using Assets.Scripts.UI.Melody;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// STOPボタン: 再生中メロディ/メトロノームのコルーチンとサウンドのみを停止（色は一切変更しない）
    /// </summary>
    public sealed class StopManager : MonoBehaviour
    {
        [SerializeField] private Button _stopButton;
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
        }
    }
}
