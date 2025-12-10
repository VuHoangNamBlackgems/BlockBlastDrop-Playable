using DG.Tweening;
using UnityEngine;

public class GridSquare : MonoBehaviour
{
    [SerializeField] Transform Square;
    [SerializeField] MeshRenderer ColorSquare;
    [SerializeField] Vector3 squareSelectedScale;
    [SerializeField] MeshRenderer GridMr, TopGridMr;

    public bool onBlock = false;
    private bool onSelected;

    // Cache layer
    private static int LAYER_DEFAULT = -1;
    private static int LAYER_CUBE = -1;

    // Cache Component
    private ColorCarrier cachedCarrier;

    void Awake()
    {
        if (LAYER_DEFAULT == -1)
        {
            LAYER_DEFAULT = LayerMask.NameToLayer("Default");
            LAYER_CUBE = LayerMask.NameToLayer("Cube");
        }
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
                GridMr.material = selectedTheme.gridMaterial;
                TopGridMr.material = selectedTheme.topGridMaterial;
            }
        }
    }

    public void SetScaleSquare(bool _onSelected)
    {
        if (onBlock) return;

        if (onSelected == _onSelected)
            return;

        onSelected = _onSelected;

        Square.transform.DOKill();
        Square.transform.DOScale(onSelected ? squareSelectedScale : Vector3.one, 0.2f);

        ColorSquare.gameObject.SetActive(onSelected);
        ColorSquare.transform.DOKill();
        ColorSquare.transform.DOScale(onSelected ? squareSelectedScale : Vector3.one, 0.2f);
    }

    public void SetColorSquare()
    {
        if (onBlock || cachedCarrier == null) return;

        Material currentColor = GameConfig.Instance.GetColorGround(cachedCarrier.colorId);

        if (ColorSquare.material != currentColor)
        {
            ColorSquare.material = currentColor;
        }
    }
/*
    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;

        if (layer == LAYER_DEFAULT) return;

        if (layer == LAYER_CUBE)
        {
            onBlock = true;
            return;
        }

        cachedCarrier = other.GetComponent<ColorCarrier>();
        SetScaleSquare(true);
        SetColorSquare();
    }

    private void OnTriggerStay(Collider other)
    {
        int layer = other.gameObject.layer;

        if (layer == LAYER_DEFAULT) return;

        if (layer == LAYER_CUBE)
        {
            onBlock = true;
            return;
        }

        SetScaleSquare(true);
        SetColorSquare();
    }

    private void OnTriggerExit(Collider other)
    {
        int layer = other.gameObject.layer;

        if (layer == LAYER_DEFAULT) return;

        if (layer == LAYER_CUBE)
        {
            onBlock = false;
            return;
        }

        cachedCarrier = null;
        SetScaleSquare(false);
    }*/

    private void OnDestroy()
    {
        Square.transform.DOKill();
        ColorSquare.transform.DOKill();
    }
}
