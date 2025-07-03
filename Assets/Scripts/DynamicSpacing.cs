using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class DynamicSpacing : MonoBehaviour
{
    public RectTransform parentRectTransform; // 親のRectTransform
    public Vector2 cellSize = new Vector2(200, 280);
    public float defaultSpacing = 10f;
    public float minSpacing = 5f;

    private GridLayoutGroup grid;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        grid.childAlignment = TextAnchor.UpperLeft; // 左寄せに設定
        UpdateSpacing();
    }

    void OnTransformChildrenChanged()
    {
        UpdateSpacing(); // 子要素が増減したら再計算
    }

    void UpdateSpacing()
    {
        int elementCount = transform.childCount;
        float totalWidth = parentRectTransform.rect.width;

        if (elementCount <= 1)
        {
            grid.spacing = new Vector2(0, grid.spacing.y);
            return;
        }

        float totalCellWidth = elementCount * cellSize.x;
        float maxSpacing = (totalWidth - totalCellWidth) / (elementCount - 1);
        float spacing = Mathf.Clamp(defaultSpacing, minSpacing, maxSpacing);

        grid.cellSize = cellSize;
        grid.spacing = new Vector2(spacing, grid.spacing.y);
    }
}
