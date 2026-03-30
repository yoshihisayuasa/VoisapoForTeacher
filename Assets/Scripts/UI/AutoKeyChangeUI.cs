using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 自動キー変更 UI（上/下ボタン）
    /// </summary>
    public class AutoKeyChangeUI : MonoBehaviour
    {
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Image _upImage;
        [SerializeField] private Image _downImage;

        private Color _normalColor = Color.white;
        private Color _activeColor = AppColors.Accent;

        void Awake()
        {
            _upButton.onClick.AddListener(OnUpClicked);
            _downButton.onClick.AddListener(OnDownClicked);
        }

        void OnEnable() 
        {
            AutoKeyChangeManager.Instance.OnStateChanged += HandleStateChanged;
            // 初期表示
            HandleStateChanged(AutoKeyChangeManager.Instance.State);
        }

        void OnDisable()
        {
            AutoKeyChangeManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void OnUpClicked()
        {
            AutoKeyChangeManager.Instance.Toggle(AutoKeyChangeManager.AutoKeyChangeState.Up);
        }

        private void OnDownClicked()
        {
            AutoKeyChangeManager.Instance.Toggle(AutoKeyChangeManager.AutoKeyChangeState.Down);
        }

        private void HandleStateChanged(AutoKeyChangeManager.AutoKeyChangeState state)
        {
            _upImage.color = state == AutoKeyChangeManager.AutoKeyChangeState.Up ? _activeColor : _normalColor;
            _downImage.color = state == AutoKeyChangeManager.AutoKeyChangeState.Down ? _activeColor : _normalColor;
        }
    }
}