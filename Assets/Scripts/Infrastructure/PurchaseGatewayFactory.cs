using Assets.Scripts.Domain.Modules;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 課金ゲートウェイの生成箇所。プラットフォーム分岐の #if はアプリ全体でここ1箇所に閉じる
    /// （AppMode と同じ思想。呼び出し側は IPurchaseGateway しか知らない）。
    /// </summary>
    public static class PurchaseGatewayFactory
    {
        public static IPurchaseGateway Create()
        {
#if UNITY_EDITOR
            return new EditorFakePurchaseGateway();
#elif UNITY_STANDALONE_OSX
            return new MacAppStorePurchaseGateway();
#elif UNITY_WSA
            return new MicrosoftStorePurchaseGateway();
#else
            return new NoStorePurchaseGateway();
#endif
        }
    }
}
