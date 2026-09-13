using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// イヤホンモードボタンUI制御
    /// 状態は <see cref="EarphoneModeManager"/> が保持し、ここはそれを駆動・表示するだけ。
    /// 生徒が入室していない間はグレーアウトして操作不可にする
    /// </summary>
    public sealed class EarphoneModeUI : MonoBehaviour
    {
        [SerializeField] private Button _earphoneModeButton;
        [SerializeField] private Image _targetImage;                // 色を変える対象（必ず割り当てる）

        void Start()
        {
            _earphoneModeButton.onClick.AddListener(OnButtonClicked);

            EarphoneModeManager.Instance.OnModeChanged
                .Subscribe(_ => UpdateVisual())
                .AddTo(this);

            EarphoneModeManager.Instance.CanToggle
                .Subscribe(SetInteractable)
                .AddTo(this);
        }

        public void OnButtonClicked()
        {
            EarphoneModeManager.Instance.Toggle();
        }

        private void SetInteractable(bool canToggle)
        {
            _earphoneModeButton.interactable = canToggle;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            _targetImage.color = _earphoneModeButton.interactable
                ? AppColors.ActiveOrWhite(EarphoneModeManager.Instance.EarphoneMode)
                : AppColors.Disabled;
        }
    }
}
