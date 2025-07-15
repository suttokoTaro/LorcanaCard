using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 endPos;

    [SerializeField] private float swipeThreshold = 80f; // スワイプとみなす距離

    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;

    private List<CardEntity> cardList;
    private int currentIndex = 0;

    public void SetCardList(List<CardEntity> newCardList)
    {
        cardList = newCardList;
        currentIndex = 0;
        ShowCard(currentIndex);
    }

    public void SetCurrentIndex(int selectedcardIndex)
    {
        currentIndex = selectedcardIndex;
        //Debug.Log("受け取ったインデックス番号：" + selectedcardIndex);
    }

    private void ShowCard(int index)
    {
        if (cardList != null && index >= 0 && index < cardList.Count)
        {
            if (zoomImage != null)
            {
                zoomImage.sprite = cardList[index].icon;
                //Debug.Log("拡大表示のインデックス番号：" + index);
            }
        }
    }

    private void ShowNextCard()
    {
        if (cardList == null || cardList.Count == 0) return;
        currentIndex = (currentIndex + 1) % cardList.Count;
        ShowCard(currentIndex);
    }

    private void ShowPreviousCard()
    {
        if (cardList == null || cardList.Count == 0) return;
        currentIndex = (currentIndex - 1 + cardList.Count) % cardList.Count;
        ShowCard(currentIndex);
    }
    void Update()
    {
        // --- スマホタッチ操作 ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    endPos = touch.position;
                    DetectSwipe();
                    break;
            }
        }
        /**
        // --- PCマウス操作 ---
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            endPos = Input.mousePosition;
            DetectSwipe();
        }
        */
    }

    private void DetectSwipe()
    {
        Vector2 delta = endPos - startPos;

        if (delta.magnitude < swipeThreshold)
            return; // スワイプとして認識しない（短すぎる）

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // 左右スワイプ
            if (delta.x > 0)
                OnSwipeRight();
            else
                OnSwipeLeft();
        }
        else
        {
            // 上下スワイプ
            if (delta.y > 0)
                OnSwipeUp();
            else
                OnSwipeDown();
        }
    }

    private void OnSwipeLeft()
    {
        //Debug.Log("← 左スワイプ");
        // 必要な処理を書く
        ShowNextCard();
    }

    private void OnSwipeRight()
    {
        //Debug.Log("→ 右スワイプ");
        // 必要な処理を書く
        ShowPreviousCard();
    }

    private void OnSwipeUp()
    {
        //Debug.Log("↑ 上スワイプ");
        if (zoomCanvas != null)
        {
            zoomCanvas.SetActive(false);
        }
        // 必要な処理を書く
    }

    private void OnSwipeDown()
    {
        //Debug.Log("↓ 下スワイプ");
        if (zoomCanvas != null)
        {
            zoomCanvas.SetActive(false);
        }
        // 必要な処理を書く
    }
}
