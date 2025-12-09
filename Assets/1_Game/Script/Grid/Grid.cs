using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Design Mode")]
    public float levelScale = 1;
    public bool AutoInstantiate = true;
    public bool AutoSetPosition = true;

    [Header("Grid Size")]
    public int numberColumns = 0;
    public int numberRows = 0;

    [Header("Prefabs & Layout")]
    public GameObject gridSquare;
    public float squareGap = 1.7f;
    public float squareScale = 1f;

    [Header("Detection & Gizmos")]
    public float checkRadius = 0.4f;
    public Color gizmoEmptyColor = Color.gray;
    public Color gizmoFilledColor = Color.green;

    [Header("Preview (Tint)")]
    [SerializeField] private Color previewValidColor = new Color(0f, 1f, 1f, 0.8f);
    [SerializeField] private Color previewInvalidColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private bool tintSquares = true;

    private readonly List<GameObject> _gridSquares = new List<GameObject>();
    private List<int> _gridSquareColors = new List<int>();

    // Preview cache
    private readonly List<int> previewIndices = new List<int>();
    private readonly List<Renderer> _squareRenderers = new List<Renderer>();
    private MaterialPropertyBlock _mpb;

    // Public accessors
    public int GetGridSquareCount() => _gridSquares != null ? _gridSquares.Count : 0;

    public Vector3 GetGridSquarePosition(int index)
    {
        if (_gridSquares == null || index < 0 || index >= _gridSquares.Count) return Vector3.zero;
        return _gridSquares[index].transform.position;
    }

    public int GetGridSquareColor(int index)
    {
        if (_gridSquareColors == null || index < 0 || index >= _gridSquareColors.Count) return -1;
        return _gridSquareColors[index];
    }

    public int GetNearestGridIndex(Vector3 worldPos, float threshold)
    {
        if (_gridSquares == null || _gridSquares.Count == 0) return -1;

        int bestIndex = -1;
        float bestDistSqr = float.MaxValue;

        for (int i = 0; i < _gridSquares.Count; i++)
        {
            var gs = _gridSquares[i];
            if (gs == null) continue;
            float d = (gs.transform.position - worldPos).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                bestIndex = i;
            }
        }

        if (bestIndex < 0) return -1;
        float threshSqr = threshold * threshold;
        return bestDistSqr <= threshSqr ? bestIndex : -1;
    }

    private void Awake()
    {
        squareGap *= levelScale;
        squareScale *= levelScale;
        checkRadius *= levelScale;
        // listBoosterCanUse.Add(BoosterType.ClearCube);
    }

    void Start()
    {
        SetBorderTheme();
        if (AutoInstantiate)
            SpawnGridSquares();
        else
            SetGridSquares();
        if (AutoSetPosition)
            SetGridSquaresPosition();
        CacheSquareRenderers();
        FillListCube();
        InitLsFrozen();
        InitLsShapes();

    }
    private void SetBorderTheme()
    {
       /* Material borderMaterial = null;
        int curentTheme = BoardController.Instance.currentTheme;
        ThemeData themeData = BoardController.Instance.themeData;
        if (themeData != null && curentTheme >= 0 && curentTheme < themeData.themes.Count)
        {
            Theme selectedTheme = themeData.themes[curentTheme];
            if (selectedTheme != null)
            {
                borderMaterial = selectedTheme.borderMaterial;
            }
        }
        GameObject borderHolder = transform.parent.Find("Border")?.gameObject;
        if (borderHolder != null)
        {
            foreach (Transform borderTransform in borderHolder.transform)
            {
                GameObject rd = borderTransform.GetChild(0).gameObject;
                MeshRenderer mr = rd.GetComponent<MeshRenderer>();
                if (mr != null && borderMaterial != null)
                {
                    mr.material = borderMaterial;
                }
                else
                {
                    Debug.LogWarning("MeshRenderer or borderMaterial is null for border: " + borderTransform.name);
                    mr.material = null;
                }
            }
        }*/
    }
    public List<Cube> listCube = new List<Cube>();
    public List<Shape> lsShapes = new List<Shape>();
    public List<FrozenController> lsFrozen = new List<FrozenController>();
    private void InitLsFrozen()
    {
        lsFrozen.Clear();
        var frozenInChildren = transform.parent.GetComponentsInChildren<FrozenController>();
        foreach (var frozen in frozenInChildren)
        {
            lsFrozen.Add(frozen);
        }
    }
    private void InitLsShapes()
    {
        lsShapes.Clear();
        var shapesInChildren = transform.parent.GetComponentsInChildren<Shape>();
        foreach (var shape in shapesInChildren)
        {
            lsShapes.Add(shape);

        }
    }
    public void RemoveFrozenFromList(FrozenController frozen)
    {
        if (lsFrozen.Contains(frozen))
        {
            lsFrozen.Remove(frozen);
        }
        FillListCube();
    }
    public void OnBreakIce()
    {
        foreach (var frozen in lsFrozen)
        {
            frozen.OnBreakIce();
        }
    }

    public void FillListCube()
    {
        listCube.Clear();
        var listCubeInChildren = transform.parent.GetComponentsInChildren<Cube>();
        foreach (var cube in listCubeInChildren)
        {
            listCube.Add(cube);
        }
    }

    private void SpawnGridSquares()
    {
        _gridSquareColors.Clear();
        _gridSquares.Clear();

        for (int row = 0; row < numberRows; ++row)
        {
            for (int col = 0; col < numberColumns; ++col)
            {
                var go = Instantiate(gridSquare, transform);
                go.transform.localScale = new Vector3(squareScale, squareScale, squareScale);

                _gridSquares.Add(go);
                _gridSquareColors.Add(-1);

            
            }
        }
    }

    private void SetGridSquaresPosition()
    {
        int column_index = 0;
        int row_index = 0;

        var first = _gridSquares[0];
        float squareWidth = first.transform.localScale.x;
        float squareHeight = first.transform.localScale.z;

        float totalWidth = numberColumns * squareWidth + (numberColumns - 1) * squareGap;
        float totalHeight = numberRows * squareHeight + (numberRows - 1) * squareGap;

        Vector3 centeredStart = new Vector3(
            -totalWidth / 2 + squareWidth / 2,
            0,
            totalHeight / 2 - squareHeight / 2
        );

        foreach (GameObject square in _gridSquares)
        {
            float pos_x = centeredStart.x + column_index * (squareWidth + squareGap);
            float pos_z = centeredStart.z - row_index * (squareHeight + squareGap);

            square.transform.localPosition = new Vector3(pos_x, 0, pos_z);

            column_index++;
            if (column_index >= numberColumns)
            {
                column_index = 0;
                row_index++;
            }
        }
    }

    private void SetGridSquares()
    {
        _gridSquareColors.Clear();
        _gridSquares.Clear();

        foreach (Transform child in transform)
        {
            var go = child.gameObject;
            go.transform.localScale = new Vector3(squareScale, squareScale, squareScale);
            _gridSquares.Add(go);
            _gridSquareColors.Add(-1);
        }
    }
    private void CacheSquareRenderers()
    {
        _squareRenderers.Clear();
        foreach (var sq in _gridSquares)
        {
            _squareRenderers.Add(sq ? sq.GetComponentInChildren<Renderer>() : null);
        }
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
    }

    #region Visual
    public void UpdateVisualGridSquare()
    {
        foreach (GameObject _gridSquare in _gridSquares)
        {
            GridSquare gridSquare = _gridSquare.GetComponent<GridSquare>();
            gridSquare.onBlock = false;
            gridSquare.SetScaleSquare(false);
        }
    }
    #endregion

    #region Preview

    public void ShowPreview(IList<int> indices, bool valid)
    {
        ClearPreview();
        if (indices == null) return;

        previewIndices.AddRange(indices);

        if (!tintSquares) return;

        for (int i = 0; i < previewIndices.Count; i++)
        {
            int idx = previewIndices[i];
            if (idx < 0 || idx >= _squareRenderers.Count) continue;
            SetRendererTint(_squareRenderers[idx], valid ? previewValidColor : previewInvalidColor);
        }
    }

    private void SetRendererTint(Renderer r, Color c)
    {
        if (!r) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c);
        _mpb.SetColor("_Color", c);
        r.SetPropertyBlock(_mpb);
    }

    public void ClearPreview()
    {
        if (tintSquares)
        {
            for (int i = 0; i < previewIndices.Count; i++)
            {
                int idx = previewIndices[i];
                if (idx < 0 || idx >= _squareRenderers.Count) continue;
                ClearRendererTint(_squareRenderers[idx]);
            }
        }
        previewIndices.Clear();
    }

    private void ClearRendererTint(Renderer r)
    {
        if (!r) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        _mpb.Clear();
        r.SetPropertyBlock(_mpb);
    }

    #endregion

    public void Update()
    {
        UpdateGridSquareColors();
    }

    // Quét ColorCarrier hoặc Cube tại tâm từng cell để lấy màu hiện có
    public void UpdateGridSquareColors()
    {
        if (_gridSquareColors.Count != _gridSquares.Count)
        {
            _gridSquareColors.Clear();
            for (int i = 0; i < _gridSquares.Count; i++) _gridSquareColors.Add(-1);
        }

        for (int i = 0; i < _gridSquares.Count; i++)
        {
            var square = _gridSquares[i];
            if (!square) { _gridSquareColors[i] = -1; continue; }

            Vector3 worldPos = square.transform.position;

            Collider[] hits = Physics.OverlapSphere(
                worldPos,
                checkRadius,
                ~0,
                QueryTriggerInteraction.Collide
            );

            int foundColor = -1;

            foreach (var col in hits)
            {
                if (col == null) continue;

                var carrier = col.GetComponent<ColorCarrier>() ?? col.GetComponentInChildren<ColorCarrier>();
                if (carrier != null)
                {
                    foundColor = carrier.colorId;
                    break;
                }

                var cube = col.GetComponent<Cube>() ?? col.GetComponentInChildren<Cube>();
                if (cube != null)
                {
                    foundColor = (int)cube.color;
                    break;
                }
            }

            _gridSquareColors[i] = foundColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (_gridSquares == null || _gridSquares.Count == 0) return;

        for (int i = 0; i < _gridSquares.Count; i++)
        {
            var square = _gridSquares[i];
            if (square == null) continue;
            Vector3 pos = square.transform.position;

            bool hasColor = (i < _gridSquareColors.Count && _gridSquareColors[i] >= 0);
            Gizmos.color = hasColor ? gizmoFilledColor : gizmoEmptyColor;
            Gizmos.DrawWireSphere(pos, checkRadius * 0.5f);
        }

        if (!tintSquares && previewIndices != null)
        {
            foreach (var idx in previewIndices)
            {
                if (idx < 0 || idx >= _gridSquares.Count) continue;
                Vector3 pos = _gridSquares[idx].transform.position;
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(pos, checkRadius * 0.5f);
            }
        }
    }
}
