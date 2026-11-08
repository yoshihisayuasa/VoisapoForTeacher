using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// 課金状態。有効期限を内部に持ち、作られた時点で期限内ならプレミアムとして扱う
    /// （トライアル中も期限付きプレミアムとして同一に扱う）。
    /// 判定は作られた時点で固定し、その後に期限の時刻を迎えても変えない。
    /// 作り直されるのは起動・購入・復元のときだけなので、起動中に期限をまたいでも
    /// レッスンの途中で機能が止まらない（解約した場合も次の起動から無料に戻る）。
    /// 「どの機能がプレミアムか」というドメインルールもここが持ち、
    /// 外へは Gate / Describe という振る舞いだけを提供する（期限やフラグは公開しない）。
    /// </summary>
    public sealed class Entitlement : ValueObject<Entitlement>
    {
        /// <summary>未課金。期限を持たないため、常にどの機能も許可しない。</summary>
        public static Entitlement Free { get; } = new Entitlement(DateTimeOffset.MinValue);

        private readonly DateTimeOffset _expiresAt;
        private readonly bool _isActive;

        private Entitlement(DateTimeOffset expiresAt)
        {
            _expiresAt = expiresAt;
            _isActive = DateTimeOffset.UtcNow < expiresAt;
        }

        /// <summary>期限付きのプレミアム。無料トライアル中もこれで表す。</summary>
        public static Entitlement PremiumUntil(DateTimeOffset expiresAt)
        {
            return new Entitlement(expiresAt);
        }

        /// <summary>
        /// プレミアム機能が使えるなら onAllowed、使えないなら onDenied を呼ぶ。
        /// プレミアム機能はすべて、作られた時点で期限内だったかどうかだけで可否が決まる。
        /// </summary>
        public void Gate(Action onAllowed, Action onDenied)
        {
            if (_isActive)
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
            if (_isActive)
            {
                onPremium(_expiresAt);
                return;
            }
            onFree();
        }

        protected override bool EqualsCore(Entitlement other)
        {
            return _expiresAt == other._expiresAt && _isActive == other._isActive;
        }

        protected override int GetHashCodeCore()
        {
            return (_expiresAt.GetHashCode() * 397) ^ _isActive.GetHashCode();
        }
    }
}
