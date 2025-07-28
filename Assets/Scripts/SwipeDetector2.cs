using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwipeDetector2 : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 endPos;

    [SerializeField] private float swipeThreshold = 50f; // スワイプとみなす距離

    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    [SerializeField] private Text countText;

    [SerializeField] private DeckDetailUI deckDetailUI;

    private List<CardEntity> cardList;
    private List<(CardEntity entity, int count)> deckCardList = new List<(CardEntity, int)>();

    private int currentIndex;

    // public void SetCardList(List<CardEntity> newCardList)
    // {
    //     cardList = newCardList;
    //     currentIndex = 0;
    //     ShowCard(currentIndex);
    // }

    public void SetDeckCardList(List<(CardEntity entity, int count)> newDeckCardList)
    {
        deckCardList = newDeckCardList;
        currentIndex = 0;
        ShowCard(currentIndex);
    }

    public void SetCurrentIndex(int selectedcardIndex)
    {
        currentIndex = selectedcardIndex;
        Debug.Log("受け取ったインデックス番号：" + selectedcardIndex);
    }

    public void ShowCard(int index)
    {
        if (deckCardList != null && index >= 0 && index < deckCardList.Count)
        {
            if (zoomImage != null)
            {
                zoomImage.sprite = deckCardList[index].entity.icon;
                countText.text = deckCardList[index].count.ToString();
                Debug.Log("拡大表示のインデックス番号：" + index);
            }
        }
    }

    public void OnClickPlusButton()
    {
        if (deckCardList != null && currentIndex >= 0 && currentIndex < deckCardList.Count)
        {
            var deckCard = deckCardList[currentIndex];
            int beforeCount = deckCard.count;
            deckCard.count = beforeCount + 1;
            deckCardList[currentIndex] = deckCard;

            deckDetailUI.PlusDeckCard(deckCard.entity.cardId);
            ShowCard(currentIndex);
        }
    }
    public void OnClickMinusButton()
    {
        if (deckCardList != null && currentIndex >= 0 && currentIndex < deckCardList.Count)
        {
            var deckCard = deckCardList[currentIndex];
            int beforeCount = deckCard.count;
            if (beforeCount > 0)
            {
                deckCard.count = beforeCount - 1;
                deckCardList[currentIndex] = deckCard;
                deckDetailUI.MinusDeckCard(deckCard.entity.cardId);
            }
            ShowCard(currentIndex);
        }
    }
    public void OnClickSetDeckIconButton()
    {
        var deckCard = deckCardList[currentIndex];
        deckDetailUI.SetDeckIcon(deckCard.entity.cardId);
        deckDetailUI.RefreshDeckCardList();
        deckDetailUI.RefreshCardListFiltered();
        zoomCanvas.SetActive(false);
    }

    private void ShowNextCard()
    {
        if (deckCardList == null || deckCardList.Count == 0) return;
        currentIndex = (currentIndex + 1) % deckCardList.Count;
        ShowCard(currentIndex);
    }

    private void ShowPreviousCard()
    {
        if (deckCardList == null || deckCardList.Count == 0) return;
        currentIndex = (currentIndex - 1 + deckCardList.Count) % deckCardList.Count;
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
            deckDetailUI.RefreshDeckCardList();
            deckDetailUI.RefreshCardListFiltered();
            zoomCanvas.SetActive(false);

        }
        // 必要な処理を書く
    }

    private void OnSwipeDown()
    {
        //Debug.Log("↓ 下スワイプ");
        if (zoomCanvas != null)
        {
            deckDetailUI.RefreshDeckCardList();
            deckDetailUI.RefreshCardListFiltered();
            zoomCanvas.SetActive(false);
        }
        // 必要な処理を書く
    }
}
