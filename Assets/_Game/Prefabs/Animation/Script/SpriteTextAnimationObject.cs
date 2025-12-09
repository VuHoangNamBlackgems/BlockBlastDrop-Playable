using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTextAnimationObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _textTransform;
    public IEnumerator Show(float intervalStart, float intervalEnd, float delay = 0f)
    {
        _textTransform.localScale = Vector3.zero;
        Color color = _spriteRenderer.color;
        color.a = 0;
        _spriteRenderer.color = color;

        if (delay > 0) yield return new WaitForSeconds(delay);

        yield return _textTransform.DOScale(1.45f, 0.1925f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return _spriteRenderer.DOFade(1f, 0.1925f).SetEase(Ease.OutQuad).WaitForCompletion();

        yield return new WaitForSeconds(intervalStart);

        yield return _textTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).WaitForCompletion();

        yield return new WaitForSeconds(intervalEnd);

        yield return _spriteRenderer.DOFade(0f, 0.45f).WaitForCompletion();
    }

}
