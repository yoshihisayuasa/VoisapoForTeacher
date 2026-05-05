using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PianoScrollController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private ScrollRect pianoScrollRect;
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private RectTransform miniMapViewRect;

    private float _prevDragLocalX = 0f;
    private Coroutine _initRoutine;

    private void OnEnable()
    {
        if (pianoScrollRect != null)
        {
            pianoScrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        _initRoutine = StartCoroutine(DeferredInit());
    }

    private void OnDisable()
    {
        if (pianoScrollRect != null)
        {
            pianoScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
        if (_initRoutine != null)
        {
            StopCoroutine(_initRoutine);
            _initRoutine = null;
        }
    }

    private IEnumerator DeferredInit()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        UpdateViewRect();
    }

    private void OnScrollChanged(Vector2 _)
    {
        UpdateViewRect();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (miniMapRect == null || miniMapViewRect == null)
        {
            return;
        }

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                miniMapRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            return;
        }

        _prevDragLocalX = localPoint.x;

        float localX = localPoint.x - miniMapRect.rect.x;
        float barLeft = miniMapViewRect.anchoredPosition.x;
        float barRight = barLeft + miniMapViewRect.rect.width;

        if (localX < barLeft || localX > barRight)
        {
            float maxMove = miniMapRect.rect.width - miniMapViewRect.rect.width;
            if (maxMove > 0.0001f)
            {
                float targetLeft = localX - miniMapViewRect.rect.width / 2f;
                pianoScrollRect.horizontalNormalizedPosition = Mathf.Clamp01(targetLeft / maxMove);
                UpdateViewRect();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (miniMapRect == null || pianoScrollRect == null || miniMapViewRect == null)
        {
            return;
        }

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                miniMapRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            return;
        }

        float deltaX = localPoint.x - _prevDragLocalX;
        _prevDragLocalX = localPoint.x;

        float maxMove = miniMapRect.rect.width - miniMapViewRect.rect.width;
        if (maxMove <= 0.0001f)
        {
            return;
        }

        float deltaNormalized = deltaX / maxMove;
        pianoScrollRect.horizontalNormalizedPosition =
            Mathf.Clamp01(pianoScrollRect.horizontalNormalizedPosition + deltaNormalized);
        UpdateViewRect();
    }

    /// <summary>
    /// ミニマップ更新。viewRect は左端アンカー・左端ピボット前提。
    /// </summary>
    private void UpdateViewRect()
    {
        if (pianoScrollRect == null || miniMapRect == null || miniMapViewRect == null)
        {
            return;
        }
        if (pianoScrollRect.content == null || pianoScrollRect.viewport == null)
        {
            return;
        }

        float contentWidth = pianoScrollRect.content.rect.width * pianoScrollRect.content.localScale.x;
        float viewportWidth = pianoScrollRect.viewport.rect.width;
        float miniMapWidth = miniMapRect.rect.width;

        if (contentWidth <= 0f || miniMapWidth <= 0f)
        {
            return;
        }

        float ratio = Mathf.Clamp01(viewportWidth / contentWidth);
        float posRange = Mathf.Max(0f, miniMapWidth - ratio * miniMapWidth);
        float normalized = Mathf.Clamp01(pianoScrollRect.horizontalNormalizedPosition);
        float pos = normalized * posRange;

        miniMapViewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ratio * miniMapWidth);
        miniMapViewRect.anchoredPosition = new Vector2(pos, miniMapViewRect.anchoredPosition.y);
    }
}
