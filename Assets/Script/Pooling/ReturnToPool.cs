using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    public MPool pool;

    public void OnDisable()
    {
        if (pool != null)
            pool.AddToPool(gameObject);
    }
}
