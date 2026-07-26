using Assets.Scripts.Domain.Modules;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 鍵アイコンに付ける。課金状態を購読し、ゲートの判定に応じて自分自身を出し入れする。
    /// 非表示中も購読は生きているため（AddTo は破棄時にだけ解除）、
    /// 購入すればその場で消え、期限切れになれば再び現れる。
    /// </summary>
    public sealed class PremiumLockIcon : MonoBehaviour
    {
        public void Bind(IPremiumGate gate)
        {
            EntitlementManager.Instance.Current
                .Subscribe(entitlement => gate.Gate(
                    entitlement,
                    onAllowed: () => gameObject.SetActive(false),
                    onDenied: () => gameObject.SetActive(true)))
                .AddTo(this);
        }
    }
}
