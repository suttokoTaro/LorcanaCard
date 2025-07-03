using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    public void OnClickDeckBuilder()
    {
        SceneManager.LoadScene("DeckListScene");
    }

    public void OnClickBattle()
    {
        DeckManager.Instance.selectedPlayerDeck = new List<int> { 1002, 1002, 1051, 1051, 1113, 1113 }; // プレイヤーデッキ
        DeckManager.Instance.selectedEnemyDeck = new List<int> { 3001, 3001, 1113, 1113, 1051, 1051 }; // 敵デッキ

        SceneManager.LoadScene("BattleScene");
    }
}
