using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaManager : MonoBehaviour
{
    public static LunaManager instance;
    private bool isStart;
    private void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        if (!isStart)
        {
            isStart = true;
            Luna.Unity.LifeCycle.GameStarted();
        }

    }

    public void GoToStore()
    {
        //StartGame();
        Luna.Unity.LifeCycle.GameEnded();

        DOVirtual.DelayedCall(0.1f, () =>
        {

            Luna.Unity.Playable.InstallFullGame();
        });
    }
}
