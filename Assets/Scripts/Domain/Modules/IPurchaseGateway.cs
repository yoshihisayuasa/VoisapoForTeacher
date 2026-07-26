using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// ストアのアプリ内課金への出入り口。実装はプラットフォームごとに差し替える。
    /// 解約・返金・自動更新・トライアルの管理はすべてストア側の責務であり、
    /// アプリはここから得た結果に従うだけとする。
    /// </summary>
    public interface IPurchaseGateway
    {
        /// <summary>ストアの購読状態（有効期限含む）を照会する。起動時に呼ぶ。</summary>
        UniTask<EntitlementResult> FetchEntitlementAsync();

        /// <summary>選択プランの購入フローを起動し、完了後の状態を返す。</summary>
        UniTask<EntitlementResult> PurchaseAsync(SubscriptionPlan plan);

        /// <summary>購入を復元する。ストアによっては照会し直すだけになる。</summary>
        UniTask<EntitlementResult> RestoreAsync();

        /// <summary>月額／年額のローカライズ済み価格を取得する。</summary>
        UniTask<PlanPrices> GetPlanPricesAsync();

        /// <summary>解約・プラン変更を行うストアの購読管理画面。アプリ内では解約処理をしない。</summary>
        string ManagementUrl { get; }
    }
}
