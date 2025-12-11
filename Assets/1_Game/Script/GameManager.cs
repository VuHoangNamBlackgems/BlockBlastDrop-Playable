using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent tutorial;
    public GameObject[] objTutorials;

    private bool isStart;
    public bool isCurrentTime;
    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.StartGame, OnStart);
        GameEventManager.RegisterEvent(GameEventManager.EventId.Luna, OnBlockStore);

    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.StartGame, OnStart);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Luna, OnBlockStore);

    }

    int indexLuna = 0;
    private void OnBlockStore()
    {
        indexLuna++;
        if (indexLuna >= 3)
        {
            LunaManager.instance.GoToStore();
        }

    }

    private void Start()
    {
        AniTutorial();
    }
    private void AniTutorial()
    {

        foreach (var item in objTutorials)
        {
            item.transform.DOScale(Vector3.one * 1.1f, 0.4F).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnStart()
    {
        if (isStart)
            return;


        isStart = true;

        tutorial?.Invoke();
        countDownTime();
        foreach (var item in objTutorials)
        {
            item.transform.DOKill();
            item.SetActive(false);

        }
    }

    private void countDownTime()
    {
        if (!isCurrentTime)
            return;

            DOVirtual.DelayedCall(30f, () =>
            {
                LunaManager.instance.GoToStore();
            });
    }


}
