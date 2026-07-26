using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 課金状態。有効期限を内部に持ち、現在時刻が期限内ならプレミアムとして扱う
    /// （トライアル中も期限付きプレミアムとして同一に扱う）。
    /// 「どの機能がプレミアムか」というドメインルールもここが持ち、
    /// 外へは Gate / Describe という振る舞いだけを提供する（期限やフラグは公開しない）。
    /// </summary>
    public sealed class Entitlement : ValueObject<Entitlement>
    {
        /// <summary>未課金。期限を持たないため、常にどの機能も許可しない。</summary>
        public static Entitlement Free { get; } = new Entitlement(DateTimeOffset.MinValue);

        private readonly DateTimeOffset _expiresAt;

        private Entitlement(DateTimeOffset expiresAt)
        {
            _expiresAt = expiresAt;
        }

        /// <summary>期限付きのプレミアム。無料トライアル中もこれで表す。</summary>
        public static Entitlement PremiumUntil(DateTimeOffset expiresAt)
        {
            return new Entitlement(expiresAt);
        }

        private bool IsActive => DateTimeOffset.UtcNow < _expiresAt;

        /// <summary>
        /// feature が今使えるなら onAllowed、使えないなら onDenied を呼ぶ。
        /// <see cref="PremiumFeature"/> に挙げた機能はすべてプレミアム限定であり、
        /// 期限内かどうかだけが可否を決める。
        /// </summary>
        public void Gate(PremiumFeature feature, Action onAllowed, Action onDenied)
        {
            if (IsActive)
            {
                onAllowed();
                return;
            }
            onDenied();
        }

        /// <summary>
        /// 状態を伝える。未課金なら onFree、プレミアムなら有効期限つきで onPremium を呼ぶ。
        /// 表示と保存の両方がこれを使う。
        /// </summary>
        public void Describe(Action onFree, Action<DateTimeOffset> onPremium)
        {
            if (IsActive)
            {
                onPremium(_expiresAt);
                return;
            }
            onFree();
        }

        protected override bool EqualsCore(Entitlement other)
        {
            return _expiresAt == other._expiresAt;
        }

        protected override int GetHashCodeCore()
        {
            return _expiresAt.GetHashCode();
        }
    }
}
