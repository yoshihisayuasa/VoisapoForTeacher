using Assets.Scripts.Infrastructure;
using UnityEditor;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// エディタ実行時の課金状態（FakePurchaseGateway）の切り替え。
    /// 再生中に切り替えた場合は、次の照会（起動 or 復元）から反映される。
    /// </summary>
    public static class FakeEntitlementMenu
    {
        private const string NotPurchasedPath = "Tools/Voisapo/課金状態/未購入";
        private const string PurchasedPath = "Tools/Voisapo/課金状態/購入済み";
        private const string ExpiredPath = "Tools/Voisapo/課金状態/期限切れ";
        private const string OfflinePath = "Tools/Voisapo/課金状態/通信できない";

        // 購入状態の 3 つ（既定 1000）と区切り線で分けるため、優先度を 11 以上離す。
        private const int OfflinePriority = 1100;

        [MenuItem(NotPurchasedPath)]
        private static void SetNotPurchased()
            => FakePurchaseGateway.State = FakePurchaseGateway.FakeState.NotPurchased;

        [MenuItem(PurchasedPath)]
        private static void SetPurchased()
            => FakePurchaseGateway.State = FakePurchaseGateway.FakeState.Purchased;

        [MenuItem(ExpiredPath)]
        private static void SetExpired()
            => FakePurchaseGateway.State = FakePurchaseGateway.FakeState.Expired;

        [MenuItem(OfflinePath, false, OfflinePriority)]
        private static void ToggleOffline()
            => FakePurchaseGateway.Offline = !FakePurchaseGateway.Offline;

        [MenuItem(NotPurchasedPath, isValidateFunction: true)]
        private static bool ValidateNotPurchased()
            => Check(NotPurchasedPath, FakePurchaseGateway.FakeState.NotPurchased);

        [MenuItem(PurchasedPath, isValidateFunction: true)]
        private static bool ValidatePurchased()
            => Check(PurchasedPath, FakePurchaseGateway.FakeState.Purchased);

        [MenuItem(ExpiredPath, isValidateFunction: true)]
        private static bool ValidateExpired()
            => Check(ExpiredPath, FakePurchaseGateway.FakeState.Expired);

        [MenuItem(OfflinePath, true, OfflinePriority)]
        private static bool ValidateOffline()
        {
            Menu.SetChecked(OfflinePath, FakePurchaseGateway.Offline);
            return true;
        }

        private static bool Check(string menuPath, FakePurchaseGateway.FakeState state)
        {
            Menu.SetChecked(menuPath, FakePurchaseGateway.State == state);
            return true;
        }
    }
}
