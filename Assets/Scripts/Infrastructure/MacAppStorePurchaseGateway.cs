// Mac ビルドは Unity IAP（com.unity.purchasing）に依存する。
// パッケージが入っていないまま Mac 向けにビルドすると課金が丸ごと無効になってしまうため、
// 黙って Free を返さずビルドを止める。
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR && !UNITY_PURCHASING
#error Mac ビルドには In-App Purchasing パッケージ（com.unity.purchasing）が必要です。Package Manager から追加してください。
#endif

#if UNITY_PURCHASING
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// Mac App Store の自動更新サブスクリプション。Unity IAP のラッパー。
    /// 購読の有効期限はレシートの購読情報（SubscriptionManager）から取得する。
    /// </summary>
    public sealed class MacAppStorePurchaseGateway : IPurchaseGateway, IStoreListener
    {
        // App Store Connect で登録したサブスクリプションの Product ID に差し替えること。
        private const string MonthlyProductId = "getsugaku1900";
        private const string YearlyProductId = "nengaku_jpy14900";

        public string ManagementUrl => "macappstores://apps.apple.com/account/subscriptions";

        private IStoreController _controller;
        private IAppleExtensions _apple;

        private UniTaskCompletionSource<bool> _initialization;
        private UniTaskCompletionSource<EntitlementResult> _purchase;
        private UniTaskCompletionSource<EntitlementResult> _restoration;

        public async UniTask<EntitlementResult> FetchEntitlementAsync()
        {
            if (!await EnsureInitializedAsync())
            {
                return EntitlementResult.Unavailable();
            }
            return EntitlementResult.Fetched(CurrentEntitlement());
        }

        public async UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan)
        {
            if (!await EnsureInitializedAsync())
            {
                return EntitlementResult.Unavailable();
            }

            _purchase = new UniTaskCompletionSource<EntitlementResult>();
            _controller.InitiatePurchase(ProductIdOf(plan));
            return await _purchase.Task;
        }

        /// <summary>
        /// StoreKit の購入復元。Apple の審査要件のため、ユーザー操作から必ず呼べるようにしておく。
        /// </summary>
        public async UniTask<EntitlementResult> RestoreAsync()
        {
            if (!await EnsureInitializedAsync())
            {
                return EntitlementResult.Unavailable();
            }

            _restoration = new UniTaskCompletionSource<EntitlementResult>();
            _apple.RestoreTransactions(OnTransactionsRestored);
            return await _restoration.Task;
        }

        public async UniTask<PlanPrices> GetPlanPricesAsync()
        {
            if (!await EnsureInitializedAsync())
            {
                return PlanPrices.Unavailable;
            }

            return new PlanPrices(
                LocalizedPriceOf(MonthlyProductId),
                LocalizedPriceOf(YearlyProductId));
        }

        private async UniTask<bool> EnsureInitializedAsync()
        {
            if (_controller != null)
            {
                return true;
            }

            if (_initialization == null)
            {
                _initialization = new UniTaskCompletionSource<bool>();

                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                builder.AddProduct(MonthlyProductId, ProductType.Subscription);
                builder.AddProduct(YearlyProductId, ProductType.Subscription);
                UnityPurchasing.Initialize(this, builder);
            }

            return await _initialization.Task;
        }

        /// <summary>
        /// 月額／年額のうち最も遅い期限を採用する。プラン変更の直後は
        /// 両方のレシートが一時的に残りうるため。
        /// </summary>
        private Entitlement CurrentEntitlement()
        {
            var latest = DateTimeOffset.MinValue;

            foreach (var productId in new[] { MonthlyProductId, YearlyProductId })
            {
                var expiresAt = ExpirationOf(productId);
                if (expiresAt > latest)
                {
                    latest = expiresAt;
                }
            }

            return latest == DateTimeOffset.MinValue
                ? Entitlement.Free
                : Entitlement.PremiumUntil(latest);
        }

        private DateTimeOffset ExpirationOf(string productId)
        {
            var product = _controller.products.WithID(productId);
            if (product == null || !product.hasReceipt)
            {
                return DateTimeOffset.MinValue;
            }

            try
            {
                var info = new SubscriptionManager(product, null).getSubscriptionInfo();
                return new DateTimeOffset(info.getExpireDate().ToUniversalTime(), TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] 購読情報の解析に失敗（{productId}）: {ex.Message}");
                return DateTimeOffset.MinValue;
            }
        }

        private string LocalizedPriceOf(string productId)
        {
            var product = _controller.products.WithID(productId);
            return product == null ? "--" : product.metadata.localizedPriceString;
        }

        private static string ProductIdOf(SubscriptionPlan plan)
        {
            return plan == SubscriptionPlan.Monthly ? MonthlyProductId : YearlyProductId;
        }

        private void OnTransactionsRestored(bool success, string error)
        {
            if (!success)
            {
                Debug.LogWarning($"[Entitlement] 購入の復元に失敗: {error}");
                _restoration.TrySetResult(EntitlementResult.Unavailable());
                return;
            }
            _restoration.TrySetResult(EntitlementResult.Fetched(CurrentEntitlement()));
        }

        // ── IStoreListener ──────────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _apple = extensions.GetExtension<IAppleExtensions>();
            _initialization.TrySetResult(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogWarning($"[Entitlement] Unity IAP の初期化に失敗: {error} {message}");

            // 次回の呼び出しで初期化をやり直せるようにする（一時的なネットワーク断からの復帰）。
            _initialization.TrySetResult(false);
            _initialization = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            _purchase?.TrySetResult(EntitlementResult.Fetched(CurrentEntitlement()));
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"[Entitlement] 購入に失敗: {failureReason}");
            _purchase?.TrySetResult(EntitlementResult.Unavailable());
        }
    }
}
#endif
