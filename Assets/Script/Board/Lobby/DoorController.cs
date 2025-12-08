using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] public GameObject StandPosition;
    [SerializeField] public Animator doorAnimator;
    [SerializeField] public MeshRenderer borderDoor;
    [SerializeField] public SkinnedMeshRenderer gateMr;
    [SerializeField] private Material defaultBorderDoor;
    private void Awake()
    {

    }
    public void SetupTheme(int currentTheme)
    {
        if (BoardController.Instance == null)
            return;
        ThemeData themeData = BoardController.Instance.themeData;
        if (themeData != null && currentTheme >= 0 && currentTheme < themeData.themes.Count)
        {
            Theme selectedTheme = themeData.themes[currentTheme];
            if (selectedTheme != null)
            {
                gateMr.material = selectedTheme.gateMaterial;
                defaultBorderDoor = selectedTheme.borderHolderBaseMaterial;
                borderDoor.material = defaultBorderDoor;
            }
        }
    }
    public void ChangeBorderColor(ColorId colorId)
    {
        // Check if colorId is valid and within range
        if (colorId >= 0 && GameConfig.Instance != null && GameConfig.Instance.GetColorCount() > (int)colorId)
        {
            borderDoor.material = GameConfig.Instance.GetColorCanon((int)colorId);
        }
        else
        {
            // Use default color for invalid colorId or when no canon present
            borderDoor.material = defaultBorderDoor;
        }
    }
}
