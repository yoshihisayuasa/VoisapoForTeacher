namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// ストアから取得した、ローカライズ済みの購読プラン価格。
    /// 価格はストアが正であり、アプリ側にハードコードしない。
    /// </summary>
    public sealed class PlanPrices : ValueObject<PlanPrices>
    {
        /// <summary>ストアへ問い合わせできなかったときの表示。</summary>
        public static PlanPrices Unavailable { get; } = new PlanPrices("--", "--");

        private readonly string _monthly;
        private readonly string _yearly;

        public PlanPrices(string monthly, string yearly)
        {
            _monthly = monthly;
            _yearly = yearly;
        }

        public string PriceOf(SubscriptionPlan plan)
        {
            return plan == SubscriptionPlan.Monthly ? _monthly : _yearly;
        }

        protected override bool EqualsCore(PlanPrices other)
        {
            return _monthly == other._monthly && _yearly == other._yearly;
        }

        protected override int GetHashCodeCore()
        {
            return (_monthly, _yearly).GetHashCode();
        }
    }
}
