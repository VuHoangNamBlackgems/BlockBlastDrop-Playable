using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [SerializeField] public ParticleSystem mergeParticleEffect, shootParticleEffect;
    //[SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bodyCanon;
    [SerializeField] private Transform ShootPoint;

    public Material CanonColor = null;
    public int ColorId = -1;

    private Animator anim;

    [SerializeField] private FontController bulletText;
    public int BulletCount = 0;

    [SerializeField] private float baseBulletSpeed = 100f;
    [SerializeField] private float speedIncreasePerMerge = 50f;
    public float currentBulletSpeed { get; private set; }

    private EnemyGridManager enemyGridManager;
    private CanonManager canonManager;

    [SerializeField] private float shootDelay = 0.35f;
    private float lastShootTime = 0f;

    [SerializeField] private float cursorDelay = 0.4f;
    [SerializeField] private float fastCursorDelay = 0.1f;
    private float lastCursorMoveTime = 0f;

    private bool paused;
    private readonly List<int> sweepOrder = new List<int>();
    public int sweepCursor = 0;
    private float initialDelay = 0f; // Delay khác nhau cho mỗi cannon để tránh conflict

    private bool onShooting = false;
    private Bullet activeBullet = null;
    private bool spawnAnimCompleted = false;

    public bool waitingForMerge = false; // If true, canon will not shoot until after merge

    private void OnEnable() { CanonManager.OnGlobalShootPauseChanged += OnGlobalPause; }
    private void OnDisable()
    {
        CanonManager.OnGlobalShootPauseChanged -= OnGlobalPause;

        if (activeBullet != null)
        {
            activeBullet.OnDespawn = null;
            activeBullet = null;
        }
    }
    private void OnGlobalPause(bool p) { paused = p; }

    private void Awake()
    {
        AudioController.Instance.Spawn();
     //   Vibration.Vibrate(5);
        anim = GetComponent<Animator>();
    }

    public void InitCanon(EnemyGridManager _enemyGridManager = null, CanonManager canonManager = null, bool isMerged = false)
    {
        if (_enemyGridManager) enemyGridManager = _enemyGridManager;
        if (this.canonManager == null) this.canonManager = canonManager;

        var r = bodyCanon.GetComponent<Renderer>();
        if (r != null) r.material = CanonColor;

        SetText(BulletCount);

        // Tăng speed nếu đây là canon sau khi merge
        if (isMerged)
        {
            currentBulletSpeed += speedIncreasePerMerge;
        }
        else
        {
            // Canon mới spawn, sử dụng base speed
            currentBulletSpeed = baseBulletSpeed;
        }

        spawnAnimCompleted = false;
        DOVirtual.DelayedCall(0.5f, () => { spawnAnimCompleted = true; }, false);

        BuildSweep();
    }
    private void BuildSweep()
    {
        sweepOrder.Clear();

        // Use the actual column count from the grid
        int columnCount = (enemyGridManager != null && enemyGridManager.firstRow != null)
            ? enemyGridManager.firstRow.Count
            : 10; // default to 10 columns

        for (int i = 0; i < columnCount; i++)
            sweepOrder.Add(i);

        // CRITICAL: Tất cả cannon đều bắt đầu từ cột 0 (trái sang phải)
        // Nhưng mỗi cannon có initialDelay khác nhau để tránh conflict
        sweepCursor = 0;

        // Tạo delay ngẫu nhiên nhỏ cho mỗi cannon (0 - 0.3s)
        int myInstanceID = GetInstanceID();
        initialDelay = (Mathf.Abs(myInstanceID) % 10) * 0.03f; // 0, 0.03, 0.06, ... 0.27s

        lastCursorMoveTime = Time.time;
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

        if (waitingForMerge)
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

        TryShootNextInSweep();
    }
    private void TryShootNextInSweep()
    {
        if (enemyGridManager == null)
        {
            return;
        }

        if (enemyGridManager.GridParent == null)
        {
            return;
        }

        // CRITICAL: KHÔNG di chuyển cursor nếu đang pause, spawn chưa xong, hoặc đang merge
        if (paused || !spawnAnimCompleted || waitingForMerge)
        {
            return;
        }

        // CRITICAL: Đợi initialDelay trước khi bắt đầu (chỉ áp dụng lần đầu tiên)
        if (sweepCursor == 0 && Time.time - lastCursorMoveTime < initialDelay)
        {
            return;
        }

        // CRITICAL FIX: Đảm bảo sweepCursor luôn trong phạm vi hợp lệ
        if (sweepCursor >= sweepOrder.Count)
        {
            sweepCursor = 0;
            lastCursorMoveTime = Time.time;
        }

        if (sweepCursor < sweepOrder.Count)
        {
            int col = sweepOrder[sweepCursor];

            // Real-time check instead of using cached targetRow
            var frontGO = enemyGridManager.GetEnemyByColumn(col);
            if (frontGO == null)
            {
                // No enemy in this column, move cursor forward
                // ALWAYS move to next column - đảm bảo quét tuần tự
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
                return;
            }

            var ec = frontGO.GetComponent<EnemyController>();
            if (ec == null)
            {
                // Invalid enemy, skip to next column
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
                return;
            }

            // CRITICAL: Check if enemy đã được claimed hoặc có bullet đang bay
            if (ec.hasAim || ec.hasActiveBullet)
            {
                // Enemy đã được claimed, skip to next column IMMEDIATELY
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
                return;
            }

            // Check if this enemy matches our color
            if (ec.ColorId != ColorId)
            {
                // Color mismatch, skip to next column
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
                return;
            }

            // Enemy is dying, skip
            if (!ec.gameObject.activeInHierarchy || ec.transform.localScale.sqrMagnitude <= 0.01f)
            {
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
                return;
            }

            // CRITICAL FIX: Đợi cursorDelay trước khi thử claim
            // Điều này giúp phân tán các cannon, tránh nhiều cannon cùng claim 1 enemy
            if (Time.time - lastCursorMoveTime < cursorDelay)
            {
                // Chưa đủ thời gian, đợi
                return;
            }

            // Try to claim this enemy
            bool claimSuccess = ec.TryClaimAim(GetInstanceID());

            if (claimSuccess)
            {
                // Claim thành công, bắn ngay!
                StartCoroutine(VerifyClaimAndShoot(ec));
            }
            else
            {
                // CRITICAL: Claim FAILED - enemy đã được claimed bởi cannon khác
                // Move to next column IMMEDIATELY to find other targets
                sweepCursor++;
                if (sweepCursor >= sweepOrder.Count)
                {
                    sweepCursor = 0;
                }
                lastCursorMoveTime = Time.time;
            }
        }
    }

    private IEnumerator VerifyClaimAndShoot(EnemyController enemy)
    {
        // REMOVED: Không cần wait 1 frame nữa vì có hệ thống lock chặt chẽ
        // Claim ngay và bắn ngay

        // Verify enemy vẫn valid
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
        {
            sweepCursor++;
            lastCursorMoveTime = Time.time;
            yield break;
        }

        // Verify claim vẫn thuộc về mình
        if (!enemy.hasAim || !enemy.TryClaimAim(GetInstanceID()))
        {
            // Lost claim, skip
            sweepCursor++;
            lastCursorMoveTime = Time.time;
            yield break;
        }

        // Claim confirmed, shoot!
        FireOneShot(enemy);
        sweepCursor++;
        lastCursorMoveTime = Time.time;
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
    GameObject inst;
    Bullet b;
    private IEnumerator SpawnBulletAtAnimatedPoint(EnemyController targetEnemy)
    {
        yield return new WaitForEndOfFrame();

        if (ShootPoint == null)
        {
            onShooting = false;

            // Release the aim if bullet spawn failed
            if (targetEnemy != null)
            {
                targetEnemy.ResetAim();
            }

            yield break;
        }

        // CRITICAL: Double-check color BEFORE spawning bullet
        // Enemy may have been destroyed and replaced during WaitForEndOfFrame
        if (targetEnemy == null ||
            !targetEnemy.gameObject.activeInHierarchy ||
            targetEnemy.ColorId != ColorId ||
            targetEnemy.transform.localScale.sqrMagnitude <= 0.01f)
        {
            // Target is invalid or wrong color, don't shoot
            onShooting = false;
            if (targetEnemy != null) targetEnemy.ResetAim();
            yield break;
        }

        // CRITICAL: Verify claim is still valid BEFORE shooting
        if (!targetEnemy.hasAim)
        {
            // Claim was lost, don't shoot
            onShooting = false;
            yield break;
        }

        Transform target = targetEnemy.transform;

        AudioController.Instance.Shoot();
    //    Vibration.Vibrate(5);
        if (spawnAnimCompleted) anim.SetTrigger("Shoot");

        if (shootParticleEffect) shootParticleEffect.Play();

        inst = PoolManager.Instance.GetFromPool(BoardController.Instance.bulletPrefab);
        inst.transform.SetPositionAndRotation(ShootPoint.position, ShootPoint.rotation);
        inst.SetActive(true);

        if (inst.TryGetComponent<Bullet>(out b))
        {
            activeBullet = b;

            b.GridRef = enemyGridManager;
            b.expectedColorId = ColorId;
            b.canonColorId = ColorId;
            b.speed = currentBulletSpeed; // Set speed từ canon
            b.InitTarget(targetEnemy, GetInstanceID()); // CRITICAL: Pass canon instance ID


            b.OnDespawn = (bullet) =>
            {
                if (this == null) return;

                //if (activeBullet == bullet)
                //{
                //    activeBullet = null;
                //}

                onShooting = false;

                // CRITICAL: Chỉ trừ bullet KHI VÀ CHỈ KHI bullet thực sự hit enemy
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
                    // Bullet không hit (enemy đã bị kill bởi bullet khác)
                    // Release aim để enemy khác có thể được target
                    if (targetEnemy != null)
                    {
                        targetEnemy.ResetAim();
                    }
                }

            };
        }
        else
        {
            activeBullet = null;
            onShooting = false;

            // Release the aim if bullet component not found
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
        if (canonManager != null) canonManager.OnCanonDepleted(this);
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
