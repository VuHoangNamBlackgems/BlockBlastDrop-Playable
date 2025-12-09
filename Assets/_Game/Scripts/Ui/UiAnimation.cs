using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UiAnimation : MonoBehaviour
{
    public Image outline;


    // Start is called before the first frame update
    void Start()
    {
        DOVirtual.DelayedCall(0.4f, () =>
        {
            Sequence se = DOTween.Sequence();
            //se.Append(outline.DOColor(Color.white * 0.5f, 0.33F).SetLoops(-1, LoopType.Yoyo));
            se.Insert(0, transform.DOScale(Vector3.one * 0.95F, 0.33F).SetLoops(-1, LoopType.Yoyo));
        });
    }
}
