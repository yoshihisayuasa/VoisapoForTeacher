using Assets.Scripts.Domain.ValueObjects;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 自動キー変更 UI（上/下ボタン）
    /// </summary>
    public sealed class AutoKeyChangeUI : MonoBehaviour
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
            AutoKeyChangeManager.Instance.State.Subscribe(HandleStateChanged).AddTo(this);
        }

        private void OnUpClicked()
        {
            AutoKeyChangeManager.Instance.Toggle(AutoKeyChangeState.Up);
        }

        private void OnDownClicked()
        {
            AutoKeyChangeManager.Instance.Toggle(AutoKeyChangeState.Down);
        }

        private void HandleStateChanged(AutoKeyChangeState state)
        {
            _upImage.color = state == AutoKeyChangeState.Up ? _activeColor : _normalColor;
            _downImage.color = state == AutoKeyChangeState.Down ? _activeColor : _normalColor;
        }
    }
}
