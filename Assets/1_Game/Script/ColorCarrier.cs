using UnityEngine;

public class ColorCarrier : MonoBehaviour
{
    public int colorId = -1;
    public string layer = "Default";
    public void Start()
    {
        var cube = GetComponent<Cube>();
        var shape = transform.parent ? transform.parent.GetComponent<Shape>() : null;
        if (cube != null)
        {
            colorId = (int)cube.color;
            layer = LayerMask.LayerToName(cube.gameObject.layer);
        }
        if (shape != null)
        {
            colorId = (int)shape.color;
            layer = LayerMask.LayerToName(shape.gameObject.layer);
        }
    }
}
