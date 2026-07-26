namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// プレミアム（課金）限定の機能。ここに挙がっている機能はすべて購読中のみ利用できる。
    /// 可否の判断は <see cref="Entitlement.Gate"/> が行う。
    /// </summary>
    public enum PremiumFeature
    {
        AutoKeyChange,
        MelodyCreate,
        PremiumMelodySelect,
        MelodyDelete,
    }
}
