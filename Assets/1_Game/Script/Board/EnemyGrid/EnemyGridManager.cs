using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGridManager : MonoBehaviour
{
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private Transform gridParent;
    public Transform GridParent => gridParent;
    public List<int> firstRow = new List<int>();
    private int rows = 12;
    private int cols = 10;

    [SerializeField] private float spacingColumn = 1.45f;
    [SerializeField] private float spacingRow = 1.45f;

    private readonly Queue<EnemyController> pool = new Queue<EnemyController>(128);

    private readonly HashSet<int> compactingColumns = new HashSet<int>();
    private readonly Dictionary<int, EnemyController> predictedFront = new Dictionary<int, EnemyController>();

    private readonly Dictionary<int, Sequence> columnSequences = new Dictionary<int, Sequence>();

    private readonly HashSet<EnemyController> dying = new HashSet<EnemyController>();

    private readonly Dictionary<int, EnemyController> currentKilling = new Dictionary<int, EnemyController>();
    void Start()
    {
        ValidateEnemyGridData();
    }

    public bool IsAllEnemiesDestroyed()
    {
        if (gridParent == null) return true;

        PruneDying();

        for (int col = 0; col < gridParent.childCount; col++)
        {
            Transform columnParent = gridParent.GetChild(col);
            if (columnParent == null) continue;

            for (int j = 0; j < columnParent.childCount; j++)
            {
                var enemy = columnParent.GetChild(j).GetComponent<EnemyController>();
                if (enemy == null) continue;

                if (enemy.gameObject.activeInHierarchy
                    && !dying.Contains(enemy)
                    && enemy.transform.localScale.sqrMagnitude > 1e-6f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void CheckGameFinish()
    {
        if (IsAllEnemiesDestroyed())
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                LunaManager.instance.GoToStore();
            });
        }
    }

    private void ValidateEnemyGridData()
    {
        if (BoardController.Instance.enemyGridData == null) return;
        if (BoardController.Instance.enemyGridData.levels == null || BoardController.Instance.enemyGridData.levels.Count == 0) return;
    }


    private int ValidateColorId(int colorId)
    {
        if (BoardController.Instance.enemyGridData == null) return 0;
        var colorMaterials = BoardController.Instance.enemyGridData.colorMaterials;
        if (colorMaterials == null || colorMaterials.Length == 0) return 0;
        int maxColors = colorMaterials.Length;
        if (colorId < 0 || colorId >= maxColors) return 0;
        return colorId;
    }

    Transform[] columnParents;
    public void LoadEnemyGrid(int level)
    {
        ClearGrid();
        if (BoardController.Instance.enemyGridData == null ||
            BoardController.Instance.enemyGridData.levels == null ||
            level >= BoardController.Instance.enemyGridData.levels.Count)
        {
            return;
        }

        EnemyLevelData levelData = BoardController.Instance.enemyGridData.levels[level];
        if (levelData == null)
        {
            return;
        }

        cols = levelData.gridWidth;
        rows = levelData.gridHeight;

        float gridWidth = (cols - 1) * spacingColumn;
        Vector3 centerOffset = new Vector3(-gridWidth / 2f, 0, 0);

        if (gridParent.childCount == 0)
        {
            columnParents = new Transform[cols];
            for (int col = 0; col < cols; col++)
            {
                GameObject columnParent = new GameObject($"Column_{col}");
                columnParent.transform.SetParent(gridParent);
                columnParent.transform.localPosition = new Vector3(col * spacingColumn, 0, 0) + centerOffset;
                columnParents[col] = columnParent.transform;
            }
        }

        firstRow.Clear();
        for (int col = 0; col < cols; col++) firstRow.Add(-1);

        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                GridCell cell = levelData.GetCell(col, row);
                int colorId = (cell != null && !cell.IsEmpty) ? cell.colorId : -1;
                if (colorId >= 0)
                {
                    int validColorId = ValidateColorId(colorId);
                    SpawnEnemy(row, columnParents[col], validColorId, cell.isBomb, cell.bombTimer);
                    if (row == 0) firstRow[col] = validColorId;
                }
            }
    }

    public void ClearGrid()
    {
        foreach (var kv in columnSequences) kv.Value?.Kill(false);
        columnSequences.Clear();

        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                Transform col = gridParent.GetChild(i);
                for (int j = col.childCount - 1; j >= 0; j--)
                {
                    var ec = col.GetChild(j).GetComponent<EnemyController>();
                    if (ec != null)
                    {
                        ec.transform.SetParent(null);
                        ec.gameObject.SetActive(false);
                        pool.Enqueue(ec);
                    }
                    else Destroy(col.GetChild(j).gameObject);
                }
                //Destroy(col.gameObject);
            }
        }
        compactingColumns.Clear();
        predictedFront.Clear();
        dying.Clear();
        currentKilling.Clear();
    }

    private EnemyController GetEnemyFromPool(Transform parent)
    {
        EnemyController e;
        if (pool.Count > 0)
        {
            e = pool.Dequeue();
            e.transform.SetParent(parent, false);
            e.gameObject.SetActive(true);
        }
        else e = Instantiate(enemyPrefab, parent);
        return e;
    }

    private void SpawnEnemy(int row, Transform _columnParent, int colorId = 0, bool isBomb = false, float bombTimer = 10f)
    {
        int validColorId = ValidateColorId(colorId);
        EnemyController enemy = GetEnemyFromPool(_columnParent);
        enemy.transform.localPosition = new Vector3(0, 0, row * spacingRow);
        enemy.SetColor(validColorId);
        enemy.SetEmoji();
        enemy.SetInfo((int)(_columnParent.localPosition.x / spacingColumn + cols / 2f), row);
        enemy.ResetAllLocks(); // CRITICAL: Reset TOÀN BỘ locks cho enemy mới/pooled

        enemy.SetEmojiVisibility(row < 1);
    }

    private void PruneDying()
    {
        dying.RemoveWhere(ec => ec == null || !ec.gameObject);
    }

    private void KillColumnSequence(int columnIndex, bool complete = false)
    {
        if (columnSequences.TryGetValue(columnIndex, out var seq) && seq != null && seq.IsActive())
        {
            seq.Kill(complete);
        }
        columnSequences.Remove(columnIndex);
    }

    private List<EnemyController> GetAliveEnemiesSorted(int columnIndex, EnemyController exclude = null)
    {
        List<EnemyController> list = new List<EnemyController>();
        if (columnIndex < 0 || columnIndex >= cols || gridParent.childCount <= columnIndex) return list;

        Transform columnParent = gridParent.GetChild(columnIndex);

        currentKilling.TryGetValue(columnIndex, out var killingNow);

        for (int i = 0; i < columnParent.childCount; i++)
        {
            EnemyController e = columnParent.GetChild(i).GetComponent<EnemyController>();
            if (e == null) continue;
            if (e == exclude) continue;
            if (e == killingNow) continue;
            if (dying.Contains(e)) continue;
            list.Add(e);
        }
        list.Sort((a, b) => a.transform.localPosition.z.CompareTo(b.transform.localPosition.z));
        return list;
    }

    public GameObject GetEnemyByColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= cols || gridParent.childCount <= columnIndex) return null;

        if (compactingColumns.Contains(columnIndex) &&
            predictedFront.TryGetValue(columnIndex, out var pf) &&
            pf != null && pf.gameObject.activeInHierarchy && !dying.Contains(pf))
        {
            // Return predictedFront WITHOUT resetting aim
            // Aim will be reset AFTER compacting completes and it becomes the actual front
            return pf.gameObject;
        }

        Transform columnParent = gridParent.GetChild(columnIndex);
        if (columnParent == null || columnParent.childCount == 0) return null;

        EnemyController firstEnemy = null;
        float minZ = float.MaxValue;

        for (int j = 0; j < columnParent.childCount; j++)
        {
            EnemyController ec = columnParent.GetChild(j).GetComponent<EnemyController>();
            if (ec != null && !dying.Contains(ec) && ec.transform.localPosition.z < minZ)
            {
                minZ = ec.transform.localPosition.z;
                firstEnemy = ec;
            }
        }
        return firstEnemy ? firstEnemy.gameObject : null;
    }

    public void AsyncGrid()
    {
        for (int col = 0; col < cols && col < gridParent.childCount; col++)
        {
            StartCompactingNow(col, null);
        }
        // fill gap 

    }
    public bool DestroyFirstEnemyInColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= cols || gridParent.childCount <= columnIndex) return false;

        PruneDying();

        compactingColumns.Add(columnIndex);

        var aliveList = GetAliveEnemiesSorted(columnIndex);
        if (aliveList.Count == 0)
        {
            if (columnIndex < firstRow.Count) firstRow[columnIndex] = -1;

            currentKilling.Remove(columnIndex);
            predictedFront.Remove(columnIndex);
            compactingColumns.Remove(columnIndex);

            CheckGameFinish();
            return false;
        }

        var firstEnemy = aliveList[0];

        // CRITICAL: Check if this enemy is already being destroyed
        if (currentKilling.ContainsKey(columnIndex) && currentKilling[columnIndex] == firstEnemy)
        {
            // This enemy is already being destroyed by another bullet
            compactingColumns.Remove(columnIndex);
            return false;
        }

        // Double check if enemy is already dying or has been destroyed
        if (!firstEnemy.gameObject.activeInHierarchy || firstEnemy.transform.localScale.sqrMagnitude < 0.01f)
        {
            compactingColumns.Remove(columnIndex);
            return false;
        }

        var nextFront = (aliveList.Count > 1) ? aliveList[1] : null;

        if (columnIndex < firstRow.Count)
            firstRow[columnIndex] = nextFront ? nextFront.ColorId : -1;

        if (nextFront)
        {
            predictedFront[columnIndex] = nextFront;
            // Don't reset aim here - will be reset after compacting animation completes
        }
        else
        {
            predictedFront.Remove(columnIndex);
        }

        currentKilling[columnIndex] = firstEnemy;

        if (!dying.Contains(firstEnemy)) dying.Add(firstEnemy);

        firstEnemy.OnHit();

        KillColumnSequence(columnIndex,  false);

        StartCompactingNow(columnIndex, firstEnemy);

        return true;
    }

    private void StartCompactingNow(int columnIndex, EnemyController destroyingEnemy)
    {
        if (columnIndex < 0 || columnIndex >= cols || gridParent.childCount <= columnIndex)
        {
            compactingColumns.Remove(columnIndex);
            predictedFront.Remove(columnIndex);
            currentKilling.Remove(columnIndex);
            KillColumnSequence(columnIndex, false);
            return;
        }

        var survivors = GetAliveEnemiesSorted(columnIndex, destroyingEnemy);


        if (survivors.Count == 0)
        {
            if (columnIndex < firstRow.Count) firstRow[columnIndex] = -1;

            compactingColumns.Remove(columnIndex);
            predictedFront.Remove(columnIndex);
            currentKilling.Remove(columnIndex);
            KillColumnSequence(columnIndex, false);

            CheckGameFinish();
            return;
        }

        // Cập nhật hiển thị emoji NGAY LẬP TỨC trước khi bắt đầu animation
        for (int i = 0; i < survivors.Count; i++)
        {
            var e = survivors[i];
            e.SetEmojiVisibility(i < 1);
        }

        var seq = DOTween.Sequence();

        for (int i = 0; i < survivors.Count; i++)
        {
            var e = survivors[i];
            Vector3 targetPos = new Vector3(0, 0, i * spacingRow);

            float distance = Vector3.Distance(e.transform.localPosition, targetPos);

            DOTween.Kill(e.transform, complete: false);

            var t = e.transform.DOLocalMove(targetPos, 0.45f).SetEase(Ease.OutQuad);
            seq.Join(t);
        }

        seq.OnComplete(() =>
        {
            for (int i = 0; i < survivors.Count; i++)
            {
                var e = survivors[i];
                e.SetInfo(columnIndex, i); // Update row info after compacting
            }

            compactingColumns.Remove(columnIndex);
            predictedFront.Remove(columnIndex);
            currentKilling.Remove(columnIndex);

            PruneDying();

            // Re-check alive enemies after compacting
            var finalAlive = GetAliveEnemiesSorted(columnIndex);
            if (finalAlive.Count > 0)
            {
                if (columnIndex < firstRow.Count)
                    firstRow[columnIndex] = finalAlive[0].ColorId;

                finalAlive[0].ResetAllLocks();
            }
            else
            {
                if (columnIndex < firstRow.Count)
                    firstRow[columnIndex] = -1;
            }

            CheckGameFinish();
        });

        KillColumnSequence(columnIndex, false);
        columnSequences[columnIndex] = seq;
        seq.Play();
    }
}
