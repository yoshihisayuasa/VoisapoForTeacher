using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// ストアへの照会・購入・復元の結果。
    /// 「取得できた課金状態」と「ストアへ届かなかった」を区別するために使う。
    /// オフライン起動でキャッシュを上書きしてしまわないよう、届かなかった場合は
    /// 呼び出し側に Free を渡さず onUnavailable を呼ぶ。
    /// </summary>
    public sealed class EntitlementResult
    {
        private readonly Entitlement _entitlement;

        private EntitlementResult(Entitlement entitlement)
        {
            _entitlement = entitlement;
        }

        /// <summary>ストアから状態を取得できた。未購読なら <see cref="Entitlement.Free"/> を渡す。</summary>
        public static EntitlementResult Fetched(Entitlement entitlement)
        {
            return new EntitlementResult(entitlement);
        }

        /// <summary>オフラインやストアのエラーで状態を確定できなかった。</summary>
        public static EntitlementResult Unavailable()
        {
            return new EntitlementResult(null);
        }

        public void Apply(Action<Entitlement> onFetched, Action onUnavailable)
        {
            if (_entitlement == null)
            {
                onUnavailable();
                return;
            }
            onFetched(_entitlement);
        }
    }
}
