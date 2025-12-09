using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    PickDown,
    PickUp,
    BlockMove,
    Boom,
    Win,
    Lose,
    Vacuum,
    Hammer,
    Freeze,
    Btn

}

public class SoundManager : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip PickDown,PickUp,Boom,Win,Lose, Destroys,Hammer,Freeze, Btn;


    public SoundEnifi SoundMove;
    public static SoundManager Ins;
    private void Awake()
    {
        Ins = this;
    }

    public void PlayVFX(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.PickDown:
                AudioSource.PlayOneShot(PickDown);
                break;
            case SoundType.PickUp:
                AudioSource.PlayOneShot(PickUp);
                break;
            case SoundType.BlockMove:
                SoundMove.Play();
                break;
            case SoundType.Freeze:
               AudioSource.PlayOneShot(Freeze);
                break;
            case SoundType.Lose:
                  //AudioSource.PlayOneShot(Lose);
                break;
            case SoundType.Win:
                 AudioSource.PlayOneShot(Win);
                break;
            case SoundType.Hammer:
                AudioSource.PlayOneShot(Hammer);
                break;
            case SoundType.Vacuum:
                AudioSource.PlayOneShot(Destroys);
                break;
            case SoundType.Btn:
                AudioSource.PlayOneShot(Btn);
                break;
        }
    }   
}
