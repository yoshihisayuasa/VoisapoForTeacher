using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// アプリ全体の課金状態の保持者。
    /// 起動時はキャッシュで即座に初期化し、ストア照会の結果で上書きする。
    /// 購入・復元の結果もここへ流れるため、購読している UI はその場で解錠される。
    /// </summary>
    public sealed class EntitlementManager
    {
        public static EntitlementManager Instance { get; } = new EntitlementManager();

        private readonly ReactiveProperty<Entitlement> _current = new(Entitlement.Free);
        public ReadOnlyReactiveProperty<Entitlement> Current => _current;

        private IPurchaseGateway _gateway;

        /// <summary>解約・プラン変更を行うストアの購読管理画面。</summary>
        public string ManagementUrl => _gateway.ManagementUrl;

        private EntitlementManager()
        {
        }

        /// <summary>起動時に composition root から一度だけ呼ぶ。</summary>
        public async UniTask InitializeAsync(IPurchaseGateway gateway)
        {
            _gateway = gateway;

            // オフライン起動でもすぐ操作できるよう、まずキャッシュで初期化する。
            _current.Value = EntitlementCache.Load();

            var result = await _gateway.FetchEntitlementAsync();
            result.Apply(Apply, KeepCached);
        }

        public async UniTask PurchaseAsync(SubscriptionPlan plan)
        {
            var result = await _gateway.PurchaseAsync(plan);
            result.Apply(Apply, KeepCached);
        }

        public async UniTask RestoreAsync()
        {
            var result = await _gateway.RestoreAsync();
            result.Apply(Apply, KeepCached);
        }

        public UniTask<PlanPrices> GetPlanPricesAsync()
        {
            return _gateway.GetPlanPricesAsync();
        }

        private void Apply(Entitlement entitlement)
        {
            _current.Value = entitlement;
            EntitlementCache.Save(entitlement);
        }

        /// <summary>
        /// ストアへ届かなかった場合。Free へ落とさずキャッシュの状態を維持する
        /// （期限切れならキャッシュ自体がプレミアムとして働かないため、放置で Free に落ちる）。
        /// </summary>
        private void KeepCached()
        {
            Debug.LogWarning("[Entitlement] ストアへ照会できませんでした。キャッシュした状態を継続します。");
        }
    }
}
