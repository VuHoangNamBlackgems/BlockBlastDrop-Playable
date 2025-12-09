using DG.Tweening;

using UnityEngine;

public class FrozenController : MonoBehaviour
{
    [SerializeField] private FontController txtRemainFrozen;
    [SerializeField] private Cube cubePrefab;
    [SerializeField] private GameObject cubeTemp;
    [SerializeField] private ParticleSystem iceBreakParticle;
    Vector3 originalScale;
    public ColorId CubeColor;
    public int FrozenRemain = 0;
    private Grid grid;
    private Camera mainCam;

    // Start is called before the first frame update
    private void Awake()
    {
        grid = transform.parent.parent.GetComponentInChildren<Grid>();
        mainCam = Camera.main;
        originalScale = transform.localScale;
    }
    void Start()
    {
        txtRemainFrozen.ShowText(FrozenRemain);
        TrySnapToGrid();
        InitFrozen();
    }
    public void OnBreakIce()
    {
        if (FrozenRemain <= 0) return;
        FrozenRemain--;
        if (FrozenRemain > 0)
        {
            txtRemainFrozen.ShowText(FrozenRemain);
            ScaleUpDownFrozen();
        }
        else
        {
            RemoveIce();
        }
    }
    private void RemoveIce()
    {
        iceBreakParticle.Play();
        Cube cube = Instantiate(cubePrefab, transform.position, Quaternion.identity, transform.parent);
        cube.color = CubeColor;

        cubeTemp.SetActive(false);

        txtRemainFrozen.HideText();

        var mr = transform.GetComponent<MeshRenderer>();
        if (mr) mr.enabled = false;
        var boxCollider = transform.GetComponent<BoxCollider>();
        if (boxCollider) boxCollider.enabled = false;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            DestroyImmediate(gameObject);
            grid.RemoveFrozenFromList(this);
        });
    }
    private void ScaleUpDownFrozen()
    {
        transform.DOKill();
        transform.DOScale(originalScale * 1.2f, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
        });
    }

    private void InitFrozen()
    {
        if (cubeTemp != null)
        {
            var mr = cubeTemp.GetComponent<MeshRenderer>();
            if (mr) mr.material = GameConfig.Instance.GetColorCube((int)CubeColor);
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

