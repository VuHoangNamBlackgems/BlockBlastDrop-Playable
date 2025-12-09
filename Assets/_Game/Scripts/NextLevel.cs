using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();

        GameEventManager.RegisterEvent(GameEventManager.EventId.Effect_2, OnEffect);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Effect_2, OnEffect);
    }



    private void OnEffect()
    {
        animator.enabled = true;
        animator.Play("Run", -1, 0f); // phát từ frame đầu (normalizedTime = 0)
    }


    public void EnableEffect()
    {
        animator.enabled = false;

    }


}
