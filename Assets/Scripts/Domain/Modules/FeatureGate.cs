using Assets.Scripts.Domain.ValueObjects;
using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 機能そのものがプレミアム限定であるゲート。可否は課金状態だけで決まる。
    /// </summary>
    public sealed class FeatureGate : IPremiumGate
    {
        public void Gate(Entitlement entitlement, Action onAllowed, Action onDenied)
        {
            entitlement.Gate(onAllowed, onDenied);
        }
    }
}
