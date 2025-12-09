using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockManager : MonoBehaviour
{
    private List<IUnlockable> allUnlockables = new List<IUnlockable>();


    public static LockManager Instance;

    private void Awake()
    {
        Instance = this;
    }


    public void Register(IUnlockable unlockable)
    {
        if (!allUnlockables.Contains(unlockable))
            allUnlockables.Add(unlockable);
    }

    public IUnlockable GetFirstLocked()
    {
        foreach (var item in allUnlockables)
        {
            if (!item.IsFullyUnlocked())
                return item;
        }

        return null;
    }
}
