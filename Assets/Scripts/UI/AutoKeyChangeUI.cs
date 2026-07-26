using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 自動キー変更 UI（上/下ボタン）。プレミアム限定のため、クリックは PremiumGateButton が受け取る。
    /// </summary>
    public sealed class AutoKeyChangeUI : MonoBehaviour
    {
        [SerializeField] private PremiumGateButton _upGate;
        [SerializeField] private PremiumGateButton _downGate;
        [SerializeField] private Image _upImage;
        [SerializeField] private Image _downImage;

        void Awake()
        {
            _upGate.Bind(new FeatureGate(PremiumFeature.AutoKeyChange), OnUpClicked);
            _downGate.Bind(new FeatureGate(PremiumFeature.AutoKeyChange), OnDownClicked);
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
            _upImage.color = AppColors.ActiveOrWhite(state == AutoKeyChangeState.Up);
            _downImage.color = AppColors.ActiveOrWhite(state == AutoKeyChangeState.Down);
        }
    }
}
