using UnityEngine.InputSystem;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// マウスホイールと修飾キーの現在値を読む共通ヘルパー。
    /// ホイールスクロール（NonDraggableScrollRect）と Ctrl+ホイールズーム（PianoZoomController）が
    /// 同じ判定を重複実装しないための置き場。
    /// </summary>
    public static class ScrollWheelInput
    {
        /// <summary>マウスホイールの縦スクロール量。マウス未接続なら 0。</summary>
        public static float ReadScroll()
        {
            var mouse = Mouse.current;
            return mouse != null ? mouse.scroll.ReadValue().y : 0f;
        }

        /// <summary>Ctrl または Command が押されているか。キーボード未接続なら false。</summary>
        public static bool IsCtrlOrCommandPressed()
        {
            var kb = Keyboard.current;
            return kb != null &&
                   (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed ||
                    kb.leftCommandKey.isPressed || kb.rightCommandKey.isPressed);
        }
    }
}
