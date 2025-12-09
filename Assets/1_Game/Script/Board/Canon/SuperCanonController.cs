using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Super Canon Controller - can shoot enemies in any row (not just front row)
/// - Pauses all normal canons when spawned
/// - Cannot merge with normal canons
/// - Resumes normal canons when depleted
/// </summary>
public class SuperCanonController : MonoBehaviour
{
    [SerializeField] public ParticleSystem shootParticleEffect;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bodyCanon;
    [SerializeField] private Transform ShootPoint;

    public Material CanonColor = null;
    public int ColorId = -1;

    private Animator anim;
   // private FLookAnimator fLook;

    [SerializeField] private FontController bulletText;
    public int BulletCount = 0;

    private EnemyGridManager enemyGridManager;
    private CanonManager canonManager;

    [SerializeField] private float shootDelay = 0.35f;
    private float lastShootTime = 0f;

    [SerializeField] private float cursorDelay = 0.4f;
    [SerializeField] private float fastCursorDelay = 0.1f;
    private float lastCursorMoveTime = 0f;

    private bool paused;
    private bool onShooting = false;
    private SuperBullet activeBullet = null; // Use SuperBullet instead of Bullet
    private bool spawnAnimCompleted = false;

    // Tracking for systematic row-by-row shooting
    private int currentScanRow = 0;
    private int currentScanColumn = 0;

    public static System.Action<bool> OnSuperCanonActiveChanged; // Notify when super canon is active/inactive

    private void OnEnable()
    {
        CanonManager.OnGlobalShootPauseChanged += OnGlobalPause;
        // Notify that a super canon is now active
        OnSuperCanonActiveChanged?.Invoke(true);
    }

    private void OnDisable()
    {
        CanonManager.OnGlobalShootPauseChanged -= OnGlobalPause;

        if (activeBullet != null)
        {
            activeBullet.OnDespawn = null;
            activeBullet = null;
        }

        // Notify that super canon is no longer active
        OnSuperCanonActiveChanged?.Invoke(false);
    }

    private void OnGlobalPause(bool p) { paused = p; }

    private void Awake()
    {
        AudioController.Instance.Spawn();
     //   Vibration.Vibrate(5);
        anim = GetComponent<Animator>();
     //   fLook = GetComponent<FLookAnimator>();
    }

    public void InitCanon(EnemyGridManager _enemyGridManager = null, CanonManager CanonManager = null)
    {
        if (_enemyGridManager) enemyGridManager = _enemyGridManager;
        if (this.canonManager == null) this.canonManager = CanonManager;

        var r = bodyCanon.GetComponent<Renderer>();
        if (r != null) r.material = CanonColor;

        SetText(BulletCount);

        spawnAnimCompleted = false;
        DOVirtual.DelayedCall(0.5f, () => { spawnAnimCompleted = true; }, false);

        // Reset scan position to start from row 0, column 0
        currentScanRow = 0;
        currentScanColumn = 0;
    }

    private void Update()
    {
        if (paused)
        {
            if (activeBullet != null)
            {
                activeBullet.SetPaused(true);
            }
            if (onShooting)
            {
                onShooting = false;
            }
            return;
        }

        if (activeBullet != null)
        {
            activeBullet.SetPaused(false);
        }

        if (BulletCount <= 0)
        {
            HandleDepleted();
            return;
        }

        if (!spawnAnimCompleted)
        {
            return;
        }

        if (onShooting)
        {
            return;
        }

        if (Time.time - lastShootTime < shootDelay)
        {
            return;
        }
        TryShootNextTarget();
    }

  
    private void TryShootNextTarget()
    {
        if (enemyGridManager == null || enemyGridManager.GridParent == null)
        {
            return;
        }

        int columnCount = enemyGridManager.GridParent.childCount; // Number of columns (10)
        int rowCount = 0;

        // Get row count from first column
        if (columnCount > 0)
        {
            Transform firstColumn = enemyGridManager.GridParent.GetChild(0);
            if (firstColumn != null)
            {
                rowCount = firstColumn.childCount; // Number of rows per column (8)
            }
        }

        if (columnCount == 0 || rowCount == 0)
        {
            return;
        }

        EnemyController targetEnemy = FindNearestMatchingEnemy(columnCount, rowCount);

        if (targetEnemy != null)
        {

            // Found a valid target! Try to claim it
            if (Time.time - lastCursorMoveTime >= cursorDelay)
            {
                bool claimSuccess = targetEnemy.TryClaimAim(GetInstanceID());

                if (claimSuccess)
                {
                    StartCoroutine(VerifyClaimAndShoot(targetEnemy));
                    lastCursorMoveTime = Time.time;
                    return;
                }
                else
                {
                    // Failed to claim, wait and try again
                    lastCursorMoveTime = Time.time;
                }
            }
        }
        else
        {
            lastCursorMoveTime = Time.time;
        }
    }

    private EnemyController FindNearestMatchingEnemy(int columnCount, int rowCount)
    {
        if (enemyGridManager == null || enemyGridManager.GridParent == null)
        {
            return null;
        }

        int totalChecked = 0;
        int maxChecks = columnCount * rowCount; // Prevent infinite loop

        // Start scanning from current position
        int row = currentScanRow;
        int col = currentScanColumn;

        while (totalChecked < maxChecks)
        {

            // Check if this position has a valid enemy
            EnemyController enemy = GetEnemyAtPosition(col, row);

            if (enemy != null)
            {

                // Update scan position to NEXT column for next search
                AdvanceScanPosition(ref col, ref row, columnCount, rowCount);
                currentScanColumn = col;
                currentScanRow = row;

                return enemy;
            }

            // Move to next position
            AdvanceScanPosition(ref col, ref row, columnCount, rowCount);
            totalChecked++;

            // If we've wrapped back to start position, no valid enemy found
            if (totalChecked > 0 && row == currentScanRow && col == currentScanColumn)
            {
                break;
            }
        }

        return null;
    }

