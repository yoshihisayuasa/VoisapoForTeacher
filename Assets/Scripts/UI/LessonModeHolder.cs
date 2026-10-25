using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// スタート画面で選んだレッスンのモードをレッスン画面へ渡す。
    /// 何も選ばずにレッスン画面を開いた場合（Editor で直接開いたときなど）は対面レッスンとして扱う。
    /// </summary>
    public static class LessonModeHolder
    {
        private static ILessonMode _current = new InPersonLesson();

        public static void Set(ILessonMode mode) => _current = mode;

        public static void HideInPersonOnly(GameObject target) => _current.HideInPersonOnly(target);

        public static void HideOnlineOnly(GameObject target) => _current.HideOnlineOnly(target);

        public static void ShowRoomId(TMP_Text text) => _current.ShowRoomId(text);
    }
}
