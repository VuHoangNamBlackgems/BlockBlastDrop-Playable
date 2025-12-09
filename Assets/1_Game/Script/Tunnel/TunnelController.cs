using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TunnelController : MonoBehaviour
{
    [SerializeField] private FontController txtRemainCube;
    [SerializeField] private Cube cubePrefab;
    [SerializeField] GameObject colliderBlock;
    public SpawnDir SpawnDir;
    public List<ColorId> CubeColors = new List<ColorId>();
    private bool tunnelReady = false;
    private int nextColorIndex = 0;
    private Grid grid;
    private Vector3 targetPos;
    private List<int> RotateTunnelY = new List<int>() { 0, 180, -90, 90 };
    private List<int> RotateTextY = new List<int>() { 0, 180, 90, -90 };
    private List<Vector3> PosText = new List<Vector3>() {
        new Vector3(0.106f, 2.12f, -0.264f),
        new Vector3(-0.102f, 2.12f, -0.219f),
        new Vector3(-0.027f, 2.12f, -0.321f),
        new Vector3(0, 2.12f, -0.147f) };
    private void Awake()
    {
        grid = transform.parent.parent.GetComponentInChildren<Grid>();
        tunnelReady = false;
    }
    public void Start()
    {
        txtRemainCube.ShowText(CubeColors.Count);
        InitTunnel();
        TrySnapToGrid();
    }

    private void Update()
    {
        if (tunnelReady && CubeColors.Count > 0 && FreeToSpawn())
        {
            SpawnCube();
            txtRemainCube.ShowText(CubeColors.Count);
        }
    }
    private bool FreeToSpawn()
    {
        if (grid == null) return false;

        int idx = grid.GetNearestGridIndex(targetPos, grid.checkRadius);

        if (idx < 0) return false;

        int colorId = grid.GetGridSquareColor(idx);
        return colorId == -1;
    }

    public void InitTunnel()
    {
        switch (SpawnDir)
        {
            case SpawnDir.Left:
                targetPos = transform.position - new Vector3(1.836f, 0, 0) * grid.levelScale;
                break;
            case SpawnDir.Right:
                targetPos = transform.position + new Vector3(1.836f, 0, 0) * grid.levelScale;
                break;
            case SpawnDir.Up:
                targetPos = transform.position + new Vector3(0, 0, 1.836f) * grid.levelScale;
                break;
            case SpawnDir.Down:
                targetPos = transform.position - new Vector3(0, 0, 1.836f) * grid.levelScale;
                break;
        }
        transform.rotation = Quaternion.Euler(0, RotateTunnelY[(int)SpawnDir], 0);
        txtRemainCube.transform.localPosition = PosText[(int)SpawnDir];
        txtRemainCube.transform.localRotation = Quaternion.Euler(0, RotateTextY[(int)SpawnDir], 0);
        colliderBlock.SetActive(false);
    }
    public void SpawnCube()
    {
        tunnelReady = false;
        colliderBlock.SetActive(true);
        Cube cube = Instantiate(cubePrefab, transform.position, Quaternion.identity, transform.parent);
        cube.transform.localScale = Vector3.zero;
        cube.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f);
        cube.transform.DOMove(targetPos, 0.35f).SetEase(Ease.OutQuad).onComplete += () =>
        {
            cube.TrySnapToGrid(0.5f * grid.levelScale);
            tunnelReady = true;
            colliderBlock.SetActive(false);
            grid.FillListCube();
            if (CubeColors.Count <= 0)
            {
                Destroy(gameObject);
            }
        };
        if (CubeColors != null && CubeColors.Count > 0)
        {
            nextColorIndex = Mathf.Clamp(nextColorIndex, 0, CubeColors.Count - 1);

            int colorIndexToUse = nextColorIndex;
            cube.color = CubeColors[colorIndexToUse];

            CubeColors.RemoveAt(colorIndexToUse);


            if (CubeColors.Count == 0)
            {
                nextColorIndex = 0;
            }
            else if (colorIndexToUse >= CubeColors.Count)
            {
                nextColorIndex = 0;
            }
            else
            {
                nextColorIndex = colorIndexToUse;
            }
        }
        else
        {
            Debug.LogError("TunnelController: No more colors to spawn!");
        }

    }
    private float offsetX = 0;
    private float offsetZ = 0;
    public void TrySnapToGrid()
    {
        if (grid == null) return;

        Vector3 localPos = transform.position - grid.transform.position;
        float squareSize = grid.squareGap + grid.squareScale;

        CalculatorOffset(squareSize);

        float snapX = Mathf.Round((localPos.x - offsetX) / squareSize) * squareSize + offsetX;
        float snapZ = Mathf.Round((localPos.z - offsetZ) / squareSize) * squareSize + offsetZ;

        Vector3 snappedPos = new Vector3(
            snapX + grid.transform.position.x,
            transform.position.y,
            snapZ + grid.transform.position.z - 0.13f
        );

        transform.DOMove(snappedPos, 0.15f).SetEase(Ease.OutBack);
        DOVirtual.DelayedCall(0.2f, () => tunnelReady = true);
    }
    private void CalculatorOffset(float squareSize)
    {
        // bảng parity riêng cho Cube (giữ như bản bạn đang dùng)
        if (grid.numberRows % 2 != 0 && grid.numberColumns % 2 != 0)
        {
            offsetZ = 0;
            offsetX = 0;
        }
        else if (grid.numberRows % 2 == 0 && grid.numberColumns % 2 != 0)
        {
            offsetZ = squareSize * 0.5f;
            offsetX = 0;
        }
        else if (grid.numberRows % 2 != 0 && grid.numberColumns % 2 == 0)
        {
            offsetZ = 0;
            offsetX = squareSize * 0.5f;
        }
        else // cả hàng & cột chẵn
        {
            offsetZ = squareSize * 0.5f;
            offsetX = squareSize * 0.5f;
        }
    }

}
public enum SpawnDir
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3

}
