using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI.Modal
{
    /// <summary>
    /// プレミアム機能の購入案内モーダル。ロックされた操作をしたときに開く。
    /// 月額・年額それぞれの購入ボタンを直接押して購入する。
    /// 価格はストアから取得して表示する（ハードコードしない）。
    /// 自動更新される旨・解約方法・トライアル条件の文言はプレハブ側のテキストが持つ（Apple 審査要件）。
    /// </summary>
    public sealed class PremiumPurchaseModalUI : MonoBehaviour
    {
        private const string ResourcePath =
            "Prefab/UnityScreenNavigator/Modal/pfb_ui_modal_premium_purchase";

        [SerializeField] private TMP_Text _monthlyPriceText;
        [SerializeField] private TMP_Text _yearlyPriceText;
        [SerializeField] private Button _monthlyPurchaseButton;
        [SerializeField] private Button _yearlyPurchaseButton;
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Button _closeButton;

        public static void Show()
        {
            ModalContainer.Find("ModalContainer").Push(ResourcePath, true);
        }

        private void Start()
        {
            _monthlyPurchaseButton.onClick.AddListener(() => PurchaseAsync(SubscriptionPlan.Monthly).Forget());
            _yearlyPurchaseButton.onClick.AddListener(() => PurchaseAsync(SubscriptionPlan.Yearly).Forget());
            _restoreButton.onClick.AddListener(() => RestoreAsync().Forget());
            _closeButton.onClick.AddListener(Close);

            ShowPricesAsync().Forget();
        }

        private async UniTaskVoid ShowPricesAsync()
        {
            var prices = await EntitlementManager.Instance.GetPlanPricesAsync();

            _monthlyPriceText.text = prices.PriceOf(SubscriptionPlan.Monthly);
            _yearlyPriceText.text = prices.PriceOf(SubscriptionPlan.Yearly);
        }

        private async UniTaskVoid PurchaseAsync(SubscriptionPlan plan)
        {
            SetBusy(true);
            await EntitlementManager.Instance.PurchaseAsync(plan);
            SetBusy(false);
            CloseWhenPremium();
        }

        private async UniTaskVoid RestoreAsync()
        {
            SetBusy(true);
            await EntitlementManager.Instance.RestoreAsync();
            SetBusy(false);
            CloseWhenPremium();
        }

        /// <summary>ストアのダイアログ中に購入・復元を重ねて起動させない。</summary>
        private void SetBusy(bool busy)
        {
            _monthlyPurchaseButton.interactable = !busy;
            _yearlyPurchaseButton.interactable = !busy;
            _restoreButton.interactable = !busy;
        }

        /// <summary>購読が有効になったときだけ閉じる。失敗やキャンセルなら案内を出したままにする。</summary>
        private void CloseWhenPremium()
        {
            EntitlementManager.Instance.Current.CurrentValue.Describe(
                onFree: () => { },
                onPremium: _ => Close());
        }

        private void Close()
        {
            ModalContainer.Of(transform).Pop(true);
        }
    }
}
