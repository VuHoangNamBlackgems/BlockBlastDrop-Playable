using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCombo : MonoBehaviour
{
    public TextMeshProUGUI[] txtAnimation;
    public Transform[] objParentAnim;

    public string[] stringName;
    public static GameCombo instance;
    /*private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.Mission, ShowAnimation);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Mission, ShowAnimation);
    }*/




    public void ShowAnimation()
    {
        SetText();
        SetTxtAnimation();
    }

    private void SetText()
    {
        foreach (var item in txtAnimation)
        {
            item.text = stringName[Random.Range(0, stringName.Length)];
        }
    }
    public void SetTxtAnimation()
    {
        foreach (var item in objParentAnim)
        {

            item.DOScale(Vector3.one, 0.3F).SetEase(Ease.InOutBack).OnComplete(() =>
            {
                item.DOScale(Vector3.zero, 0.3f).SetDelay(0.5f);

            });
        }
    }
}
