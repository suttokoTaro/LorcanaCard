using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public CardView view; // カードの見た目の処理
    public CardModel model; // カードのデータを処理

    private void Awake()
    {
        view = GetComponent<CardView>();
    }

    /** カードの生成（表画像表示） */
    public void CreateCardAndViewIcon(int cardID)
    {
        model = new CardModel(cardID);
        view.ShowIcon(model);
    }

    /** カードの生成（表画像表示） */
    public void CreateCardAndViewBackIcon(int cardID)
    {
        model = new CardModel(cardID);
        view.ShowBackIcon(model);
    }
}