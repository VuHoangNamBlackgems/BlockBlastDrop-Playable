using System;
using UnityEngine;

/// <summary>
/// Super Bullet - Used by Super Canon to hit enemies in any row
/// Simpler logic than normal Bullet, doesn't check firstRow
/// </summary>
public class SuperBullet : MonoBehaviour
{
    public float speed = 90f; // Faster than normal bullet
    private Transform target;
    private EnemyController targetEnemyInstance;
    private bool isMoving;
    public Action<SuperBullet> OnDespawn;
    public bool didHit { get; private set; }
    private bool isPaused = false;
    public int expectedColorId = -1;
    public int canonColorId = -1;
    public int canonInstanceID = -1;
    private int myBulletID = -1;

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void InitTarget(EnemyController enemyController, int _canonInstanceID)
    {
        if (enemyController == null)
        {
            Debug.LogWarning("[SuperBullet] InitTarget called with NULL enemy!");
            Despawn();
            return;
        }

        targetEnemyInstance = enemyController;
        target = enemyController.transform;
        canonInstanceID = _canonInstanceID;
        myBulletID = GetInstanceID();

        Debug.Log($"[SuperBullet {myBulletID}] Initializing - Target: {enemyController.gameObject.name} at row={enemyController.row}, col={enemyController.col}");

        // Lock enemy for this bullet
        bool locked = enemyController.LockForBullet(canonInstanceID, myBulletID);
        if (!locked)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Failed to lock enemy - already locked!");
            Despawn();
            return;
        }

        Debug.Log($"[SuperBullet {myBulletID}] Successfully locked enemy at row={enemyController.row}, col={enemyController.col}");

        // Start moving
        isMoving = true;
        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void Update()
    {
        if (isPaused || !isMoving || target == null) return;

        Vector3 dir = (target.position + new Vector3(0, 1, 0)) - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        float mag = dir.magnitude;

        if (mag <= distanceThisFrame)
        {
            OnTargetReached();
            return;
        }

        transform.position += (dir / mag) * distanceThisFrame;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void OnTargetReached()
    {
        isMoving = false;

        Debug.Log($"[SuperBullet {myBulletID}] Reached target!");

        if (isPaused)
        {
            Debug.Log($"[SuperBullet {myBulletID}] Paused, despawning without hit");
            Despawn();
            return;
        }

        if (targetEnemyInstance == null || target == null)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Target is NULL!");
            Despawn();
            return;
        }

        if (!targetEnemyInstance.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Target not active!");
            Despawn();
            return;
        }

        if (targetEnemyInstance.transform.localScale.sqrMagnitude < 0.01f)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Target is dying (scale too small)!");
            Despawn();
            return;
        }

        // Verify we have permission to destroy this enemy
        if (!targetEnemyInstance.CanBulletDestroy(myBulletID))
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] No permission to destroy enemy!");
            Despawn();
            return;
        }

        // Verify color matches
        if (canonColorId >= 0 && targetEnemyInstance.ColorId != canonColorId)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Color mismatch: canon={canonColorId}, enemy={targetEnemyInstance.ColorId}");
            Despawn();
            return;
        }

        if (expectedColorId >= 0 && targetEnemyInstance.ColorId != expectedColorId)
        {
            Debug.LogWarning($"[SuperBullet {myBulletID}] Expected color mismatch: expected={expectedColorId}, enemy={targetEnemyInstance.ColorId}");
            Despawn();
            return;
        }

        // CRITICAL: Directly destroy the enemy using OnHit()
        // No need to check firstRow or use GridManager for SuperBullet
        Debug.Log($"[SuperBullet {myBulletID}] ✓✓✓ HITTING enemy at row={targetEnemyInstance.row}, col={targetEnemyInstance.col}, ColorId={targetEnemyInstance.ColorId}");

        didHit = true;

        // Call enemy's OnHit to destroy it
        targetEnemyInstance.OnHit();

        Despawn();
    }

    private void Despawn()
    {
        isMoving = false;

        Debug.Log($"[SuperBullet {myBulletID}] Despawning - didHit={didHit}");

        // Release the lock on enemy
        if (targetEnemyInstance != null)
        {
            targetEnemyInstance.ResetBulletLock();
        }

        target = null;
        targetEnemyInstance = null;

        // Notify canon
        if (OnDespawn != null)
        {
            try { OnDespawn(this); }
            catch (MissingReferenceException) { }
        }

        Destroy(gameObject);
    }
}
