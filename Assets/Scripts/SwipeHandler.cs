using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector2 startPos;

    public void OnDrag(PointerEventData eventData)
    {
        if (startPos == Vector2.zero)
            startPos = eventData.pressPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 endPos = eventData.position;
        float deltaX = endPos.x - startPos.x;

        if (Mathf.Abs(deltaX) > 50f) // スワイプ距離のしきい値
        {
            if (deltaX > 0)
                OnSwipeRight();
            else
                OnSwipeLeft();
        }

        startPos = Vector2.zero;
    }

    private void OnSwipeLeft()
    {
        Debug.Log("← 左にスワイプされました");
    }

    private void OnSwipeRight()
    {
        Debug.Log("→ 右にスワイプされました");
    }
}
