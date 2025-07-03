using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform cardParent;
    public Transform zoomParent; // 最前面のZoom用CanvasオブジェクトをInspectorで設定



    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    public float holdTime = 0.5f;

    private Vector3 originalScale;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private Vector2 originalSizeDelta;
    private bool isZoomed = false;

    private Coroutine holdCoroutine;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        holdCoroutine = StartCoroutine(HoldDetection());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        pointerDownTimer = 0f;

        if (isZoomed)
        {
            ResetZoom();
        }

        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);
    }

    private IEnumerator HoldDetection()
    {
        pointerDownTimer = 0f;

        while (pointerDownTimer < holdTime)
        {
            if (!isPointerDown)
                yield break;

            pointerDownTimer += Time.deltaTime;
            yield return null;
        }

        ZoomFullScreen();
    }

    private void ZoomFullScreen()
    {
        isZoomed = true;

        // 保存
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalSizeDelta = rectTransform.sizeDelta;

        transform.SetParent(zoomParent, true);

        // フルスクリーン化（画面サイズに合わせる）
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 最前面へ
        transform.SetAsLastSibling();
    }

    private void ResetZoom()
    {
        isZoomed = false;

        transform.SetParent(originalParent, true);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = originalAnchoredPosition;
        rectTransform.sizeDelta = originalSizeDelta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isPointerDown = false;

        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        if (isZoomed)
            ResetZoom();

        cardParent = transform.parent;
        transform.SetParent(cardParent.parent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(cardParent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    private void Start()
{
    // zoomParent が未設定なら自動で探す
    if (zoomParent == null)
    {
        GameObject found = GameObject.Find("ZoomParent");
        if (found != null)
        {
            zoomParent = found.transform;
        }
        else
        {
            Debug.LogWarning("ZoomParent がシーン内に見つかりませんでした");
        }
    }
}
}
