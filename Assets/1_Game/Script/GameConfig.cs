using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance;
    [SerializeField] private Material[] colorEnemyList;
    [SerializeField] private Material[] colorCubeList;
    [SerializeField] private Material[] colorCanonList;
    [SerializeField] private Material[] colorHoleList;
    [SerializeField] private Material[] colorBloodList;
    [SerializeField] private Material[] colorGroundList;


    void Awake()
    {
        Instance = this;
    }

    public int GetColorCount()
    {
        return colorCubeList.Length;
    }
    public Material GetColorHole(int colorId)
    {
        if (colorId < 0 || colorId >= colorHoleList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorHoleList[colorId];
    }
    public Material GetColorEnemy(int colorId)
    {
        if (colorId < 0 || colorId >= colorEnemyList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorEnemyList[colorId];
    }
    public Material GetColorCube(int colorId)
    {
        if (colorId < 0 || colorId >= colorCubeList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorCubeList[colorId];
    }
    public Material GetColorCanon(int colorId)
    {
        if (colorId < 0 || colorId >= colorCanonList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorCanonList[colorId];
    }
    public Material GetColorBlood(int colorId)
    {
        if (colorId < 0 || colorId >= colorBloodList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorBloodList[colorId];
    }
    public Material GetColorGround(int colorId)
    {
        if (colorId < 0 || colorId >= colorGroundList.Length)
        {
            Debug.LogError("Invalid colorId: " + colorId);
            return null;
        }
        return colorGroundList[colorId];
    }

    public string GetColorName(int id)
    {
        string a = "";
        switch (id)
        {
            case 0:
                a = "Green";
                break;
            case 1:
                a = "Blue";
                break;
            case 2:
                a = "Red";
                break;
            case 3:
                a = "Charcoal";
                break;
            case 4:
                a = "Orange";
                break;
            case 5:
                a = "Pink";
                break;
            case 6:
                a = "Purple";
                break;
            case 7:
                a = "Yellow";
                break;
            default:
                Debug.Log("Unknown ColorId: " + id);
                break;
        }
        return a;
    }

}

[Serializable]
public enum ColorId
{
    Green = 0,
    Blue = 1,
    Red = 2,
    Charcoal = 3,
    Orange = 4,
    Pink = 5,
    Purple = 6,
    Yellow = 7,
    Black = 8
}
[Serializable]
public enum DifficultyType
{
    Easy = 3,
    Medium = 16,
    Hard = 36
}
[Serializable]
public enum ThemeType
{
    Theme1 = 0,
    Theme2 = 1,
    Theme3 = 2,
    Theme4 = 3,
    Theme5 = 4,
    Theme6 = 5
}

