using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeController : MonoBehaviour
{
    public static ThemeController Instance;
    [SerializeField] private MeshRenderer tableMr;
    [SerializeField] private MeshRenderer spawnerMr;
    
    [SerializeField] private MeshRenderer[] tabsMr;
    private void Awake()
    {
        Instance = this;
    }
    public void SetupTheme(int currentTheme)
    {
        if (BoardController.Instance == null) return;
        ThemeData themeData = BoardController.Instance.themeData;
        if (themeData != null && currentTheme >= 0 && currentTheme < themeData.themes.Count)
        {
            Theme selectedTheme = themeData.themes[currentTheme];
            if (selectedTheme != null)
            {
               // tableMr.material = selectedTheme.tableMaterial;
                spawnerMr.material = selectedTheme.spawnerMaterial;

                foreach (var item in tabsMr)
                {
                    item.material = selectedTheme.tableMaterial;
                }
            }
        }
    }
}
