using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PianoScrollController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private ScrollRect pianoScrollRect;
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private RectTransform viewRect;

    private float dragOffsetX = 0f; // バー内のどこを掴んだか
    private Coroutine _initRoutine;

    private void OnEnable()
    {
        // スクロール時にミニマップ更新
        if (pianoScrollRect != null)
        {
            pianoScrollRect.onValueChanged.AddListener(OnScrollChanged);
        }
    
        // レイアウト確定後に初期更新（Start より安全）
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
        // 1フレーム待機してから更新（必要ならもう1フレーム）
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
        if (miniMapRect == null || viewRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float barLeft = viewRect.anchoredPosition.x;
            float barRight = barLeft + viewRect.rect.width;
            if (localPoint.x >= barLeft && localPoint.x <= barRight)
            {
                // バーの上をクリックした場合、どこを掴んだか記録
                dragOffsetX = localPoint.x - viewRect.anchoredPosition.x;
            }
            else
            {
                // バー外をクリックした場合、中央を掴む
                dragOffsetX = viewRect.rect.width / 2f;
                SetScrollPositionWithOffset(localPoint.x);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (miniMapRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            SetScrollPositionWithOffset(localPoint.x);
        }
    }

    private void SetScrollPositionWithOffset(float pointerX)
    {
        if (pianoScrollRect == null || miniMapRect == null || viewRect == null) return;

        float targetX = pointerX - dragOffsetX;
        float maxMove = miniMapRect.rect.width - viewRect.rect.width;

        if (maxMove <= 0.0001f)
        {
            // スクロールできない（= ビューポートが全域を覆う等）ので左端固定
            pianoScrollRect.horizontalNormalizedPosition = 0f;
            UpdateViewRect();
            return;
        }

        float normalized = Mathf.Clamp01(targetX / maxMove);
        pianoScrollRect.horizontalNormalizedPosition = normalized;
        UpdateViewRect();
    }


    /// <summary>
    /// ミニマップ更新
    /// </summary>
    private void UpdateViewRect()
    {
        // null/幅ガード
        if (pianoScrollRect == null || miniMapRect == null || viewRect == null) return;
        if (pianoScrollRect.content == null || pianoScrollRect.viewport == null) return;

        float contentWidth = pianoScrollRect.content.rect.width;
        float viewportWidth = pianoScrollRect.viewport.rect.width;
        float miniMapWidth = miniMapRect.rect.width;

        if (contentWidth <= 0f || miniMapWidth <= 0f) return;

        // ビューポート:コンテンツ比率（0..1）
        float ratio = Mathf.Clamp01(viewportWidth / contentWidth);

        // スクロール可能範囲（負にならないようにクランプ）
        float posRange = Mathf.Max(0f, miniMapWidth - ratio * miniMapWidth);

        // 正規化位置を0..1で使用
        float normalized = Mathf.Clamp01(pianoScrollRect.horizontalNormalizedPosition);
        float pos = normalized * posRange;

        viewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ratio * miniMapWidth);
        viewRect.anchoredPosition = new Vector2(pos, viewRect.anchoredPosition.y);
    }
}