
using UnityEngine;

public class PrefabPreviewInfo
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 scale=new Vector3(2,2,2);
    public Quaternion rotation;

    public PrefabPreviewInfo(GameObject prefab, Vector3 position, Vector3 scale, Quaternion rotation)
    {
        this.prefab = prefab;
        this.position = position;
        this.scale = scale;
        this.rotation = rotation;
    }
}