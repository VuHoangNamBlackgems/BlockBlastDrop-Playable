using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSize : MonoBehaviour
{
    [SerializeField] ViewCamera[] viewCameraHori;
    [SerializeField] ViewCamera[] viewCameraVerti;
    [SerializeField] int currentViewIndex = 0;
    [SerializeField] TScreen _currentScreen;

    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.LandscapeScreen, OnHori);
        GameEventManager.RegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerti);
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameWin, NextCamera);

    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.LandscapeScreen, OnHori);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerti);

        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameWin, NextCamera);

    }



    private void OnHori()
    {
        SetCamera(viewCameraHori, TScreen.LandscapeScreen);
    }

    private void OnVerti()
    {
        SetCamera(viewCameraVerti, TScreen.VerticalScreen);
    }


    private void NextCamera()
    {
        DOVirtual.DelayedCall(0.5F, () =>
        {
            int nextIndex = currentViewIndex + 1;
            if (nextIndex < viewCameraHori.Length)
            {
                currentViewIndex = nextIndex;
                (_currentScreen == TScreen.LandscapeScreen ? (System.Action)OnHori : OnVerti)();
            }
        });
    }

    private void SetCamera(ViewCamera[] views, TScreen currentScreen)
    {
        _currentScreen = currentScreen;
        transform.DOMove(views[currentViewIndex].pos, 0.25f).SetEase(Ease.OutQuad);

        // Tween fieldOfView camera
        DOTween.To(
            () => Camera.main.fieldOfView,
            x => Camera.main.fieldOfView = x,
            views[currentViewIndex].fieldOfView,
            0.25f
        ).SetEase(Ease.OutQuad);
    }


}
[System.Serializable]
public class ViewCamera
{
    public Vector3 pos;
    public float fieldOfView;
}
