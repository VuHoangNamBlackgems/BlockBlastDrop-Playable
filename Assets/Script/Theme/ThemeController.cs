using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeController : MonoBehaviour
{
    public static ThemeController Instance;
    [SerializeField] public MeshRenderer tableMr;
    [SerializeField] public MeshRenderer spawnerMr;
    [SerializeField] public GameObject VFX;
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
                tableMr.material = selectedTheme.tableMaterial;
                spawnerMr.material = selectedTheme.spawnerMaterial;

                // Quản lý VFX children
                if (VFX != null && VFX.transform.childCount > 0)
                {
                    int vfxTheme = currentTheme % VFX.transform.childCount;
                    for (int i = 0; i < VFX.transform.childCount; i++)
                    {
                        VFX.transform.GetChild(i).gameObject.SetActive(i == vfxTheme);
                    }
                }
            }
        }
    }
}
