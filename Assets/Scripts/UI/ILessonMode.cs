using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// レッスンのモード（対面／オンライン）。モードごとに違う画面の出し分けを受け持つ。
    /// </summary>
    public interface ILessonMode
    {
        /// <summary>対面レッスン専用の表示を、このモードで不要なら非表示にする。</summary>
        void HideInPersonOnly(GameObject target);

        /// <summary>オンラインレッスン専用の表示を、このモードで不要なら非表示にする。</summary>
        void HideOnlineOnly(GameObject target);

        /// <summary>対面レッスン専用の操作を、このモードで使えなければ使えない状態にする。</summary>
        void RestrictInPersonOnly(UnavailableFeature feature);

        /// <summary>オンラインレッスン専用の操作を、このモードで使えなければ使えない状態にする。</summary>
        void RestrictOnlineOnly(UnavailableFeature feature);

        /// <summary>生徒に伝えるルームIDがあれば表示する。</summary>
        void ShowRoomId(TMP_Text text);
    }
}
