using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class HammerBoos : MonoBehaviour
{
    [SerializeField] Animator ani;
    [SerializeField] Transform _hammerTransform;
    [SerializeField] ParticleSystem _effect;
  //  [SerializeField] AudioClip _hammerClip;

    public void AppearActive(bool isCheck)
    {
        _hammerTransform.localScale = Vector3.zero;
        _hammerTransform.gameObject.SetActive(isCheck);
        HammerAni(Vector3.one);
    }

    public void HammerBooster(MultiBlockController multiBlockController,Action a = null)
    {
        StartCoroutine(IEHammerBooster(multiBlockController,a));
    }


    private IEnumerator IEHammerBooster(MultiBlockController multiBlockController, Action a = null)
    {
        ani.enabled = false;

        Vector3 targetPosition = multiBlockController.MainBlock.CenterTf.position;
        Quaternion targetRotation = Quaternion.Euler(Vector3.down * 100f);

        float duration = Vector3.Distance(_hammerTransform.position, targetPosition) / 20f;

        // Tạo sequence để thực hiện đồng thời
        Sequence sequence = DOTween.Sequence();

        // Thêm di chuyển và xoay cùng lúc
        sequence.Join(_hammerTransform.DOMove(targetPosition, duration));
        sequence.Join(_hammerTransform.DORotateQuaternion(targetRotation, duration));
        multiBlockController.ActiveCollider();

        yield return sequence.WaitForCompletion();

        // Phóng to búa
        yield return _hammerTransform.DOScale(Vector3.one * 1.5f, duration).WaitForCompletion();

        ani.enabled = true;
        yield return new WaitForSeconds(0.2f);
        EffectSmoker();
        a?.Invoke();
        yield return multiBlockController.BlockAnim().WaitForCompletion();
        //yield return new WaitForSeconds(0.5f);
        yield return HammerAni(Vector3.zero).WaitForCompletion();
        LunaManager.instance.GoToStore();

    }
    private Tween HammerAni(Vector3 scale)
    {
        Sequence SE = DOTween.Sequence();
        SE.Append(_hammerTransform.DOScale(scale,0.3f));
        return SE;
    }

    private void EffectSmoker()
    {
        _effect.Play();

    }


}
