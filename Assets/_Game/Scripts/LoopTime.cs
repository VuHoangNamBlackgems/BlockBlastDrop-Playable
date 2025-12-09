using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopTime : MonoBehaviour
{

    private bool isStartGame;

    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.StartGame, StartGame);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.StartGame, StartGame);
    }

    private void StartGame()
    {
        if (!isStartGame)
        {
            isStartGame = true;
            DOVirtual.DelayedCall(30f, OnLoop)
                        .SetLoops(-1, LoopType.Restart);
        }
    }

    void OnLoop()
    {
        LunaManager.instance.GoToStore();
    }


}
