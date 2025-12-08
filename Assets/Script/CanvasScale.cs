using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScale : MonoBehaviour
{
    private void Awake()
    {
        if ((float)Screen.width / Screen.height < (float)1920 / 1080)
        {
            GetComponent<CanvasScaler>().matchWidthOrHeight = 0;
        }
        if ((float)Screen.width / Screen.height > (float)1080 / 1920)
        {
            GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
        }
    }
}
