#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;

public class SetCardEntitiesToAddressables
{
    [MenuItem("Tools/Addressables/Mark All CardEntity as Addressable")]
    public static void MarkAllCardEntitiesAsAddressable()
    {
        string targetFolder = "Assets/Resources_moved/CardEntityList";
        string labelName = "CardEntityList";

        // Addressables 設定の取得
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables settings not found. Create Addressables Settings first.");
            return;
        }

        // ラベルがなければ追加
        if (!settings.GetLabels().Contains(labelName))
        {
            settings.AddLabel(labelName);
        }

        // ファイル一覧取得
        string[] guids = AssetDatabase.FindAssets("t:CardEntity", new[] { targetFolder });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            // アドレス名（例: "Card_1001"）を決定
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // 既存のアドレスを確認し、なければ追加
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = assetName;

            // ラベルを追加
            if (!entry.labels.Contains(labelName))
            {
                entry.SetLabel(labelName, true);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("すべての CardEntity を Addressables に設定し、ラベルを付けました。");
    }
}
#endif
