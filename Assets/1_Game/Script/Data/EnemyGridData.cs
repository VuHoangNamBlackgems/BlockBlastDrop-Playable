using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCell
{
    // -1 = trống (đen), >= 0 = index trong colorPalette
    public int colorId = -1;

    [Tooltip("Ô này có chứa bomb không")]
    public bool isBomb = false;

    [Tooltip("Thời gian (giây) để bomb phát nổ")]
    public float bombTimer = 10f;

    public bool IsEmpty => colorId == -1;
}

[CreateAssetMenu(fileName = "EnemyGridData", menuName = "DataAsset/EnemyGridData")]
public class EnemyGridData : ScriptableObject
{
    [Header("Color Material")]
    public Material[] colorMaterials;

    [HideInInspector] public Color[] colorPalette = new Color[] { };

    [Header("Enemy Levels")]
    public List<EnemyLevelData> levels = new List<EnemyLevelData>();

    private void OnValidate()
    {
        // Tự động cập nhật colorPalette từ colorMaterials
        UpdateColorPaletteFromMaterials();

        // Đảm bảo mọi level có kích thước và mảng cell hợp lệ
        if (levels == null) return;
        foreach (var lv in levels)
        {
            if (lv == null) continue;
            lv.EnsureSize();
            lv.ClampCellsToPalette(colorMaterials != null ? colorMaterials.Length : 0);
        }
    }

    /// <summary>
    /// Cập nhật colorPalette dựa trên màu chính của các material trong colorMaterials
    /// </summary>
    public void UpdateColorPaletteFromMaterials()
    {
        if (colorMaterials == null || colorMaterials.Length == 0)
        {
            Debug.LogWarning("colorMaterials is null or empty. colorPalette will not be updated.");
            return;
        }

        // Tạo array mới với kích thước bằng colorMaterials
        colorPalette = new Color[colorMaterials.Length];

        for (int i = 0; i < colorMaterials.Length; i++)
        {
            if (colorMaterials[i] != null)
            {
                // Lấy màu chính từ material (_Color hoặc _BaseColor)
                Color materialColor = GetColorFromMaterial(colorMaterials[i]);
                colorPalette[i] = materialColor;
            }
            else
            {
                // Fallback color nếu material null
                colorPalette[i] = Color.white;
                Debug.LogWarning($"Material at index {i} is null. Using white as fallback color.");
            }
        }

        Debug.Log($"Updated colorPalette with {colorPalette.Length} colors from materials.");
    }

    /// <summary>
    /// Lấy màu chính từ material (hỗ trợ các shader property phổ biến)
    /// </summary>
    private Color GetColorFromMaterial(Material material)
    {
        if (material == null) return Color.white;

        // Thử các property name phổ biến cho màu chính
        string[] colorPropertyNames = { "_Color", "_BaseColor", "_MainColor", "_Albedo" };

        foreach (string propertyName in colorPropertyNames)
        {
            if (material.HasProperty(propertyName))
            {
                return material.GetColor(propertyName);
            }
        }

        // Nếu không tìm thấy property màu nào, sử dụng màu mặc định
        Debug.LogWarning($"Material '{material.name}' doesn't have standard color properties. Using white as fallback.");
        return Color.white;
    }
}

[System.Serializable]
public class EnemyLevelData
{
    [Header("Grid Settings")]
    [Min(1)] public int gridWidth = 10;
    [Min(1)] public int gridHeight = 12;
    public GridCell[] gridCells;
    public void EnsureSize()
    {
        int target = Mathf.Max(1, gridWidth) * Mathf.Max(1, gridHeight);
        if (gridCells == null || gridCells.Length != target)
        {
            var old = gridCells;
            gridCells = new GridCell[target];
            for (int i = 0; i < target; i++)
            {
                if (old != null && i < old.Length && old[i] != null)
                {
                    gridCells[i] = old[i];
                }
                else
                {
                    gridCells[i] = new GridCell { colorId = -1 };
                }
            }
        }
    }

    /// <summary>
    /// Lấy cell theo (x,y).
    /// </summary>
    public GridCell GetCell(int x, int y)
    {
        EnsureSize();
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return null;
        return gridCells[y * gridWidth + x];
    }

    /// <summary>
    /// Đặt màu cho cell (x,y).
    /// -1 = trống (đen), >= 0 = index trong palette.
    /// </summary>
    public void SetCellColor(int x, int y, int colorId)
    {
        var c = GetCell(x, y);
        if (c != null) c.colorId = colorId;
    }

    /// <summary>
    /// Xóa (đặt trống) toàn bộ grid.
    /// </summary>
    public void ClearAll()
    {
        EnsureSize();
        for (int i = 0; i < gridCells.Length; i++)
        {
            gridCells[i].colorId = -1;
        }
    }

    /// <summary>
    /// Điền toàn bộ grid bằng colorId (có thể là -1).
    /// </summary>
    public void FillAll(int colorId)
    {
        EnsureSize();
        for (int i = 0; i < gridCells.Length; i++)
        {
            gridCells[i].colorId = colorId;
        }
    }

    /// <summary>
    /// Giới hạn colorId theo kích thước palette (tránh index out of range
    /// khi palette bị rút gọn).
    /// </summary>
    public void ClampCellsToPalette(int paletteSize)
    {
        EnsureSize();
        for (int i = 0; i < gridCells.Length; i++)
        {
            if (gridCells[i] == null) gridCells[i] = new GridCell();
            if (gridCells[i].colorId >= paletteSize)
            {
                gridCells[i].colorId = -1; // đưa về trống nếu vượt palette
            }
        }
    }
}
