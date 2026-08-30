using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using System.Collections.Generic;
using Windows.Services.Store;
#endif

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// Microsoft Store の自動更新サブスクリプションアドオン。
    /// UWP ビルドの利点を使い、C# から WinRT（Windows.Services.Store）を直接呼ぶ。
    /// Unity IAP を使わないのは、5.x が UWP を打ち切り、4.x も購読の有効期限を
    /// Apple / Google からしか取得できないため。
    /// </summary>
    public sealed class MicrosoftStorePurchaseGateway : IPurchaseGateway
    {
        // パートナーセンターで採番されたアドオンのストア ID。
        // 括弧内は同じアドオンの製品 ID（パートナーセンターで人が読む方の識別子）。
        private const string MonthlyStoreId = "9P5P5VSQ5CXW"; // getsugaku_jpy1900_1weekfree
        private const string YearlyStoreId = "9NGHBXBSCPRR";  // nengaku_jpy14900_1weekfree

        /// <summary>サブスクリプションアドオンは Durable として取得する。</summary>
        private static readonly string[] SubscriptionProductKinds = { "Durable" };

        public string ManagementUrl => "ms-windows-store://account/subscriptions";

        public async UniTask<EntitlementResult> FetchEntitlementAsync()
        {
#if ENABLE_WINMD_SUPPORT
            try
            {
                var license = await StoreContext.GetDefault().GetAppLicenseAsync().AsTask();
                return EntitlementResult.Fetched(ToEntitlement(license));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] ストア照会に失敗: {ex.Message}");
                return EntitlementResult.Unavailable();
            }
#else
            await UniTask.CompletedTask;
            return EntitlementResult.Unavailable();
#endif
        }

        public async UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan)
        {
#if ENABLE_WINMD_SUPPORT
            try
            {
                // ストアの購入ダイアログが起動する。キャンセル・失敗も含め、
                // 確定した状態はライセンスの再照会で受け取る。
                await StoreContext.GetDefault().RequestPurchaseAsync(StoreIdOf(plan)).AsTask();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] 購入フローに失敗: {ex.Message}");
                return EntitlementResult.Unavailable();
            }
            return await FetchEntitlementAsync();
#else
            await UniTask.CompletedTask;
            return EntitlementResult.Unavailable();
#endif
        }

        /// <summary>
        /// Microsoft Store は購入がアカウントのライセンスとして常に残るため、
        /// 復元は照会し直すことと同じになる。
        /// </summary>
        public UniTask<EntitlementResult> RestoreAsync()
        {
            return FetchEntitlementAsync();
        }

        public async UniTask<PlanPrices> GetPlanPricesAsync()
        {
#if ENABLE_WINMD_SUPPORT
            try
            {
                var result = await StoreContext.GetDefault()
                    .GetAssociatedStoreProductsAsync(SubscriptionProductKinds)
                    .AsTask();

                return new PlanPrices(
                    FormattedPriceOf(result.Products, MonthlyStoreId),
                    FormattedPriceOf(result.Products, YearlyStoreId));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] 価格の取得に失敗: {ex.Message}");
                return PlanPrices.Unavailable;
            }
#else
            await UniTask.CompletedTask;
            return PlanPrices.Unavailable;
#endif
        }

        private static string StoreIdOf(SubscriptionPlan plan)
        {
            return plan == SubscriptionPlan.Monthly ? MonthlyStoreId : YearlyStoreId;
        }

#if ENABLE_WINMD_SUPPORT
        /// <summary>
        /// 有効なアドオンライセンスの中から、月額／年額のうち最も遅い期限を採用する。
        /// プラン変更の直後は両方のライセンスが一時的に有効になりうるため。
        /// </summary>
        private static Entitlement ToEntitlement(StoreAppLicense license)
        {
            var latest = DateTimeOffset.MinValue;

            foreach (var addOn in license.AddOnLicenses)
            {
                var addOnLicense = addOn.Value;
                if (!addOnLicense.IsActive)
                {
                    continue;
                }
                if (!IsSubscription(addOnLicense.SkuStoreId))
                {
                    continue;
                }
                if (addOnLicense.ExpirationDate > latest)
                {
                    latest = addOnLicense.ExpirationDate;
                }
            }

            return latest == DateTimeOffset.MinValue
                ? Entitlement.Free
                : Entitlement.PremiumUntil(latest);
        }

        /// <summary>SkuStoreId は "9NBLGGH4TNMP/0010" のようにアドオンのストア ID から始まる。</summary>
        private static bool IsSubscription(string skuStoreId)
        {
            return skuStoreId.StartsWith(MonthlyStoreId, StringComparison.Ordinal)
                || skuStoreId.StartsWith(YearlyStoreId, StringComparison.Ordinal);
        }

        private static string FormattedPriceOf(IReadOnlyDictionary<string, StoreProduct> products, string storeId)
        {
            return products.TryGetValue(storeId, out var product)
                ? product.Price.FormattedPrice
                : "--";
        }
#endif
    }
}
