using Assets.Scripts.Domain.Modules;
using Assets.Scripts.UI.Modal;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// プレミアム限定の操作を持つボタンに付ける。クリックを受け取り、
    /// 許可されていれば本来の処理を、未許可なら購入案内モーダルを開く。
    /// 鍵アイコンの表示切替は PremiumLockIcon に任せる。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class PremiumGateButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private PremiumLockIcon _lockIcon;

        private IPremiumGate _gate;
        private Action _onAllowed;

        /// <summary>
        /// 可否の判断と、許可されたときに実行する本来の処理を結びつける。
        /// 本来の処理は Button.onClick ではなくここへ渡す（クリックはこのコンポーネントが受け取るため）。
        /// </summary>
        public void Bind(IPremiumGate gate, Action onAllowed)
        {
            _gate = gate;
            _onAllowed = onAllowed;

            _button.onClick.AddListener(OnClicked);
            _lockIcon.Bind(gate);
        }

        private void OnClicked()
        {
            _gate.Gate(
                EntitlementManager.Instance.Current.CurrentValue,
                onAllowed: _onAllowed,
                onDenied: PremiumPurchaseModalUI.Show);
        }
    }
}
