#if UNITY_EDITOR
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 開発用のフェイク。未購入 / 購入済み / 期限切れ を切り替えて動作を確認する。
    /// 切り替えは Tools > Voisapo > 課金状態 メニューから行い、EditorPrefs に保存する。
    /// </summary>
    public sealed class EditorFakePurchaseGateway : IPurchaseGateway
    {
        public enum FakeState
        {
            NotPurchased,
            Purchased,
            Expired,
        }

        private const string StateKey = "Voisapo.FakeEntitlementState";

        /// <summary>購入済みとして扱うときの残り期間。</summary>
        private static readonly TimeSpan PurchasedDuration = TimeSpan.FromDays(30);

        public static FakeState State
        {
            get => (FakeState)EditorPrefs.GetInt(StateKey, (int)FakeState.NotPurchased);
            set => EditorPrefs.SetInt(StateKey, (int)value);
        }

        public string ManagementUrl => "https://example.com/fake-subscription-management";

        public UniTask<EntitlementResult> FetchEntitlementAsync()
        {
            return UniTask.FromResult(EntitlementResult.Fetched(CurrentEntitlement()));
        }

        public UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan)
        {
            State = FakeState.Purchased;
            return UniTask.FromResult(EntitlementResult.Fetched(CurrentEntitlement()));
        }

        public UniTask<EntitlementResult> RestoreAsync()
        {
            return FetchEntitlementAsync();
        }

        public UniTask<PlanPrices> GetPlanPricesAsync()
        {
            return UniTask.FromResult(new PlanPrices("￥1,900", "￥14,900"));
        }

        private static Entitlement CurrentEntitlement()
        {
            switch (State)
            {
                case FakeState.Purchased:
                    return Entitlement.PremiumUntil(DateTimeOffset.UtcNow + PurchasedDuration);
                case FakeState.Expired:
                    return Entitlement.PremiumUntil(DateTimeOffset.UtcNow - TimeSpan.FromDays(1));
                default:
                    return Entitlement.Free;
            }
        }
    }
}
#endif
