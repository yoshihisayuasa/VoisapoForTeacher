using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 対面レッスン専用の、押す必要のない表示に付ける。操作には <see cref="InPersonLessonFeature"/> を付ける。
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
