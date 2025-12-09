using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBlockVehicle : MonoBehaviour
{
    [Header("EffectFreeze")]
    [SerializeField] ParticleSystem effFree;
    [SerializeField] Transform imFreeze;

    [Space]

    [Header("EffectBooster")]
    [SerializeField] HammerBoos hammerBooster;

    [Header("EffectVaCuum")]
    [SerializeField] ColorVacoom vacuumBooster;
    public void TimeFreeze()
    {
        PlayEffect(true);
        AppearFreeze(true);
        PlayVFX(SoundType.Btn);
        PlayVFX(SoundType.Freeze);

        DOVirtual.DelayedCall(15f, () =>
        {
            PlayEffect(false);
            AppearFreeze(false);
            RunTime();
        });
    }

    public void DestroyBlock()
    {
        RunTime();
        PlayVFX(SoundType.Btn);
        hammerBooster.AppearActive(true);
    }

    public void MagnetBlock()
    {

        RunTime();
        PlayVFX(SoundType.Btn);
        vacuumBooster.AppearActive(true);

    }

    private void PlayVFX(SoundType sound)
    {
        SoundManager.Ins.PlayVFX(sound);

    }


    public void HammerBoosterMove(MultiBlockController multiBlockController, Action a = null)
    {
        hammerBooster.HammerBooster(multiBlockController, a);

    }

    public void VacuumBoosterMove(MultiBlockController multiBlockController, Action a = null)
    {
        vacuumBooster.VaCuumBooster(multiBlockController, a);

    }


    public void StopTime()
    {
        UIManager.instance.StopTime();

    }

    public void RunTime()
    {
        UIManager.instance.RunTime();

    }



    private void PlayEffect(bool check)
    {
        if (check)
            effFree.Play();
        else
            effFree.Stop();

    }
    private void AppearFreeze(bool check)
    {
        imFreeze.gameObject.SetActive(check);
    }
}
