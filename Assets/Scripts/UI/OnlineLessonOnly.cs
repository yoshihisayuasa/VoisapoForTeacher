using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// オンラインレッスン専用の表示（相手の在室ステータスなど）に付ける。
    /// 対面レッスンでは相手が来ることはないので非表示にする。
    /// 他のコンポーネントから参照されている表示でも壊れないよう、Destroy ではなく非アクティブにする。
    /// </summary>
    public sealed class OnlineLessonOnly : MonoBehaviour
    {
        private void Awake()
        {
            LessonModeHolder.HideOnlineOnly(gameObject);
        }
    }
}
