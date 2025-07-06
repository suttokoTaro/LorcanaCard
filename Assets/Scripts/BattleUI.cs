using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleUI : MonoBehaviour
{
    [SerializeField] CardController cardPrefab;
    [SerializeField] Transform playerHandArea, enemyHandArea, playerDeckArea, enemyDeckArea;
    [SerializeField] Text playerLoaPointText;
    [SerializeField] Text enemyLoaPointText;
    [SerializeField] private GameObject confirmExitPanel;

    public int playerLoaPoint;
    public int enemyLoaPoint;

    bool isPlayerTurn = true;
    List<int> playerDeckList = new List<int>() { };
    List<int> enemyDeckList = new List<int>() { };

    void Start()
    {
        playerLoaPoint = 0;
        enemyLoaPoint = 0;
        ShowLoaPoint();

        // DeckManager からデッキを受け取る
        if (DeckManager.Instance != null)
        {
            playerDeckList = new List<int>(DeckManager.Instance.selectedPlayerDeck);
            enemyDeckList = new List<int>(DeckManager.Instance.selectedEnemyDeck);
        }

        // 初期手札を受け取って配る
        foreach (int cardId in DeckManager.Instance.playerInitialHand)
        {
            CreateCard(cardId, playerHandArea);
        }
        foreach (int cardId in DeckManager.Instance.enemyInitialHand)
        {
            CreateCard(cardId, enemyHandArea);
        }
    }


    /** カードの生成 */
    void CreateCard(int cardID, Transform place)
    {
        // エリアの種類に応じて、カードの生成処理（HandかFieldかTrashなら表向き／DeckかInkwellなら裏向き）
        CardController card = Instantiate(cardPrefab, place);
        card.CreateCardAndViewBackIcon(cardID);

        // デッキ枚数表示値の更新処理
    }


    void PlayerDrawCard()
    {
        if (playerDeckList.Count > 0)
        {
            int cardID = playerDeckList[0];
            playerDeckList.RemoveAt(0);
            CreateCard(cardID, playerHandArea);
        }
    }

    void EnemyDrawCard()
    {
        if (enemyDeckList.Count > 0)
        {
            int cardID = enemyDeckList[0];
            enemyDeckList.RemoveAt(0);
            CreateCard(cardID, enemyHandArea);
        }
    }


    /** ターン終了ボタン押下時の処理 */
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする
        TurnCalc(); // ターンを相手に回す
    }
    void TurnCalc()
    {
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }
    void PlayerTurn()
    {
        PlayerDrawCard();
    }
    void EnemyTurn()
    {
        EnemyDrawCard();
    }


    /** ロア値の加減ボタン押下時の処理 */
    void ShowLoaPoint()
    {
        playerLoaPointText.text = playerLoaPoint.ToString();
        enemyLoaPointText.text = enemyLoaPoint.ToString();
    }
    public void PlayerLoaPointPlus()
    {
        playerLoaPoint = playerLoaPoint + 1;
        ShowLoaPoint();
    }
    public void PlayerLoaPointMinus()
    {
        playerLoaPoint = playerLoaPoint - 1;
        ShowLoaPoint();
    }
    public void EnemyLoaPointPlus()
    {
        enemyLoaPoint = enemyLoaPoint + 1;
        ShowLoaPoint();
    }
    public void EnemyLoaPointMinus()
    {
        enemyLoaPoint = enemyLoaPoint - 1;
        ShowLoaPoint();
    }


    /** ターン終了ボタン押下時の処理 */
    public void OnClickEndBattleButton()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(true);
    }
    public void OnClickConfirmExitYes()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    public void OnClickConfirmExitNo()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }
}
