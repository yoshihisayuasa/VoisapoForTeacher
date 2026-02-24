using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NonDraggableScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
    private const float _wheelSensitivity = 0.01f;

    /// <summary>
    /// UI外でもスクロールホイールで動かせるようにする
    /// ドラッグによるスクロールは無効化
    /// </summary>
    void Update()
    {
        // マウスホイールの入力を取得
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool ctrl =
         Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
         Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

        if (Mathf.Abs(scroll) > _wheelSensitivity && !ctrl)
        {
            // スクロール方向に応じてScrollRectを動かす
            if (vertical)
            {
                verticalNormalizedPosition += scroll;
                verticalNormalizedPosition = Mathf.Clamp01(verticalNormalizedPosition);
            }
            else if (horizontal)
            {
                horizontalNormalizedPosition += scroll;
                horizontalNormalizedPosition = Mathf.Clamp01(horizontalNormalizedPosition);
            }
        }
    }

    public override void OnScroll(PointerEventData data)
    {
        base.OnScroll(data);
    }
}