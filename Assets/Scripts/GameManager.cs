using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] CardController cardPrefab;
    [SerializeField] Transform playerHand, enemyHand;
    [SerializeField] Text playerLoaPointText;
    [SerializeField] Text enemyLoaPointText;
    [SerializeField] private GameObject confirmExitPanel;

    public int playerLoaPoint;
    public int enemyLoaPoint;

    bool isPlayerTurn = true; //
    List<int> deck = new List<int>() { 3001, 3001, 1002, 1002, 1051, 1051, 1051, 1051, 3001, 3001, 1113, 1113, 1113, 1113, 1002, 1002, 1002, 1002 };  //
    List<int> enemyDeck = new List<int>() { 3001, 3001, 1002, 1002, 1051, 1051, 1051, 1051, 3001, 3001, 1113, 1113, 1113, 1113, 1002, 1002, 1002, 1002 };  //

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {

        playerLoaPoint = 0;
        enemyLoaPoint = 0;
        ShowLoaPoint();

        // DeckManager からデッキを受け取る
        if (DeckManager.Instance != null)
        {
            deck = new List<int>(DeckManager.Instance.selectedPlayerDeck);
            enemyDeck = new List<int>(DeckManager.Instance.selectedEnemyDeck);
        }

        // 初期手札を配る
        //SetStartHand();
        // 🔽 初期手札を受け取って配る
        foreach (int cardId in DeckManager.Instance.playerInitialHand)
        {
            CreateCard(cardId, playerHand);
        }
        foreach (int cardId in DeckManager.Instance.enemyInitialHand)
        {
            CreateCard(cardId, enemyHand);
        }

        // ターンの決定
        TurnCalc();
    }
    /**
        void StartGame() {
              CreateCard(3001, playerHand);
              CreateCard(1002, playerHand);
              CreateCard(1051, playerHand);
              CreateCard(1113, playerHand);
              CreateCard(3001, playerHand);
              CreateCard(3001, enemyHand);
              CreateCard(1051, enemyHand);
              CreateCard(1051, enemyHand);
              CreateCard(1113, enemyHand);
              CreateCard(1002, enemyHand);
        }
        */

    void ShowLoaPoint()
    {
        playerLoaPointText.text = playerLoaPoint.ToString();
        enemyLoaPointText.text = enemyLoaPoint.ToString();
    }

    void CreateCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        card.Init(cardID);
    }

    void DrawCard(Transform hand)
    {
        // デッキがないなら引かない
        if (deck.Count == 0)
        {
            return;
        }

        // デッキの一番上のカードを抜き取り、手札に加える
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }
    void EnemyDrawCard(Transform hand)
    {
        // デッキがないなら引かない
        if (enemyDeck.Count == 0)
        {
            return;
        }

        // デッキの一番上のカードを抜き取り、手札に加える
        int cardID = enemyDeck[0];
        enemyDeck.RemoveAt(0);
        CreateCard(cardID, hand);
    }

    void SetStartHand()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard(playerHand);
        }
        for (int i = 0; i < 3; i++)
        {
            EnemyDrawCard(enemyHand);
        }
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

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする
        TurnCalc(); // ターンを相手に回す
    }

    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
        DrawCard(playerHand); // 手札を一枚加える
    }

    void EnemyTurn()
    {
        Debug.Log("Enemyのターン");
        //CreateCard(1, enemyField); // カードを召喚
        //ChangeTurn(); // ターンエンドする
        EnemyDrawCard(enemyHand);
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
