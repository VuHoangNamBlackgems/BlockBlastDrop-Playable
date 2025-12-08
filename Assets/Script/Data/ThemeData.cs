using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "DataAsset/ThemeData")]
public class ThemeData : ScriptableObject
{
    public List<Theme> themes = new List<Theme>();
}
[System.Serializable]
public class Theme
{
    public string themeName;
    public int themeId;
    public Material tableMaterial, spawnerMaterial, borderHolderBaseMaterial, gateMaterial, gridMaterial, topGridMaterial, borderMaterial;
}
