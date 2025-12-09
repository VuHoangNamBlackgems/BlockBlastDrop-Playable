
using UnityEngine;

public class FontController : MonoBehaviour
{
    [SerializeField] private FaceEmoji Text1;
    [SerializeField] private FaceEmoji Text2;
    private void Awake()
    {
      HideText();
    }
    public void HideText()
    {
        Text1.gameObject.SetActive(false);
        Text2.gameObject.SetActive(false);
    }
    public void ShowText(int value)
    {
        SetVisualFont(value);
        SetText(value);

    }
    private void SetVisualFont(int value)
    {
        if (value < 10)
        {
            Text1.gameObject.SetActive(true);
            Text2.gameObject.SetActive(false);
            Text1.gameObject.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            Text1.gameObject.SetActive(true);
            Text2.gameObject.SetActive(true);
            Text1.gameObject.transform.localPosition = new Vector3(-0.3f, 0, 0);
            Text2.gameObject.transform.localPosition = new Vector3(0.3f, 0, 0);
        }
    }
    private void SetText(int value)
    {
        if (value < 10)
        {
            Text1.Play(value.ToString());
        }
        else
        {
            int first = value / 10;
            int second = value % 10;
            Text1.Play(first.ToString());
            Text2.Play(second.ToString());
        }
    }
}
