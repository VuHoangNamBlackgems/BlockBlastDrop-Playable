using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCube : MonoBehaviour
{
    private Grid grid;

    private void Awake()
    {
        grid = transform.parent.parent.GetComponentInChildren<Grid>();
    }

    private void Start()
    {
        TrySnapToGrid();
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
            snapZ + grid.transform.position.z
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
