using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// ストアを持たないプラットフォーム向け。購入できないので常に Free を返す。
    /// Mac App Store / Microsoft Store 以外への配布ビルドが、
    /// 課金状態の照会で落ちないようにするための実装。
    /// </summary>
    public sealed class NoStorePurchaseGateway : IPurchaseGateway
    {
        public string ManagementUrl => string.Empty;

        public UniTask<EntitlementResult> FetchEntitlementAsync()
        {
            Debug.LogWarning("[Entitlement] このプラットフォームはストア課金に対応していません。Free として扱います。");
            return UniTask.FromResult(EntitlementResult.Fetched(Entitlement.Free));
        }

        public UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan)
        {
            return UniTask.FromResult(EntitlementResult.Unavailable());
        }

        public UniTask<EntitlementResult> RestoreAsync()
        {
            return UniTask.FromResult(EntitlementResult.Unavailable());
        }

        public UniTask<PlanPrices> GetPlanPricesAsync()
        {
            return UniTask.FromResult(PlanPrices.Unavailable);
        }
    }
}
