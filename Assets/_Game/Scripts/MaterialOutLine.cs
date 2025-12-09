using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOutLine : MonoBehaviour
{
/*    [SerializeField] Material outlineMaskMaterial;
    [SerializeField] Material outlineFillMaterial;*/



/*    public static Material OutlineMaskMaterial => Ins.outlineMaskMaterial;
    public static Material OutlineFillMaterial => Ins.outlineFillMaterial;*/

    public static MaterialOutLine Ins;
    public void Awake()
    {
        Ins = this;
    }
}
