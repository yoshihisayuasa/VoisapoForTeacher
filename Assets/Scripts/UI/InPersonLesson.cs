using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 対面レッスン。Photon に接続しないので、相手が来ることはなくルームIDもない。
    /// </summary>
    public sealed class InPersonLesson : ILessonMode
    {
        public void HideInPersonOnly(GameObject target)
        {
        }

        public void HideOnlineOnly(GameObject target)
        {
            target.SetActive(false);
        }

        public void ShowRoomId(TMP_Text text)
        {
        }
    }
}
