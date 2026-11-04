using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 対面レッスンでだけ使える操作（録音・再生など）に付ける。
    /// オンラインレッスンでは消さずに薄く表示し、押されたら対面レッスンで使えると案内する。
    /// 押す必要のない表示には、これではなく <see cref="InPersonLessonOnly"/> を付けて隠す。
    /// </summary>
    public sealed class InPersonLessonFeature : MonoBehaviour
    {
        private static readonly LocalizedMessage Notice = new(
            japanese: "この機能は対面レッスンで使えます。",
            english: "This feature is available in in-person lessons.");

        private void Awake()
        {
            LessonModeHolder.RestrictInPersonOnly(new UnavailableFeature(gameObject, Notice));
        }
    }
}
