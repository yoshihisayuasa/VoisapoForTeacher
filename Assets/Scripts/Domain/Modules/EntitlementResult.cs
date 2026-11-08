using Assets.Scripts.Domain.ValueObjects;
using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// ストアへの照会・購入・復元の結果。
    /// 「取得できた課金状態」と「ストアへ届かなかった」を区別するために使う。
    /// オフライン起動でキャッシュを上書きしてしまわないよう、届かなかった場合は
    /// 呼び出し側に Free を渡さず onUnavailable を呼ぶ。
    /// どちらを呼ぶかは結果の種類ごとのクラスが決める。
    /// </summary>
    public abstract class EntitlementResult
    {
        /// <summary>オフラインやストアのエラーで状態を確定できなかった。</summary>
        public static EntitlementResult Unavailable { get; } = new UnavailableResult();

        private EntitlementResult()
        {
        }

        /// <summary>ストアから状態を取得できた。未購読なら <see cref="Entitlement.Free"/> を渡す。</summary>
        public static EntitlementResult Fetched(Entitlement entitlement)
        {
            return new FetchedResult(entitlement);
        }

        public abstract void Apply(Action<Entitlement> onFetched, Action onUnavailable);

        private sealed class FetchedResult : EntitlementResult
        {
            private readonly Entitlement _entitlement;

            public FetchedResult(Entitlement entitlement)
            {
                _entitlement = entitlement;
            }

            public override void Apply(Action<Entitlement> onFetched, Action onUnavailable)
            {
                onFetched(_entitlement);
            }
        }

        private sealed class UnavailableResult : EntitlementResult
        {
            public override void Apply(Action<Entitlement> onFetched, Action onUnavailable)
            {
                onUnavailable();
            }
        }
    }
}
