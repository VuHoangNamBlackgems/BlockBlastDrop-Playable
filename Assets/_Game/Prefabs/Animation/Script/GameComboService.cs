using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.Image;
public enum EText
{
    AMAZING = 0,
    PERFECT = 1,
    TOOGOOD = 2,
    WELLDONE = 3
}
public class GameComboService : MonoBehaviour
{

    public TextAnimatorService _textAnimatorService;
    [SerializeField] Transform posVerti;
    [SerializeField] Transform posHori;
    private Vector3 pos;
    public static GameComboService Instance;

    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.Effect, TrackTruckCombo);
        GameEventManager.RegisterEvent(GameEventManager.EventId.VerticalScreen, Verti);
        GameEventManager.RegisterEvent(GameEventManager.EventId.LandscapeScreen, Hori);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Effect, TrackTruckCombo);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.VerticalScreen, Verti);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.LandscapeScreen, Hori);
    }

    private void Hori() 
    {
        pos = posHori.position;
    }

    private void Verti()
    {
        pos = posVerti.position;

    }


    public void TrackTruckCombo()
    {
        _textAnimatorService.ShowTextAnimator(pos);
    }
}
