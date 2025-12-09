using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonManager : MonoBehaviour
{
    [SerializeField] private CanonController canon;
    [SerializeField] private CanonController canonEnd;
    [SerializeField] private SuperCanonController superCanonPrefab; // Super Canon prefab
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private EnemyGridManager enemyGridManager;
    [SerializeField] private ParticleSystem explosionEffect;
    private bool isSpawning;
    private bool isMerging;
    private bool isRearranging;
    private bool isLastFalling;
    private bool isPhantomMerging;
    private bool isProcessingQueue;

    private readonly Queue<int> spawnQueue = new Queue<int>();

    private Sequence currentMergeSequence;
    private Tween currentCheckMergeTween;

    public static System.Action<bool> OnGlobalShootPauseChanged;
    private bool pendingQueueTick;
    private Tween periodicValidationTween;

    // Super Canon management
    private SuperCanonController activeSuperCanon = null;
    private bool isSuperCanonActive = false;
    private List<CanonController> pausedNormalCanons = new List<CanonController>();


    public void Start()
    {
        StartPeriodicValidation();

        // Subscribe to super canon events
        SuperCanonController.OnSuperCanonActiveChanged += OnSuperCanonActiveChanged;
    }

    /// <summary>
    /// Bắt đầu validation định kỳ để phát hiện desync sớm
    /// </summary>
    private void StartPeriodicValidation()
    {
        periodicValidationTween = DOVirtual.DelayedCall(2f, () =>
        {
            ValidateAndSyncStandStatus();
            StartPeriodicValidation(); // Loop
        }).SetId(this);
    }

    private void OnDestroy()
    {
        if (periodicValidationTween != null && periodicValidationTween.IsActive())
        {
            periodicValidationTween.Kill();
        }

        // Unsubscribe from super canon events
        SuperCanonController.OnSuperCanonActiveChanged -= OnSuperCanonActiveChanged;
    }

    public void ClearAllCanons()
    {
        StopAllOperations();
        DestroyAllBullets();
        lobbyManager.ResetLobby();
    }

    private void DestroyAllBullets()
    {
        //Bullet[] allBullets = FindObjectsOfType<Bullet>();
        //foreach (Bullet bullet in allBullets)
        //{
        //    if (bullet != null)
        //    {
        //        Destroy(bullet.gameObject);
        //    }
        //}
    }

    private void StopAllOperations()
    {
        isSpawning = isMerging = isRearranging = isLastFalling = isPhantomMerging = isProcessingQueue = false;
        pendingQueueTick = false;
        spawnQueue.Clear();
        if (currentMergeSequence != null && currentMergeSequence.IsActive()) currentMergeSequence.Kill();
        if (currentCheckMergeTween != null && currentCheckMergeTween.IsActive()) currentCheckMergeTween.Kill();
        if (periodicValidationTween != null && periodicValidationTween.IsActive()) periodicValidationTween.Kill();
        DOTween.Kill(this);
    }
    private void ScheduleQueueTick(float delay = 0.05f)
    {
        if (pendingQueueTick) return;
        pendingQueueTick = true;
        DOVirtual.DelayedCall(delay, () =>
        {
            pendingQueueTick = false;
            ProcessQueue();
        }).SetId(this);
    }
    public void SpawnCanon(int colorId)
    {
        spawnQueue.Enqueue(colorId);
        ProcessQueue();
    }
    private void ProcessQueue()
    {
        if (isProcessingQueue || isSpawning || isPhantomMerging || isMerging)
        {
            ScheduleQueueTick(0.05f);
            return;
        }
        if (spawnQueue.Count == 0) return;

        int nextColorId = spawnQueue.Dequeue();
        isProcessingQueue = true;
        ProcessSpawnRequest(nextColorId);
    }


    private void ProcessSpawnRequest(int colorId)
    {
        int standIndex = lobbyManager.FindEmptyStand();
        if (standIndex == -1)
        {
            if (CanFormMergeGroup(colorId)) { PerformPhantomMerge(colorId); return; }
            if (!isLastFalling) SpawnOutRage(colorId);
            isProcessingQueue = false;
            return;
        }
        if (colorId < 0 || colorId >= GameConfig.Instance.GetColorCount())
        {
            isProcessingQueue = false;
            ProcessQueue();
            return;
        }
        StartCoroutine(SpawnCanonCoroutine(colorId, standIndex));
    }

    private IEnumerator SpawnCanonCoroutine(int colorId, int standIndex)
    {
        isSpawning = true;
        Material selectedMaterial = GameConfig.Instance.GetColorCanon(colorId);
        int sameColorPosition = -1;
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            if (lobbyManager.StandStatus[i] == colorId) { sameColorPosition = i; break; }
        }
        if (sameColorPosition != -1)
        {
            ArrangeCanonsForSameColor(colorId, sameColorPosition);
            // Đợi rearrange RIÊNG cho arrange này xong
            yield return new WaitWhile(() => isRearranging);
            standIndex = FindBestPositionForColor(colorId);
        }

        lobbyManager.OpenDoor(standIndex);
        yield return new WaitForSeconds(0.08f);

        Transform standPosition = lobbyManager.GetStandPosition(standIndex);
        CanonController newCanon = Instantiate(canon, standPosition);
        lobbyManager.SetStandStatus(standIndex, colorId);
        newCanon.CanonColor = selectedMaterial;
        newCanon.ColorId = colorId;
        newCanon.BulletCount = 10;

        // CRITICAL FIX: Chỉ set waitingForMerge nếu có 3 cannon LIÊN TIẾP cùng màu
        // Không chỉ dựa vào số lượng, phải kiểm tra consecutive
        bool hasConsecutiveThree = false;
        int consecutiveStartIndex = -1;

        for (int i = 0; i <= lobbyManager.StandStatus.Count - 3; i++)
        {
            if (HasThreeConsecutiveSameColor(i) && lobbyManager.StandStatus[i] == colorId)
            {
                hasConsecutiveThree = true;
                consecutiveStartIndex = i;
                break;
            }
        }

        if (hasConsecutiveThree)
        {
            // Chỉ set waitingForMerge cho 3 cannon LIÊN TIẾP
            for (int i = consecutiveStartIndex; i < consecutiveStartIndex + 3; i++)
            {
                Transform stand = lobbyManager.GetStandPosition(i);
                if (stand != null && stand.childCount > 0)
                {
                    CanonController c = stand.GetChild(0).GetComponent<CanonController>();
                    if (c != null && c.ColorId == colorId)
                    {
                        c.waitingForMerge = true;
                    }
                }
            }
        }
        else
        {
            // Không có 3 liên tiếp, tất cả đều có thể bắn
            newCanon.waitingForMerge = false;
        }

        newCanon.InitCanon(enemyGridManager,this);

        lobbyManager.UpdateAllDoorColors();
        lobbyManager.PrintStandStatus();

        isSpawning = false;
        isProcessingQueue = false;

        ValidateAndSyncStandStatus();

        if (ShouldCompactCanons())
        {
            CompactCanonsToLeft();
        }

        yield return new WaitForSeconds(1f);

        ScheduleCheckMerge(0.1f);
        ScheduleQueueTick(0.15f);
    }

    /// <summary>
    /// CRITICAL: Validate và sync StandStatus với cannon vật lý thực tế
    /// Fix desync giữa data và scene
    /// </summary>
    private void ValidateAndSyncStandStatus()
    {
        bool hasDesync = false;

        // Kiểm tra từng stand position
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            int statusColorId = lobbyManager.StandStatus[i];

            if (stand != null)
            {
                bool hasCanon = stand.childCount > 0;

                if (hasCanon)
                {
                    // Có cannon thật
                    CanonController canon = stand.GetChild(0).GetComponent<CanonController>();
                    if (canon != null)
                    {
                        // So sánh ColorId với StandStatus
                        if (canon.ColorId != statusColorId)
                        {
                            lobbyManager.SetStandStatus(i, canon.ColorId);
                            hasDesync = true;
                        }
                    }
                    else
                    {
                        lobbyManager.SetStandStatus(i, -1);
                        hasDesync = true;
                    }
                }
                else
                {
                    // Không có cannon thật
                    if (statusColorId != -1)
                    {
                        lobbyManager.SetStandStatus(i, -1);
                        hasDesync = true;
                    }
                }
            }
        }

        if (hasDesync)
        {
            lobbyManager.PrintStandStatus();
        }
    }

    private bool ShouldCompactCanons()
    {
        return false;
        // Trước tiên validate để sync
        bool hadDesync = CheckForDesync();

        // Kiểm tra left gap: nếu có -1 ở bên TRÁI của cannon → cần compact
        bool hasLeftGap = HasLeftGap();

        Dictionary<int, List<int>> colorPositions = new Dictionary<int, List<int>>();

        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            int colorId = lobbyManager.StandStatus[i];
            if (colorId != -1)
            {
                if (!colorPositions.ContainsKey(colorId))
                {
                    colorPositions[colorId] = new List<int>();
                }
                colorPositions[colorId].Add(i);
            }
        }

        // Kiểm tra từng màu xem có bị phân tách không
        foreach (var kvp in colorPositions)
        {
            List<int> positions = kvp.Value;
            if (positions.Count >= 2)
            {
                // Kiểm tra xem các vị trí có liên tiếp không
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    if (positions[i + 1] - positions[i] > 1)
                    {
                        return true;
                    }
                }
            }
        }

        return hadDesync || hasLeftGap;
    }

    /// <summary>
    /// Kiểm tra xem có khoảng trống bên trái không
    /// </summary>
    private bool HasLeftGap()
    {
        bool foundCanon = false;
        for (int i = lobbyManager.StandStatus.Count - 1; i >= 0; i--)
        {
            if (lobbyManager.StandStatus[i] != -1)
            {
                foundCanon = true;
            }
            else if (foundCanon)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Chỉ kiểm tra xem có desync không, không sửa
    /// </summary>
    private bool CheckForDesync()
    {
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            int statusColorId = lobbyManager.StandStatus[i];

            if (stand != null)
            {
                bool hasCanon = stand.childCount > 0;

                if (hasCanon)
                {
                    CanonController canon = stand.GetChild(0).GetComponent<CanonController>();
                    if (canon != null)
                    {
                        if (canon.ColorId != statusColorId) return true;
                    }
                    else
                    {
                        if (statusColorId != -1) return true;
                    }
                }
                else
                {
                    if (statusColorId != -1) return true;
                }
            }
        }
        return false;
    }

    private bool HasThreeConsecutiveSameColor(int startIndex)
    {
        if (startIndex + 2 >= lobbyManager.StandStatus.Count) return false;
        int colorId = lobbyManager.StandStatus[startIndex];

        // Ignore super canon positions (marked as -100)
        if (colorId == -100) return false;

        bool result = colorId != -1 &&
               colorId == lobbyManager.StandStatus[startIndex + 1] &&
               colorId == lobbyManager.StandStatus[startIndex + 2];

        // Additional check: make sure none of the three positions is a super canon
        if (result)
        {
            if (lobbyManager.StandStatus[startIndex + 1] == -100 ||
                lobbyManager.StandStatus[startIndex + 2] == -100)
            {
                return false;
            }
        }

        return result;
    }



    private bool CanFormMergeGroup(int colorId)
    {
        int sameColorCount = 0;
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++) if (lobbyManager.StandStatus[i] == colorId) sameColorCount++;
        int queueCount = 0;
        foreach (int q in spawnQueue) if (q == colorId) queueCount++;
        int totalCount = sameColorCount + queueCount + 1;
        return totalCount >= 3;
    }

    private void PerformPhantomMerge(int colorId)
    {
        isPhantomMerging = true;
        // NOTE: No global pause - waitingForMerge flag already prevents shooting

        // Perform phantom merge immediately
        StartCoroutine(WaitAndPerformPhantomMerge(colorId));
    }

    private IEnumerator WaitAndPerformPhantomMerge(int colorId)
    {
        yield return null;

        int existingCount = 0;
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
            if (lobbyManager.StandStatus[i] == colorId) existingCount++;

        int neededFromQueue = Mathf.Max(0, 3 - existingCount - 1);

        int consumedFromQueue = 0;
        if (neededFromQueue > 0 && spawnQueue.Count > 0)
        {
            Queue<int> tempQueue = new Queue<int>();
            while (spawnQueue.Count > 0)
            {
                int q = spawnQueue.Dequeue();
                if (q == colorId && consumedFromQueue < neededFromQueue)
                {
                    consumedFromQueue++;
                }
                else tempQueue.Enqueue(q);
            }
            while (tempQueue.Count > 0) spawnQueue.Enqueue(tempQueue.Dequeue());
        }

        List<int> sameColorPositions = new List<int>();
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
            if (lobbyManager.StandStatus[i] == colorId) sameColorPositions.Add(i);

        if (sameColorPositions.Count == 0)
        {
            isPhantomMerging = false;
            isProcessingQueue = false;
            OnGlobalShootPauseChanged?.Invoke(false);
            ScheduleQueueTick(0.01f);
            yield break;
        }

        List<CanonController> sameColorCanons = new List<CanonController>();
        foreach (int pos in sameColorPositions)
        {
            Transform st = lobbyManager.GetStandPosition(pos);
            if (st != null && st.childCount > 0)
            {
                var cc = st.GetChild(0).GetComponent<CanonController>();
                if (cc != null && cc.ColorId == colorId) sameColorCanons.Add(cc);
            }
        }

        if (sameColorCanons.Count == 0)
        {
            isPhantomMerging = false;
            isProcessingQueue = false;
            OnGlobalShootPauseChanged?.Invoke(false);
            ScheduleQueueTick(0.01f);
            yield break;
        }

        for (int i = 0; i < sameColorCanons.Count && i < 3; i++)
            sameColorCanons[i].waitingForMerge = true;

        yield return null;

        int totalBulletCount = 10 + (consumedFromQueue * 10);
        for (int i = 0; i < sameColorCanons.Count; i++)
            totalBulletCount += sameColorCanons[i].BulletCount;

        CanonController survivingCanon = sameColorCanons[0];
        int survivingPosition = sameColorPositions[0];

        for (int i = 1; i < sameColorCanons.Count; i++)
            if (sameColorCanons[i] != null) Destroy(sameColorCanons[i].gameObject);

        if (survivingCanon != null)
        {
            survivingCanon.BulletCount = totalBulletCount;

            var anim = survivingCanon.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("MergeJump");

            DOTween.Sequence().SetId(this)
                .AppendInterval(0.25f)
                .AppendCallback(() =>
                {
                    if (survivingCanon == null) return;
                    survivingCanon.transform.DOScale(1.15f, 0.1f).OnComplete(() =>
                    {
                        if (survivingCanon == null) return;
                        survivingCanon.transform.DOScale(1.0f, 0.1f).OnComplete(() =>
                        {
                            if (survivingCanon == null) return;
                            survivingCanon.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
                            var an = survivingCanon.GetComponent<Animator>();
                            if (an != null) an.SetTrigger("MergeFall");

                            survivingCanon.waitingForMerge = false;

                            survivingCanon.InitCanon(enemyGridManager,this);
                            survivingCanon.BulletCount = totalBulletCount;

                            if (survivingCanon.mergeParticleEffect != null)
                                survivingCanon.mergeParticleEffect.Play();
                        });
                    });
                });
        }

        List<int> newStandStatus = new List<int>();
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            if (lobbyManager.StandStatus[i] == colorId && i != survivingPosition) continue;
            newStandStatus.Add(lobbyManager.StandStatus[i]);
        }
        while (newStandStatus.Count < lobbyManager.StandStatus.Count) newStandStatus.Add(-1);
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++) lobbyManager.StandStatus[i] = newStandStatus[i];

        lobbyManager.UpdateAllDoorColors();

        RearrangePhysicalCanons(true, true);
        lobbyManager.PrintStandStatus();

        DOVirtual.DelayedCall(0.5f, () =>
        {
            isPhantomMerging = false;
            isProcessingQueue = false;
            OnGlobalShootPauseChanged?.Invoke(false);
            ScheduleQueueTick(0.01f);
        }).SetId(this);
    }




    private void ArrangeCanonsForSameColor(int targetColor, int existingPosition)
    {
        int lastSameColorIndex = -1;
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++) if (lobbyManager.StandStatus[i] == targetColor) lastSameColorIndex = i;
        if (lastSameColorIndex == -1) return;

        List<int> newArrangement = new List<int>(lobbyManager.StandStatus);
        int insertPosition = lastSameColorIndex + 1;

        if (insertPosition < lobbyManager.StandStatus.Count && lobbyManager.StandStatus[insertPosition] != -1)
        {
            for (int i = lobbyManager.StandStatus.Count - 1; i > insertPosition; i--) if (newArrangement[i - 1] != -1) newArrangement[i] = newArrangement[i - 1];
            newArrangement[insertPosition] = -1;
        }
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++) lobbyManager.StandStatus[i] = newArrangement[i];

        lobbyManager.UpdateAllDoorColors();
        RearrangePhysicalCanons();
    }

    private int FindBestPositionForColor(int colorValue)
    {
        int lastSameColorIndex = -1;
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++) if (lobbyManager.StandStatus[i] == colorValue) lastSameColorIndex = i;
        if (lastSameColorIndex != -1 && lastSameColorIndex + 1 < lobbyManager.StandStatus.Count && lobbyManager.StandStatus[lastSameColorIndex + 1] == -1) return lastSameColorIndex + 1;
        return lobbyManager.FindEmptyStand();
    }

    private void ScheduleCheckMerge(float delay)
    {
        if (currentCheckMergeTween != null && currentCheckMergeTween.IsActive()) currentCheckMergeTween.Kill();
        currentCheckMergeTween = DOVirtual.DelayedCall(delay, () =>
        {
            // KHÔNG cần kiểm tra rearrange - merge có thể chạy độc lập
            if (!isMerging && !isSpawning) CheckAndMergeCanons();
        });
    }

    private void RearrangePhysicalCanons(bool skipMergeCheck = false, bool skipQueue = false)
    {
        if (isRearranging) return;
        isRearranging = true;

        // CRITICAL: Validate trước khi rearrange
        ValidateAndSyncStandStatus();

        Dictionary<int, Queue<CanonController>> byColor = new Dictionary<int, Queue<CanonController>>();
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0)
            {
                var c = stand.GetChild(0).GetComponent<CanonController>();
                if (c == null) continue;
                if (!byColor.TryGetValue(c.ColorId, out var q))
                {
                    q = new Queue<CanonController>();
                    byColor.Add(c.ColorId, q);
                }
                q.Enqueue(c);
            }
        }

        foreach (var kv in byColor) foreach (var c in kv.Value) c.transform.SetParent(null, true);

        float maxAnim = 0f;
        for (int i = 0; i < lobbyManager.StandStatus.Count && i < lobbyManager.GetStandCount(); i++)
        {
            int desiredColor = lobbyManager.StandStatus[i];
            if (desiredColor == -1) continue;
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand == null) continue;

            if (byColor.TryGetValue(desiredColor, out var q) && q.Count > 0)
            {
                var canonToMove = q.Dequeue();
                canonToMove.transform.SetParent(stand, true);
                float t = skipMergeCheck ? 0.12f : 0.2f;
                if (Vector3.Distance(canonToMove.transform.position, stand.position) > 0.01f)
                {
                    canonToMove.transform.DOMove(stand.position, t).SetEase(Ease.InOutSine);
                    if (t > maxAnim) maxAnim = t;
                }
                else canonToMove.transform.position = stand.position;
            }
        }

        foreach (var kv in byColor) while (kv.Value.Count > 0) Destroy(kv.Value.Dequeue().gameObject);

        DOVirtual.DelayedCall(maxAnim + 0.03f, () =>
        {
            isRearranging = false;
            lobbyManager.UpdateAllDoorColors();

            ValidateAndSyncStandStatus();

            if (ShouldCompactCanons())
            {
                CompactCanonsToLeft();
            }

            if (!skipMergeCheck) ScheduleCheckMerge(0.25f);
            if (!skipQueue) ScheduleQueueTick(0.01f);
        });
    }

    private void CheckAndMergeCanons()
    {
        // KHÔNG cần kiểm tra rearrange - merge có thể chạy độc lập
        if (isMerging || isSpawning) return;

        // CRITICAL: Validate trước khi check merge
        ValidateAndSyncStandStatus();

        for (int i = 0; i <= lobbyManager.StandStatus.Count - 3; i++)
        {
            if (HasThreeConsecutiveSameColor(i))
            {
                int colorId = lobbyManager.StandStatus[i];

                // CRITICAL: Check if any of the three canons is a Super Canon
                bool hasSuperCanon = false;
                for (int j = i; j < i + 3; j++)
                {
                    Transform stand = lobbyManager.GetStandPosition(j);
                    if (stand != null && stand.childCount > 0)
                    {
                        SuperCanonController sc = stand.GetChild(0).GetComponent<SuperCanonController>();
                        if (sc != null)
                        {
                            hasSuperCanon = true;
                            break;
                        }
                    }
                }

                if (hasSuperCanon)
                {
                    continue; // Skip this merge group
                }
                MergeCanon(i, colorId);
                return;
            }
        }

        ProcessQueue();
    }

    private void MergeCanon(int startIndex, int colorValue)
    {
        isMerging = true;


        CanonController[] canonsToMerge = new CanonController[3];

        for (int i = startIndex; i < startIndex + 3; i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0)
            {
                CanonController c = stand.GetChild(0).GetComponent<CanonController>();
                if (c != null && c.ColorId == colorValue)
                {
                    c.waitingForMerge = true;
                    canonsToMerge[i - startIndex] = c;
                }
            }
        }

        if (currentMergeSequence != null && currentMergeSequence.IsActive()) currentMergeSequence.Kill();

        StartCoroutine(WaitAndMergeCanons(startIndex, colorValue, canonsToMerge));
    }

    private IEnumerator WaitAndMergeCanons(int startIndex, int colorValue, CanonController[] canonsToMerge)
    {
        yield return null;
        yield return null;

        int totalBulletCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if (canonsToMerge[i] != null)
            {
                totalBulletCount += canonsToMerge[i].BulletCount;
            }
        }

        currentMergeSequence = DOTween.Sequence();
        currentMergeSequence.AppendCallback(() =>
        {
            for (int i = 0; i < 3; i++) if (canonsToMerge[i] != null) canonsToMerge[i].GetComponent<Animator>().SetTrigger("MergeJump");
        });
        currentMergeSequence.AppendInterval(0.2f);
        currentMergeSequence.AppendCallback(() =>
        {
            if (canonsToMerge[0] != null) canonsToMerge[0].BulletCount = totalBulletCount;
        });

        Transform firstStandPosition = lobbyManager.GetStandPosition(startIndex);
        if (canonsToMerge[1] != null && firstStandPosition != null)
        {
            var moveTween1 = canonsToMerge[1].transform.DOMove(firstStandPosition.position, 0.16f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                if (canonsToMerge[1] != null) Destroy(canonsToMerge[1].gameObject);
            });
            currentMergeSequence.Append(moveTween1);
        }
        if (canonsToMerge[2] != null && firstStandPosition != null)
        {
            var moveTween2 = canonsToMerge[2].transform.DOMove(firstStandPosition.position, 0.16f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                if (canonsToMerge[2] != null) Destroy(canonsToMerge[2].gameObject);
            });
            if (canonsToMerge[1] != null) currentMergeSequence.Join(moveTween2);
            else currentMergeSequence.Append(moveTween2);
        }

        currentMergeSequence.AppendCallback(() =>
        {
            if (canonsToMerge[0] != null)
            {
                canonsToMerge[0].transform.DOScale(1.12f, 0.08f).OnComplete(() =>
                {
                    canonsToMerge[0].transform.DOScale(1.0f, 0.08f).OnComplete(() =>
                    {
                        if (canonsToMerge[0] != null)
                        {
                            canonsToMerge[0].transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
                            canonsToMerge[0].GetComponent<Animator>().SetTrigger("MergeFall");
                            canonsToMerge[0].waitingForMerge = false; // Allow merged canon to shoot
                            canonsToMerge[0].InitCanon(enemyGridManager,this, true); // Tăng speed sau merge
                            canonsToMerge[0].mergeParticleEffect.Play();
                        }
                    });
                });
            }
        });

        currentMergeSequence.AppendInterval(0.28f);
        currentMergeSequence.AppendCallback(() =>
        {
            UpdateStandStatusAfterMerge(startIndex);
            isMerging = false;

            ValidateAndSyncStandStatus();

            CompactCanonsToLeft();

            lobbyManager.PrintStandStatus();
        });
    }

    private void UpdateStandStatusAfterMerge(int startIndex)
    {
        List<int> newStandStatus = new List<int>();
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            if (i < startIndex || i > startIndex + 2)
            {
                newStandStatus.Add(lobbyManager.StandStatus[i]);
            }
            else if (i == startIndex)
            {
                newStandStatus.Add(lobbyManager.StandStatus[i]);
            }
        }

        // Thêm -1 cho các slot còn thiếu
        while (newStandStatus.Count < lobbyManager.StandStatus.Count)
        {
            newStandStatus.Add(-1);
        }

        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            lobbyManager.StandStatus[i] = newStandStatus[i];
        }

        lobbyManager.UpdateAllDoorColors();

        lobbyManager.PrintStandStatus();
    }

    public void OnCanonDepleted(CanonController canonCtrl)
    {
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0 && stand.GetChild(0) == canonCtrl.transform)
            {
                // CRITICAL FIX: If this canon was waiting for merge, we need to reset merge states
                if (canonCtrl.waitingForMerge)
                {
                    // Reset waitingForMerge for all canons of the same color
                    ResetMergeStateForColor(canonCtrl.ColorId);
                }

                lobbyManager.SetStandStatus(i, -1);

                Destroy(canonCtrl.gameObject);

                canonCtrl.transform.SetParent(null);

                lobbyManager.PrintStandStatus();

                ValidateAndSyncStandStatus();

                CompactCanonsToLeft();

                ScheduleCheckMerge(0.2f);
                return;
            }
        }
    }

    /// <summary>
    /// Reset waitingForMerge state for all canons of the specified color
    /// This prevents deadlock when a canon waiting for merge gets depleted
    /// </summary>
    private void ResetMergeStateForColor(int colorId)
    {
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0)
            {
                CanonController canon = stand.GetChild(0).GetComponent<CanonController>();
                if (canon != null && canon.ColorId == colorId && canon.waitingForMerge)
                {
                    canon.waitingForMerge = false;
                }
            }
        }
    }

    private void CompactCanonsToLeft()
    {
        return;
        if (isRearranging) return;
        isRearranging = true;

        Dictionary<int, List<CanonController>> canonsByColor = new Dictionary<int, List<CanonController>>();

        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0)
            {
                CanonController canon = stand.GetChild(0).GetComponent<CanonController>();
                if (canon != null)
                {
                    if (!canonsByColor.ContainsKey(canon.ColorId))
                    {
                        canonsByColor[canon.ColorId] = new List<CanonController>();
                    }
                    canonsByColor[canon.ColorId].Add(canon);
                    canon.transform.SetParent(null, true);
                }
            }
        }

        // Reset tất cả stand status về -1
        for (int i = 0; i < lobbyManager.StandStatus.Count; i++)
        {
            lobbyManager.StandStatus[i] = -1;
        }

        int currentPosition = 0;
        float maxAnimTime = 0f;

        foreach (var kvp in canonsByColor)
        {
            int colorId = kvp.Key;
            List<CanonController> canons = kvp.Value;

            foreach (CanonController canon in canons)
            {
                if (currentPosition >= lobbyManager.GetStandCount()) break;

                Transform newStand = lobbyManager.GetStandPosition(currentPosition);
                if (newStand != null && canon != null)
                {
                    // Cập nhật status
                    lobbyManager.StandStatus[currentPosition] = colorId;

                    // Di chuyển cannon đến vị trí mới
                    canon.transform.SetParent(newStand, true);

                    float animTime = 0.25f;
                    if (Vector3.Distance(canon.transform.position, newStand.position) > 0.01f)
                    {
                        canon.transform.DOMove(newStand.position, animTime).SetEase(Ease.InOutSine);
                        if (animTime > maxAnimTime) maxAnimTime = animTime;
                    }
                    else
                    {
                        canon.transform.position = newStand.position;
                    }

                    currentPosition++;
                }
            }
        }

        // Cập nhật door colors và kết thúc rearranging
        DOVirtual.DelayedCall(maxAnimTime + 0.05f, () =>
        {
            lobbyManager.UpdateAllDoorColors();
            isRearranging = false;

            // CRITICAL: Validate lại sau compact để đảm bảo sync
            ValidateAndSyncStandStatus();

            // Verify không còn khoảng trống bên trái
            VerifyNoLeftGaps();

            lobbyManager.PrintStandStatus();

            // Sau khi compact xong, check merge
            ScheduleCheckMerge(0.2f);
        });
    }

    /// <summary>
    /// Verify rằng không có khoảng trống bên trái (tất cả cannon phải compact về trái)
    /// </summary>
    private void VerifyNoLeftGaps()
    {
        return;

        bool foundCanon = false;
        for (int i = lobbyManager.StandStatus.Count - 1; i >= 0; i--)
        {
            if (lobbyManager.StandStatus[i] != -1)
            {
                foundCanon = true;
            }
            else if (foundCanon)
            {
                DOVirtual.DelayedCall(0.1f, CompactCanonsToLeft);
                return;
            }
        }
    }

    private void SpawnOutRage(int colorId)
    {
        AudioController.Instance.FullSlot();
        isLastFalling = true;
        int middleStandIndex = 2;
        Transform middleStandPosition = lobbyManager.GetStandPosition(middleStandIndex);
        CanonController endCanon = Instantiate(canonEnd, middleStandPosition);
        Vector3 startPosition = middleStandPosition.position;
        startPosition.y = 30f;
        endCanon.transform.position = startPosition;

        if (colorId >= 0 && colorId < GameConfig.Instance.GetColorCount())
        {
            Material selectedMaterial = GameConfig.Instance.GetColorCanon(colorId);
            endCanon.CanonColor = selectedMaterial;
            endCanon.ColorId = colorId;
            endCanon.InitCanon();
        }

        Vector3 targetPosition = middleStandPosition.position;
        targetPosition.y = 0f;
        endCanon.transform.DOMove(targetPosition, 0.72f).SetEase(Ease.InQuad).OnComplete(ShowEndGame);
    }

    private void ShowEndGame()
    {
        explosionEffect.Play();
        OnGlobalShootPauseChanged?.Invoke(true);
        int middleStandIndex = 2;
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform standPosition = lobbyManager.GetStandPosition(i);
            if (standPosition != null && standPosition.childCount > 0)
            {
                CanonController c = standPosition.GetChild(0).GetComponent<CanonController>();
                if (c != null)
                {
                    Vector3 blastDirection;
                    float blastTime = 1.35f;
                    if (i == middleStandIndex) blastDirection = new Vector3(0, 1f, -3f);
                    else if (i < middleStandIndex)
                    {
                        float leftMultiplier = (middleStandIndex - i) * 0.5f;
                        blastDirection = new Vector3(-2.5f * leftMultiplier, 2f, -1f);
                    }
                    else
                    {
                        float rightMultiplier = (i - middleStandIndex) * 0.5f;
                        blastDirection = new Vector3(2.5f * rightMultiplier, 2f, -1f);
                    }
                    Vector3 startPos = c.transform.position;
                    Vector3 targetPosition = startPos + blastDirection;
                    Vector3 midPoint = startPos + (blastDirection * 0.75f);
                    midPoint.y += 1.5f;
                    Vector3[] waypoints = new Vector3[] { startPos, midPoint, targetPosition };
                    c.transform.DOPath(waypoints, blastTime, PathType.CatmullRom).SetEase(Ease.OutQuart);
                }
            }
        }
        DOVirtual.DelayedCall(0.45f, () => { UIEndLevel.Instance.Show(false); });
    }

    #region Super Canon Management

    /// <summary>
    /// Spawn a Super Canon at the specified stand position
    /// Super Canon will pause all normal canons and shoot enemies in any row
    /// </summary>
    public void SpawnSuperCanon(int colorId, int standIndex = -1)
    {
        if (superCanonPrefab == null)
        {
            Debug.LogError("[CanonManager] Super Canon prefab is not assigned!");
            return;
        }

        if (isSuperCanonActive)
        {
            Debug.LogWarning("[CanonManager] Super Canon already active!");
            return;
        }

        // Find empty stand if not specified
        if (standIndex == -1)
        {
            standIndex = lobbyManager.FindEmptyStand();
            if (standIndex == -1)
            {
                // Try middle position
                standIndex = 2;
            }
        }

        StartCoroutine(SpawnSuperCanonCoroutine(colorId, standIndex));
    }

    private IEnumerator SpawnSuperCanonCoroutine(int colorId, int standIndex)
    {
        isSpawning = true;
        isSuperCanonActive = true;

        // Pause all normal canons
        PauseAllNormalCanons();

        // Open door and spawn
        lobbyManager.OpenDoor(standIndex);
        yield return new WaitForSeconds(0.08f);

        Transform standPosition = lobbyManager.GetStandPosition(standIndex);
        activeSuperCanon = Instantiate(superCanonPrefab, standPosition);

        // Set super canon properties
        Material selectedMaterial = GameConfig.Instance.GetColorCanon(colorId);
        activeSuperCanon.CanonColor = selectedMaterial;
        activeSuperCanon.ColorId = colorId;
        activeSuperCanon.BulletCount = 10; // Super canon has more bullets

        // Mark this stand as occupied by super canon (use special ID like -100)
        lobbyManager.SetStandStatus(standIndex, -100); // Special marker for super canon

        activeSuperCanon.InitCanon(enemyGridManager,this);

        lobbyManager.UpdateAllDoorColors();
        lobbyManager.PrintStandStatus();

        isSpawning = false;

        Debug.Log($"[CanonManager] Super Canon spawned at position {standIndex}, all normal canons paused");
    }

    /// <summary>
    /// Pause all normal canons by setting their waitingForMerge flag
    /// This prevents them from shooting while super canon is active
    /// </summary>
    private void PauseAllNormalCanons()
    {
        pausedNormalCanons.Clear();

        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0)
            {
                CanonController normalCanon = stand.GetChild(0).GetComponent<CanonController>();
                if (normalCanon != null)
                {
                    normalCanon.waitingForMerge = true;
                    pausedNormalCanons.Add(normalCanon);
                }
            }
        }

        Debug.Log($"[CanonManager] Paused {pausedNormalCanons.Count} normal canons");
    }

    /// <summary>
    /// Resume all normal canons that were paused
    /// </summary>
    private void ResumeAllNormalCanons()
    {
        int resumedCount = 0;
        foreach (var canon in pausedNormalCanons)
        {
            if (canon != null)
            {
                canon.waitingForMerge = false;
                resumedCount++;
            }
        }

        pausedNormalCanons.Clear();
        Debug.Log($"[CanonManager] Resumed {resumedCount} normal canons");
    }

    /// <summary>
    /// Called when super canon is depleted
    /// </summary>
    public void OnSuperCanonDepleted(SuperCanonController superCanon)
    {
        Debug.Log("[CanonManager] Super Canon depleted, resuming normal canons");

        // Find the stand position of this super canon
        for (int i = 0; i < lobbyManager.GetStandCount(); i++)
        {
            Transform stand = lobbyManager.GetStandPosition(i);
            if (stand != null && stand.childCount > 0 && stand.GetChild(0) == superCanon.transform)
            {
                // Clear stand status
                lobbyManager.SetStandStatus(i, -1);
                break;
            }
        }

        // Destroy super canon
        if (superCanon != null)
        {
            Destroy(superCanon.gameObject);
        }

        activeSuperCanon = null;
        isSuperCanonActive = false;

        // Resume all normal canons
        ResumeAllNormalCanons();

        // Update doors
        lobbyManager.UpdateAllDoorColors();
        lobbyManager.PrintStandStatus();

        enemyGridManager.AsyncGrid();

        // Check if we can merge any normal canons
        ScheduleCheckMerge(0.3f);
    }

    /// <summary>
    /// Event handler for super canon active state changes
    /// </summary>
    private void OnSuperCanonActiveChanged(bool isActive)
    {
        if (!isActive && isSuperCanonActive)
        {
            // Super canon became inactive
            isSuperCanonActive = false;
            ResumeAllNormalCanons();
        }
    }

    /// <summary>
    /// Check if a normal canon can merge with super canon (it cannot)
    /// </summary>
    private bool CanMergeWithSuperCanon(int colorId, int standIndex)
    {
        // Super canon cannot merge with normal canons
        Transform stand = lobbyManager.GetStandPosition(standIndex);
        if (stand != null && stand.childCount > 0)
        {
            SuperCanonController superCanon = stand.GetChild(0).GetComponent<SuperCanonController>();
            if (superCanon != null)
            {
                return false; // Cannot merge with super canon
            }
        }
        return true;
    }

    #endregion
}

