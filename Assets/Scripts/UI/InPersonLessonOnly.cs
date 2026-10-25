using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 対面レッスン専用の表示（録音・再生など）に付ける。
    /// オンラインレッスンでは非表示にする。
    /// 他のコンポーネントから参照されている表示でも壊れないよう、Destroy ではなく非アクティブにする。
    /// </summary>
    public sealed class InPersonLessonOnly : MonoBehaviour
    {
        private void Awake()
        {
            LessonModeHolder.HideInPersonOnly(gameObject);
        }
    }
}
