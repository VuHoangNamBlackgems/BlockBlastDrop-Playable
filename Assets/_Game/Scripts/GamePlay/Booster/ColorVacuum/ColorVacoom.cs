using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ColorVacoom : MonoBehaviour
{

    public Transform _magnetTf;
    public Transform _blocksHolder;
    // Start is called before the first frame update

    public void AppearActive(bool isCheck)
    {
        _magnetTf.localScale = Vector3.zero;

        _magnetTf.gameObject.SetActive(isCheck);
        HammerAni(Vector3.one);
    }
    private Tween HammerAni(Vector3 scale)
    {
        Sequence SE = DOTween.Sequence();
        SE.Append(_magnetTf.DOScale(scale, 0.3f));
        return SE;
    }

    public void VaCuumBooster(MultiBlockController MultiBlockController, Action a = null)
    {
        IeUserBooster(MultiBlockController, a);
    }

    private void IeUserBooster(MultiBlockController MultiBlockController, Action a = null)
    {
        a?.Invoke();

        List<MultiBlockController> listMul = new List<MultiBlockController>();
        var targetOffset = 0f;

        foreach (var item in LevelMarket.instance.MultiBlockControllerList)
        {
            if (MultiBlockController.MainBlock._tColor == item.MainBlock.TColor)
            {
                item.ActiveCollider();
                listMul.Add(item);
            }
        }

        listMul = listMul
            .OrderBy(item => Vector3.Distance(_blocksHolder.position, item.MoveTransform.position))
            .ToList();



        SoundManager.Ins.PlayVFX(SoundType.Vacuum);

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < listMul.Count; i++)
        {
            targetOffset += 0.86F;
            listMul[i].MoveTransform.SetParent(_blocksHolder);
            
            float value = targetOffset;
            float distance = Vector3.Distance(_blocksHolder.position, listMul[i].MoveTransform.position);
            float duration = distance / 30f;

            sequence.Append(listMul[i].MoveTransform.DOLocalMove(Vector3.down * value, duration)).SetEase(Ease.InQuad);
            sequence.Join(listMul[i].MoveTransform.DORotate(new Vector3(0, UnityEngine.Random.Range(180, 360), 0), duration));
            sequence.Join(listMul[i].MoveTransform.DOScale(Vector3.one * 0.7F, duration));
        }
        foreach (var item in listMul)
        {
            LevelMarket.instance.RemoveBlock(item);
        }

        sequence.OnComplete(() =>
        {
            _magnetTf.DOLocalMoveY(25, 2f).OnComplete(() =>
            {
                AppearActive(false);
            });
        });

    }
}
