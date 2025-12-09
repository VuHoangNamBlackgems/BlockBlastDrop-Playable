using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSquare : MonoBehaviour
{
    private Shape shape;
    private float overlapPercent = 0f;

    public void Init(Shape _shape)
    {
        shape = _shape;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckConditionPickedUp(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckConditionPickedUp(other);
    }

    void CheckConditionPickedUp(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Grid")) return;
        if (shape == null) return;
        else
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Cube") && other.CompareTag(shape.color.ToString()))
                OnPickedUp(other);
        }
            
    }

    void OnPickedUp(Collider other)
    {
        overlapPercent = GetOverlapPercent(other, GetComponent<MeshCollider>());
        if (overlapPercent > 60f && shape.numberCubePickup > 0)
        {
            other.transform.GetComponent<BoxCollider>().enabled = false;
            shape.numberCubePickup--;
            other.transform.GetComponent<Cube>().OnPickedUp(transform, shape);
        }
    }

    float GetOverlapPercent(Collider a, Collider b)
    {
        if (!a || !b) return 0f;

        Bounds A = a.bounds;
        Bounds B = b.bounds;

        float overlapX = Mathf.Max(0, Mathf.Min(A.max.x, B.max.x) - Mathf.Max(A.min.x, B.min.x));
        float overlapZ = Mathf.Max(0, Mathf.Min(A.max.z, B.max.z) - Mathf.Max(A.min.z, B.min.z));

        float overlapVolume = overlapX * overlapZ;
        float volumeA = (A.size.x * A.size.z);

        if (volumeA <= 0.0001f) return 0f;
        return (overlapVolume / volumeA) * 100f;
    }
}
