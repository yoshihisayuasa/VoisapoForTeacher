#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using Assets.Scripts.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Development Build 専用の課金状態（FakePurchaseGateway）の切り替え表示。
    /// エディタの Tools > Voisapo > 課金状態 メニューの代わり。シーンに置かず、起動時にコードから生成する。
    /// TeacherMain は読み込み直されるたびに生成されるため、重ならないようシーンと一緒に破棄させる。
    /// 切り替えると照会し直すため、その場で各画面の解錠状態に反映される。
    /// </summary>
    public sealed class FakeEntitlementOverlay : MonoBehaviour
    {
        private const float ButtonWidth = 110f;
        private const float ButtonHeight = 36f;
        private const float Margin = 8f;

        private static readonly (string Label, FakePurchaseGateway.FakeState State)[] Choices =
        {
            ("未購入", FakePurchaseGateway.FakeState.NotPurchased),
            ("購入済み", FakePurchaseGateway.FakeState.Purchased),
            ("期限切れ", FakePurchaseGateway.FakeState.Expired),
        };

        public static void Spawn()
        {
            new GameObject(nameof(FakeEntitlementOverlay)).AddComponent<FakeEntitlementOverlay>();
        }

        private void OnGUI()
        {
            var x = Screen.width - (ButtonWidth + Margin) * Choices.Length;
            var y = Screen.height - ButtonHeight - Margin;

            foreach (var choice in Choices)
            {
                var label = FakePurchaseGateway.State == choice.State ? $"● {choice.Label}" : choice.Label;
                if (GUI.Button(new Rect(x, y, ButtonWidth, ButtonHeight), label))
                {
                    Switch(choice.State);
                }
                x += ButtonWidth + Margin;
            }
        }

        private static void Switch(FakePurchaseGateway.FakeState state)
        {
            FakePurchaseGateway.State = state;
            EntitlementManager.Instance.RestoreAsync().Forget();
        }
    }
}
#endif
