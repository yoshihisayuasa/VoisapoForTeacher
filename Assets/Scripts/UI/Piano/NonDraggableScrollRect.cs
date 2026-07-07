using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NonDraggableScrollRect : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }

    private const float _wheelSensitivity = 0.03f;

    /// <summary>
    /// UI外でもスクロールホイールで動かせるようにする。
    /// ドラッグによるスクロールは無効化。
    /// Ctrl+ホイールはズーム（PianoZoomController）に譲るため反応しない。
    /// </summary>
    private void Update()
    {
        float delta = ScrollWheelInput.ReadScroll() * _wheelSensitivity;

        if (Mathf.Abs(delta) > 0f && !ScrollWheelInput.IsCtrlOrCommandPressed())
        {
            horizontalNormalizedPosition = Mathf.Clamp01(horizontalNormalizedPosition + delta);
        }
    }

    public override void OnScroll(PointerEventData data)
    {
    }
}
