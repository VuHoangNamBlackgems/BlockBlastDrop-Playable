using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MissingIdea : MonoBehaviour
{
    public BoosterManager boosterManager;
    public UnityEvent inputEvent;

    private Coroutine coroutine;

    private int valueNumber;

    [Space]
    [SerializeField] private int Point = 65;
    [SerializeField] private TextMeshProUGUI[] txtPoint;
    [SerializeField] private Image[] imBarPoint;
    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.Idea_3, AddPoint);
        GameEventManager.RegisterEvent(GameEventManager.EventId.Idea_4, OnStartIdea);
       // GameEventManager.RegisterEvent(GameEventManager.EventId.Idea, OnStartIdea);
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameWin, GameWin);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Idea_3, AddPoint);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Idea_4, OnStartIdea);
      //  GameEventManager.UnRegisterEvent(GameEventManager.EventId.Idea, OnStartIdea);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameWin, GameWin);
    }

    private void AddPoint()
    {
        int points = Point + 9;
        AniTxt(points);
        AniImageBar();

    }
    public int valueMax;
    private int indexPoints;
    public bool isStartWin;
    public bool isDestroyBlock;
    public float isMaxDestroy;

    private void GameWin()
    {
        indexPoints++;
        if(indexPoints >= valueMax)
        {

            isStartWin = true;
        }
    }


    private void AniTxt(int Point)
    {
        DOVirtual.Int(this.Point, Point, 0.5f, (value) =>
        {
            foreach (var item in txtPoint)
            {
                item.text = value.ToString();
            }
            this.Point = Point;
        });
    }
    float index = 0f;
    private void AniImageBar()
    {
        index++;
        foreach (var item in imBarPoint)
        {
            item.DOFillAmount((float)index / 13f, 0.5f);
        }
    }

    

    private void OnStartIdea()
    {
        if(!isDestroyBlock || !isStartWin)
            return;

        AppearPop(() =>
        {
            inputEvent?.Invoke();
        });

    }



    private void AppearPop(Action A = null)
    {
        Debug.Log(":ssss");
        valueNumber++;
        if (valueNumber >= isMaxDestroy /*|| valueNumber == isMaxDestroy_2*/)
        {
            A?.Invoke();

        }
    }

    private void ActivePop()
    {
        boosterManager.AppearPop();
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }


}
