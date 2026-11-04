using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// イヤホンモードボタンUI制御
    /// 状態は <see cref="EarphoneModeManager"/> が保持し、ここはそれを駆動・表示するだけ。
    /// 生徒が入室する前から押せる。対面レッスンでは OnlineLessonFeature で薄く表示し、押せなくする。
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

            UpdateVisual();
        }

        public void OnButtonClicked()
        {
            EarphoneModeManager.Instance.Toggle();
        }

        private void UpdateVisual()
        {
            _targetImage.color = AppColors.ActiveOrWhite(EarphoneModeManager.Instance.EarphoneMode);
        }
    }
}
