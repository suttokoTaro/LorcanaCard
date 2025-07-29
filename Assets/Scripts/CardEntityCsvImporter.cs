using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CardEntityCsvImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import CardEntities from CSV")]
    public static void ShowWindow()
    {
        GetWindow<CardEntityCsvImporter>("CardEntity CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV ファイルを指定してください", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("インポート"))
        {
            if (csvFile != null)
            {
                ImportFromCsv(csvFile);
            }
            else
            {
                Debug.LogWarning("CSVファイルが指定されていません。");
            }
        }
    }

    private void ImportFromCsv(TextAsset csvTextAsset)
    {
        string[] lines = csvTextAsset.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1)
        {
            Debug.LogError("CSVにデータが含まれていません。");
            return;
        }

        string folderPath = "Assets/Resources/ToolTrial";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "ToolTrial");
        }

        for (int i = 1; i < lines.Length; i++) // 1行目はヘッダー
        {
            string[] tokens = lines[i].Split(',');

            CardEntity card = ScriptableObject.CreateInstance<CardEntity>();

            int index = 0;
            card.cardId = int.Parse(tokens[index++]);
            card.setSeries = tokens[index++];
            card.idInSetSeries = tokens[index++];
            card.icon = LoadIconSprite(card.setSeries, card.idInSetSeries);
            index++;
            card.backIcon = LoadBackIconSprite();
            index++;
            card.name = tokens[index++];
            card.versionName = tokens[index++];
            card.color = tokens[index++];
            card.cost = int.Parse(tokens[index++]);
            card.cardType = tokens[index++];
            card.classification = tokens[index++];
            card.willpower = int.Parse(tokens[index++]);
            card.strength = int.Parse(tokens[index++]);
            card.loreValue = int.Parse(tokens[index++]);
            card.rarity = tokens[index++];
            card.inkwellFlag = int.Parse(tokens[index++]);
            card.vanillaFlag = int.Parse(tokens[index++]);
            card.bodyguardFlag = int.Parse(tokens[index++]);
            card.challengerFlag = int.Parse(tokens[index++]);
            card.evasiveFlag = int.Parse(tokens[index++]);
            card.recklessFlag = int.Parse(tokens[index++]);
            card.resistFlag = int.Parse(tokens[index++]);
            card.rushFlag = int.Parse(tokens[index++]);
            card.shiftFlag = int.Parse(tokens[index++]);
            card.singerFlag = int.Parse(tokens[index++]);
            card.singTogetherFlag = int.Parse(tokens[index++]);
            card.supportFlag = int.Parse(tokens[index++]);
            card.wardFlag = int.Parse(tokens[index++]);
            card.effectText = tokens[index++];
            card.flavorText = tokens[index++];

            string assetPath = $"{folderPath}/Card_{card.cardId}.asset";
            AssetDatabase.CreateAsset(card, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSVからCardEntityをインポートしました。");
    }

    private Sprite LoadIconSprite(string setSeries, string idInSetSeries)
    {
        if (string.IsNullOrEmpty(setSeries)) return null;
        if (string.IsNullOrEmpty(idInSetSeries)) return null;

        string series = setSeries.Substring(0, 2);


        // 例: "CardIcons/icon001" → Resources/CardIcons/icon001.png
        Sprite sprite = Resources.Load<Sprite>($"CardEntityList/Card_images/{series}/{series}_{idInSetSeries}");
        if (sprite == null)
        {
            Debug.LogWarning("Sprite not found at path");
        }
        return sprite;
    }
    private Sprite LoadBackIconSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("CardEntityList/Card_images/CardBack/00_card_back");
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite not found");
        }
        return sprite;
    }
}
