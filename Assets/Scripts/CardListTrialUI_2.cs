using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CardLoader : MonoBehaviour
{
    [SerializeField] private GameObject cardItemPrefab;
    [SerializeField] private Transform content;

    private async void Start()
    {
        // ラベルでCardEntityを非同期ロード
        AsyncOperationHandle<IList<CardEntity>> handle = Addressables.LoadAssetsAsync<CardEntity>("CardEntityList", null);
        IList<CardEntity> cardEntities = await handle.Task;

        // 非同期表示（バッチ処理）
        StartCoroutine(DisplayCardsAsync(cardEntities));

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("カードの読み込みに失敗しました: " + handle.OperationException);
        }
    }

    private IEnumerator DisplayCardsAsync(IList<CardEntity> cardEntities)
    {
        int batchSize = 30;

        for (int i = 0; i < cardEntities.Count; i++)
        {
            CardEntity cardEntity = cardEntities[i];

            GameObject item = Instantiate(cardItemPrefab, content);
            Image iconImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = cardEntity.icon;

            if ((i + 1) % batchSize == 0)
            {
                yield return null;
            }
        }
    }
}
