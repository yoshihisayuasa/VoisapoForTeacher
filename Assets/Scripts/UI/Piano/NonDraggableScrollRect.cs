using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class NonDraggableScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
    private const float _wheelSensitivity = 0.03f;


    /// <summary>
    /// UI外でもスクロールホイールで動かせるようにする
    /// ドラッグによるスクロールは無効化
    /// </summary>
    void Update()
    {
        float scroll = 0f;

        if (Mouse.current != null)
        {
            scroll = Mouse.current.scroll.ReadValue().y;
        }
        // マウスホイールの入力を取得
        var kb = Keyboard.current;
        bool ctrl =
             kb != null &&
            (kb.leftCtrlKey.isPressed ||
            kb.rightCtrlKey.isPressed ||
            kb.leftCommandKey.isPressed ||
            kb.rightCommandKey.isPressed);

        var delta = scroll * _wheelSensitivity;

        if (Mathf.Abs(delta) > 0f && !ctrl)
        {
            // スクロール方向に応じてScrollRectを動かす
            if (vertical)
            {
                verticalNormalizedPosition += delta;
                verticalNormalizedPosition = Mathf.Clamp01(verticalNormalizedPosition);
            }
            else if (horizontal)
            {
                horizontalNormalizedPosition += delta;
                horizontalNormalizedPosition = Mathf.Clamp01(horizontalNormalizedPosition);
            }
        }
    }

    public override void OnScroll(PointerEventData data)
    {
    }
}