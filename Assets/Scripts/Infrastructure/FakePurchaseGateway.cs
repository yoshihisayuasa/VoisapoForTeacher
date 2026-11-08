#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 開発用のフェイク（エディタと Development Build で使う）。未購入 / 購入済み / 期限切れ を切り替えて動作を確認する。
    /// 購入状態とは別に「通信できない」をオンにでき、オンの間は照会・購入・復元がすべて Unavailable を返す
    /// （オフライン起動や、購入・復元の失敗を確認するため）。
    /// 切り替えはエディタでは Tools > Voisapo > 課金状態 メニュー、Development Build では画面右下のデバッグ表示から行い、
    /// PlayerPrefs に保存する。
    /// </summary>
    public sealed class FakePurchaseGateway : IPurchaseGateway
    {
        public enum FakeState
        {
            NotPurchased,
            Purchased,
            Expired,
        }

        private const string StateKey = "Voisapo.FakeEntitlementState";
        private const string OfflineKey = "Voisapo.FakeEntitlementOffline";

        /// <summary>購入済みとして扱うときの残り期間。</summary>
        private static readonly TimeSpan PurchasedDuration = TimeSpan.FromDays(30);

        public static FakeState State
        {
            get => (FakeState)PlayerPrefs.GetInt(StateKey, (int)FakeState.NotPurchased);
            set
            {
                PlayerPrefs.SetInt(StateKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>オンの間はストアへ届かなかったものとして振る舞う。</summary>
        public static bool Offline
        {
            get => PlayerPrefs.GetInt(OfflineKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(OfflineKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public string ManagementUrl => "https://example.com/fake-subscription-management";

        public UniTask<EntitlementResult> FetchEntitlementAsync()
        {
            if (Offline)
            {
                return UniTask.FromResult(EntitlementResult.Unavailable);
            }
            return UniTask.FromResult(EntitlementResult.Fetched(CurrentEntitlement()));
        }

        public UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan)
        {
            if (Offline)
            {
                return UniTask.FromResult(EntitlementResult.Unavailable);
            }
            State = FakeState.Purchased;
            return UniTask.FromResult(EntitlementResult.Fetched(CurrentEntitlement()));
        }

        public UniTask<EntitlementResult> RestoreAsync()
        {
            return FetchEntitlementAsync();
        }

        public UniTask<PlanPrices> GetPlanPricesAsync()
        {
            if (Offline)
            {
                return UniTask.FromResult(PlanPrices.Unavailable);
            }
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
