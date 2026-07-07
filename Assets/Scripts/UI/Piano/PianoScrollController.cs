using Assets.Scripts.UI;
using Assets.Scripts.UI.Piano;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public sealed class PianoScrollController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private ScrollRect pianoScrollRect;
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private RectTransform miniMapViewRect;
    [SerializeField] private RectTransform leftDarkOverlay;
    [SerializeField] private RectTransform rightDarkOverlay;
    [SerializeField] private Button octaveUpButton;
    [SerializeField] private Button octaveDownButton;

    private float _prevDragLocalX = 0f;
    private Coroutine _initRoutine;

    private void OnEnable()
    {
        if (pianoScrollRect != null)
        {
            pianoScrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        if (octaveUpButton != null) octaveUpButton.onClick.AddListener(ScrollOctaveUp);
        if (octaveDownButton != null) octaveDownButton.onClick.AddListener(ScrollOctaveDown);

        _initRoutine = StartCoroutine(DeferredInit());
    }

    private void OnDisable()
    {
        if (pianoScrollRect != null)
        {
            pianoScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }

        if (octaveUpButton != null) octaveUpButton.onClick.RemoveListener(ScrollOctaveUp);
        if (octaveDownButton != null) octaveDownButton.onClick.RemoveListener(ScrollOctaveDown);

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

    public void ScrollOctaveUp() => ScrollByKeys(12);
    public void ScrollOctaveDown() => ScrollByKeys(-12);

    private void ScrollByKeys(int keyCount)
    {
        if (pianoScrollRect == null) return;

        var viewport = pianoScrollRect.viewport;
        if (pianoScrollRect.content == null || viewport == null) return;

        float contentWidth = ScrollRectGeometry.VisualContentWidth(pianoScrollRect);
        float viewportWidth = viewport.rect.width;
        float scrollable = contentWidth - viewportWidth;
        if (scrollable <= 0f) return;

        int totalKeys = PianoController.Instance.KeyCount;
        if (totalKeys <= 0) return;

        float singleKeyWidth = contentWidth / totalKeys;
        float delta = keyCount * singleKeyWidth / scrollable;

        pianoScrollRect.horizontalNormalizedPosition =
            Mathf.Clamp01(pianoScrollRect.horizontalNormalizedPosition + delta);
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

        float contentWidth = ScrollRectGeometry.VisualContentWidth(pianoScrollRect);
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

        float viewRectWidth = ratio * miniMapWidth;
        miniMapViewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewRectWidth);
        miniMapViewRect.anchoredPosition = new Vector2(pos, miniMapViewRect.anchoredPosition.y);

        if (leftDarkOverlay != null)
        {
            leftDarkOverlay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pos);
        }

        if (rightDarkOverlay != null)
        {
            float rightStart = pos + viewRectWidth;
            rightDarkOverlay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, miniMapWidth - rightStart);
        }
    }
}
