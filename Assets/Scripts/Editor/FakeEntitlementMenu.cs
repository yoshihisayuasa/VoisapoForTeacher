using Assets.Scripts.Infrastructure;
using UnityEditor;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// エディタ実行時の課金状態（EditorFakePurchaseGateway）の切り替え。
    /// 再生中に切り替えた場合は、次の照会（起動 or 復元）から反映される。
    /// </summary>
    public static class FakeEntitlementMenu
    {
        private const string NotPurchasedPath = "Tools/Voisapo/課金状態/未購入";
        private const string PurchasedPath = "Tools/Voisapo/課金状態/購入済み";
        private const string ExpiredPath = "Tools/Voisapo/課金状態/期限切れ";

        [MenuItem(NotPurchasedPath)]
        private static void SetNotPurchased()
            => EditorFakePurchaseGateway.State = EditorFakePurchaseGateway.FakeState.NotPurchased;

        [MenuItem(PurchasedPath)]
        private static void SetPurchased()
            => EditorFakePurchaseGateway.State = EditorFakePurchaseGateway.FakeState.Purchased;

        [MenuItem(ExpiredPath)]
        private static void SetExpired()
            => EditorFakePurchaseGateway.State = EditorFakePurchaseGateway.FakeState.Expired;

        [MenuItem(NotPurchasedPath, isValidateFunction: true)]
        private static bool ValidateNotPurchased()
            => Check(NotPurchasedPath, EditorFakePurchaseGateway.FakeState.NotPurchased);

        [MenuItem(PurchasedPath, isValidateFunction: true)]
        private static bool ValidatePurchased()
            => Check(PurchasedPath, EditorFakePurchaseGateway.FakeState.Purchased);

        [MenuItem(ExpiredPath, isValidateFunction: true)]
        private static bool ValidateExpired()
            => Check(ExpiredPath, EditorFakePurchaseGateway.FakeState.Expired);

        private static bool Check(string menuPath, EditorFakePurchaseGateway.FakeState state)
        {
            Menu.SetChecked(menuPath, EditorFakePurchaseGateway.State == state);
            return true;
        }
    }
}
