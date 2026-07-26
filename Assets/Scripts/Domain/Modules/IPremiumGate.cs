using Assets.Scripts.Domain.ValueObjects;
using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 「この操作を今して良いか」の判断。UI 側で課金状態を読んで分岐しないための入り口。
    /// 機能そのものが対象なら <see cref="FeatureGate"/>、
    /// メロディごとに可否が変わるものは <see cref="MelodySelectGate"/> を使う。
    /// </summary>
    public interface IPremiumGate
    {
        void Gate(Entitlement entitlement, Action onAllowed, Action onDenied);
    }
}
