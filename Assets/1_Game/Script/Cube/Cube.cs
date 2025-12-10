using DG.Tweening;
using System;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public ColorId color;
    [SerializeField] FaceEmoji faceEmoji;
    [SerializeField] FontController text;

    public MeshRenderer meshRenderer;
    public BoxCollider boxCollider;
    public Grid grid;

    BoardController boardController => BoardController.Instance;
    private void Awake()
    {
        grid = transform.parent.parent.GetComponentInChildren<Grid>();
    }

    private void Start()
    {
        InitialSetup();
        TrySnapToGrid();
    }

    private void InitialSetup()
    {
        text.ShowText(10);
        gameObject.tag = color.ToString();
        var mr = transform.GetChild(0).GetComponent<MeshRenderer>();
        meshRenderer.material = GameConfig.Instance.GetColorCube((int)color);
        faceEmoji.PlayRandomEmoji();
    }

    public void OnPickedUp(Transform target, Shape shape, Action ondone = null)
    {
     //   Vibration.Vibrate(5);
        transform.parent = target;
        faceEmoji.Play("Pickup");

        transform.DORotate(new Vector3(40, 0, 0), 0.2f);
        transform.DOMove(transform.parent.position + 2 * Vector3.up, 0.2f);
        grid.OnBreakIce();

        DOVirtual.DelayedCall(0.15f, () =>
        {
            boardController.PlayCombo();
            boardController.SpawnCanon((int)color);
            grid.UpdateVisualGridSquare();
            transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
            {
                DestroyImmediate(gameObject);
                grid.FillListCube();
                if (ondone != null)
                    ondone.Invoke();
            });
            transform.DOMove(transform.parent.position + 5f * Vector3.down, 0.75f);
            shape.UpdateNumberCubePickup();
            if (shape.numberCubePickup <= 0)
            {
                shape.transform.DOScale(Vector3.zero, 0.75f).SetDelay(0.25f).OnComplete(() =>
                {
                    grid.UpdateVisualGridSquare();
                    DestroyImmediate(shape.gameObject);
                });
            }
        });
    }

    private float offsetX = 0;
    private float offsetZ = 0;

    public void TrySnapToGrid(float offsetY = 0)
    {
        if (grid == null) return;

        Vector3 localPos = transform.position - grid.transform.position;
        float squareSize = grid.squareGap + grid.squareScale;

        CalculatorOffset(squareSize);

        float snapX = Mathf.Round((localPos.x - offsetX) / squareSize) * squareSize + offsetX;
        float snapZ = Mathf.Round((localPos.z - offsetZ) / squareSize) * squareSize + offsetZ;

        Vector3 snappedPos = new Vector3(
            snapX + grid.transform.position.x,
            transform.position.y - offsetY,
            snapZ + grid.transform.position.z - 0.1f
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

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
