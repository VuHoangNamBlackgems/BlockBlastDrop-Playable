using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject, MPool> dicPools = new Dictionary<GameObject, MPool>();

    public GameObject GetFromPool(GameObject obj)
    {
        if (dicPools.ContainsKey(obj) == false)
        {
            dicPools.Add(obj, new MPool(obj));
        }
        return dicPools[obj].Get();
    }
}