    private EnemyController GetEnemyAtPosition(int col, int row)
    {
        // Validate column index
        if (col >= enemyGridManager.GridParent.childCount)
        {
            return null;
        }

        // Get column transform
        Transform columnTransform = enemyGridManager.GridParent.GetChild(col);
        if (columnTransform == null)
        {
            return null;
        }

        // Validate row index
        if (row >= columnTransform.childCount)
        {
            return null;
        }

        // Get enemy transform (enemy IS the child directly)
        Transform enemyTransform = columnTransform.GetChild(row);
        if (enemyTransform == null)
        {
            return null;
        }

        GameObject enemyGO = enemyTransform.gameObject;
        if (enemyGO == null)
        {
            return null;
        }

        // Get EnemyController
        EnemyController ec = enemyGO.GetComponent<EnemyController>();
        if (ec == null)
        {
            return null;
        }

        // Check if this enemy is valid
        if (ec.hasAim || ec.hasActiveBullet)
        {
            return null;
        }

        if (ec.ColorId != ColorId)
        {
            return null;
        }

        if (!ec.gameObject.activeInHierarchy || ec.transform.localScale.sqrMagnitude <= 0.01f)
        {
            return null;
        }

        // Valid enemy found!
        return ec;
    }

    /// <summary>
    /// Advance scan position: column-by-column within a row, then next row
    /// Pattern: [0,0] -> [1,0] -> [2,0] -> ... -> [9,0] -> [0,1] -> [1,1] -> ...
    /// </summary>
    private void AdvanceScanPosition(ref int col, ref int row, int columnCount, int rowCount)
    {
        col++;
        if (col >= columnCount)
        {
            col = 0;
            row++;
            if (row >= rowCount)
            {
                row = 0; // Wrap back to start
            }
        }
    }
    private IEnumerator VerifyClaimAndShoot(EnemyController enemy)
    {
        yield return null;

        // Verify enemy vẫn valid
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
        {
            yield break;
        }

        // Verify claim vẫn thuộc về mình
        if (!enemy.hasAim || !enemy.TryClaimAim(GetInstanceID()))
        {
            yield break;
        }

        // Claim confirmed, shoot!
        FireOneShot(enemy);
    }

    private void FireOneShot(EnemyController targetEnemy)
    {
        if (paused)
        {
            onShooting = false;
            return;
        }

        onShooting = true;
        lastShootTime = Time.time;

        StartCoroutine(SpawnBulletAtAnimatedPoint(targetEnemy));
    }

    private IEnumerator SpawnBulletAtAnimatedPoint(EnemyController targetEnemy)
    {
        yield return new WaitForEndOfFrame();

        if (bulletPrefab == null || ShootPoint == null)
        {
            onShooting = false;
            if (targetEnemy != null)
            {
                targetEnemy.ResetAim();
            }
            yield break;
        }

        // Double-check target validity
        if (targetEnemy == null ||
            !targetEnemy.gameObject.activeInHierarchy ||
            targetEnemy.ColorId != ColorId ||
            targetEnemy.transform.localScale.sqrMagnitude <= 0.01f)
        {
            onShooting = false;
            if (targetEnemy != null) targetEnemy.ResetAim();
            yield break;
        }

        // Verify claim is still valid
        if (!targetEnemy.hasAim)
        {
            onShooting = false;
            yield break;
        }

        Transform target = targetEnemy.transform;

        AudioController.Instance.Shoot();
    //    Vibration.Vibrate(5);
        if (spawnAnimCompleted) anim.SetTrigger("Shoot");

        if (shootParticleEffect) shootParticleEffect.Play();

        var inst = Instantiate(bulletPrefab, ShootPoint.position, ShootPoint.rotation);

        // CRITICAL: Use SuperBullet component instead of Bullet
        var b = inst.GetComponent<SuperBullet>();
        if (b != null)
        {
            activeBullet = b;

            b.expectedColorId = ColorId;
            b.canonColorId = ColorId;
            b.InitTarget(targetEnemy, GetInstanceID());

         //   if (fLook) fLook.ObjectToFollow = target;

            b.OnDespawn = (bullet) =>
            {
                if (this == null) return;

                if (activeBullet == bullet)
                {
                    activeBullet = null;
                }

                onShooting = false;

                // Only decrease bullet count if hit
                if (bullet.didHit)
                {

                    if (BulletCount > 0)
                    {
                        BulletCount--;
                        SetText(BulletCount);
                    }

                    if (BulletCount <= 0)
                    {
                        HandleDepleted();
                        return;
                    }
                }
                else
                {
                    // Bullet didn't hit
                    if (targetEnemy != null)
                    {
                        targetEnemy.ResetAim();
                    }
                }

              //  if (fLook) fLook.ObjectToFollow = null;
            };
        }
        else
        {
            activeBullet = null;
            onShooting = false;

            if (targetEnemy != null)
            {
                targetEnemy.ResetAim();
            }

            Destroy(inst);
        }
    }

    private void SetText(int _bulletCount)
    {
        if (bulletText) bulletText.ShowText(_bulletCount);
    }

    private void HandleDepleted()
    {

        // Notify that super canon is no longer active
        OnSuperCanonActiveChanged?.Invoke(false);

        if (canonManager != null)
        {
            canonManager.OnSuperCanonDepleted(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
