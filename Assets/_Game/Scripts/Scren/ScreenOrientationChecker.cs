using BlackGemsGlobal.SeatAway.GamePlayEvent;
using UnityEngine;

public class ScreenOrientationChecker : MonoBehaviour
{

    [SerializeField] TScreen _currentScreen;

    public TScreen currentScreen => _currentScreen;

    public bool isChange;


    [SerializeField] private GameObject obj_Hori;
    [SerializeField] private GameObject obj_Verti;


    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.LandscapeScreen, OnHori);
        GameEventManager.RegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerti);
    }
    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.LandscapeScreen, OnHori);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerti);
    }

    private void Start()
    {
        OnLoadScreen();
    }

    private void OnHori()
    {
        obj_Hori.gameObject.SetActive(true);
        obj_Verti.gameObject.SetActive(false);
    }
    private void OnVerti()
    {
        obj_Hori.gameObject.SetActive(false);
        obj_Verti.gameObject.SetActive(true);
    }

    public void OnLoadScreen()
    {
        isChange = true;
    }

    private void Update()
    {
        if (isChange)
            OnChangeScreen();
    }

    private void OnChangeScreen()
    {
        TScreen newScreen = GetCurrentScreen();

        if (newScreen != _currentScreen)
        {
            OnScreenChanged(newScreen);

            _currentScreen = newScreen;
        }
    }

    private TScreen GetCurrentScreen()
    {
        if (Screen.width > Screen.height)
        {
            return TScreen.LandscapeScreen;
        }
        else
        {
            return TScreen.VerticalScreen;
        }
    }

    private void OnScreenChanged(TScreen screen)
    {
        if (screen == TScreen.LandscapeScreen)
        {
            GameEventManager.RaisedEvent(GameEventManager.EventId.LandscapeScreen);
        }
        else
        {
            GameEventManager.RaisedEvent(GameEventManager.EventId.VerticalScreen);
        }
    }

}
public enum TScreen
{
    None,
    LandscapeScreen,
    VerticalScreen
}
