using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour, IKey
{
    private LockManager lockManager;
    public float flyDuration = 0.5f;



    public void MoveKey()
    {
        transform.parent = null;
        if (lockManager == null)
        {
            lockManager = LockManager.Instance;
        }

        IUnlockable target = lockManager.GetFirstLocked();

        Vector3 startPos = transform.position;
        Vector3 endPos = target.GetTransform().position + Vector3.up * 1.3F;

        Vector3 controlPoint = (startPos + endPos) / 2 + Vector3.up * 2F;

        Vector3[] path = new Vector3[] { startPos, controlPoint, endPos };


        if (target == null)
        {
            return;
        }


        transform.DOPath(path, flyDuration, PathType.CatmullRom)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                target.TryUnlock();
                ScaleKey();
            });
    }


    private void ScaleKey()
    {
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutSine);
    }
}

public interface IKey
{
    void MoveKey();
}
