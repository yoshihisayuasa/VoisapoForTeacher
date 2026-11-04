using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// オンラインレッスンでだけ使える操作（イヤホンモード・音の再生側の切り替えなど）に付ける。
    /// 対面レッスンでは消さずに薄く表示し、押されたらオンラインレッスンで使えると案内する。
    /// 押す必要のない表示には、これではなく <see cref="OnlineLessonOnly"/> を付けて隠す。
    /// </summary>
    public sealed class OnlineLessonFeature : MonoBehaviour
    {
        private static readonly LocalizedMessage Notice = new(
            japanese: "この機能はオンラインレッスンで使えます。",
            english: "This feature is available in online lessons.");

        private void Awake()
        {
            LessonModeHolder.RestrictOnlineOnly(new UnavailableFeature(gameObject, Notice));
        }
    }
}
