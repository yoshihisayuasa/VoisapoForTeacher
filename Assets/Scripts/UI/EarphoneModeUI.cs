using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AsseScripts.EarphoneModeControll
{
    /// <summary>
    /// イヤホンモードボタンUI制御
    /// 他のクラスにボタンのオンオフ状態を通知する
    /// </summary>
    public class EarphoneModeUI : MonoBehaviour
    {
        [SerializeField] private Button _earphoneModeButton;
        [SerializeField] private Image _targetImage;                // 色を変える対象（必ず割り当てる）
        private Color _normalColor = Color.white;  // 2回目で戻す色
        private Color _activeColor = Color.red;

        private bool _isEarphoneModeOn = false;

        void Start()
        {
            if (EarphoneModeManager.Instance != null)
            {
                _isEarphoneModeOn = EarphoneModeManager.Instance.EarphoneMode;
            }
            _earphoneModeButton.onClick.AddListener(OnButtonClicked);
            UpdateVisual();

        }

        // ボタンがクリックされたときにオンオフを切り替えて通知
        public void OnButtonClicked()
        {
            _isEarphoneModeOn = !_isEarphoneModeOn;
            // Infra へ伝達（UI -> Infra のみ）
            if (EarphoneModeManager.Instance != null)
            {
                EarphoneModeManager.Instance.SetMode(_isEarphoneModeOn);
            }
            UpdateVisual();
        }
        private void UpdateVisual()
        {
            _targetImage.color = _isEarphoneModeOn ? _activeColor : _normalColor;
        }
    }
}
