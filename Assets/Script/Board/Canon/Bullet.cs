using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 70f;
    private Transform target;
    private EnemyController targetEnemyInstance;
    private bool isMoving;
    public Action<Bullet> OnDespawn;
    public EnemyGridManager GridRef;
    public bool didHit { get; private set; }
    public int hitColumnIndex { get; private set; } = -1;
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
        if (enemyController == null) { Despawn(); return; }

        targetEnemyInstance = enemyController;
        target = enemyController.transform;
        canonInstanceID = _canonInstanceID;
        myBulletID = GetInstanceID();

        bool locked = enemyController.LockForBullet(canonInstanceID, myBulletID);
        if (!locked)
        {
            Despawn();
            return;
        }

        Vector3 currentWorldPosition = transform.position;
        transform.position = currentWorldPosition;
        isMoving = true;
        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
    }

    private void Update()
    {
        if (isPaused || !isMoving || target == null) return;
        Vector3 dir = (target.position + new Vector3(0, 1, 0)) - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        float mag = dir.magnitude;
        if (mag <= distanceThisFrame) { OnTargetReached(); return; }
        transform.position += (dir / mag) * distanceThisFrame;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void OnTargetReached()
    {
        isMoving = false;
        if (isPaused) { Despawn(); return; }

        if (targetEnemyInstance == null || target == null)
        {
            Despawn();
            return;
        }

        if (!targetEnemyInstance.gameObject.activeInHierarchy ||
            targetEnemyInstance.transform.localScale.sqrMagnitude < 0.01f)
        {
            Despawn();
            return;
        }

        if (!targetEnemyInstance.CanBulletDestroy(myBulletID))
        {
            Despawn();
            return;
        }

        if (canonColorId >= 0 && targetEnemyInstance.ColorId != canonColorId)
        {
            Despawn();
            return;
        }

        if (expectedColorId >= 0 && targetEnemyInstance.ColorId != expectedColorId)
        {
            Despawn();
            return;
        }

        int colIndex = -1;
        if (target.parent != null) colIndex = target.parent.GetSiblingIndex();

        if (GridRef != null && colIndex >= 0)
        {
            var currentFrontGO = GridRef.GetEnemyByColumn(colIndex);
            if (currentFrontGO != null)
            {
                var currentFront = currentFrontGO.GetComponent<EnemyController>();

                if (currentFront != targetEnemyInstance)
                {
                    Despawn();
                    return;
                }

                if (canonColorId >= 0 && currentFront.ColorId != canonColorId)
                {
                    Despawn();
                    return;
                }

                if (currentFront.ColorId != expectedColorId)
                {
                    Despawn();
                    return;
                }
            }
            else
            {
                Despawn();
                return;
            }

            bool ok = GridRef.DestroyFirstEnemyInColumn(colIndex);
            if (ok)
            {
                didHit = true;
                hitColumnIndex = colIndex;
            }
        }

        Despawn();
    }

    private void Despawn()
    {
        if (targetEnemyInstance != null)
        {
            targetEnemyInstance.ResetBulletLock();
        }

        if (OnDespawn != null)
        {
            try { OnDespawn(this); }
            catch (MissingReferenceException) { }
        }

        isMoving = false;
        target = null;
        targetEnemyInstance = null;
        OnDespawn = null;
        didHit = false;
        hitColumnIndex = -1;
        expectedColorId = -1;
        canonColorId = -1;
        canonInstanceID = -1;
        myBulletID = -1;
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}
