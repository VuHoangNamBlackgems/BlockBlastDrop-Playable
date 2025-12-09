using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public UnityEvent acStart;
    public GameObject[] imStart;
    public TextMeshProUGUI[] txtCountDown;
    public TextMeshProUGUI[] txtlevel;

    public GameObject objHori, objVerti;
    private bool isStartGame;
    public int[] currentTimeLevels;
    public int currentTimeLevel;
    private int currentlevel = 0;

    private Coroutine countdownCoroutine;

    [SerializeField] private bool isCountDown = false;
    private bool IsUseFreezeTime;
    public static UIManager instance;

    private void Awake()
    {
        instance = this;
        GameEventManager.RegisterEvent(GameEventManager.EventId.StartGame, StartGame);


        GameEventManager.RegisterEvent(GameEventManager.EventId.GameWin, OnGameWin);
        GameEventManager.RegisterEvent(GameEventManager.EventId.BirdIQ, MoveRectBrid);
        GameEventManager.RegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerticalScreen);
        GameEventManager.RegisterEvent(GameEventManager.EventId.LandscapeScreen, OnLandscapeScreen);
    }

    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.StartGame, StartGame);


        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameWin, OnGameWin);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.BirdIQ, MoveRectBrid);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.VerticalScreen, OnVerticalScreen);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.LandscapeScreen, OnLandscapeScreen);
    }


    private void Start()
    {
        Init();
        SetCurrentTimes();

        OnStartTime();
    }

    private void Init()
    {
        AniTutorial();
    }

    private void AniTutorial()
    {
        foreach (var item in imStart)
        {
            item.transform.DOScale(Vector3.one * 1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnLandscapeScreen()
    {
        objHori.gameObject.SetActive(true);
        objVerti.gameObject.SetActive(false);
        OnChangeSize(0.65f, -180);

    }

    private void OnVerticalScreen()
    {
        objHori.gameObject.SetActive(false);
        objVerti.gameObject.SetActive(true);
        OnChangeSize(1.7f, -540f);
    }

    private void OnChangeSize(float size, float anChorY)
    {
        if (recttMission == null)
            return;

        recttMission.localScale = Vector3.one * size;
        recttMission.anchoredPosition = new Vector2(recttMission.anchoredPosition.x, anChorY);
    }

    public void StartGame()
    {
        if (isStartGame)
            return;

        isStartGame = true;

        acStart?.Invoke();
        Tutorial();
        if (isCountDown)
        {
            TextCountDown();
        }
    }

    private void TextCountDown()
    {
        if (currentTimeLevels.Length <= 0)
            return;

        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private void TextCurrentLevel()
    {
        foreach (var item in txtlevel)
        {
            item.text = "Level " + (currentlevel + 1).ToString();
        }
    }
    private void Tutorial()
    {
        foreach (var item in imStart)
        {
            item.transform.DOKill();
            item.gameObject.SetActive(false);
        }
    }
    public bool IsPopUp;

    private IEnumerator CountdownCoroutine()
    {
        SetCurrentTimes();
        while (currentTimeLevel > 0)
        {
            if (!IsUseFreezeTime)
            {
                SetCurrentTime();
                OnStartTime();
                // OnStore(); 
                yield return new WaitForSeconds(1f);

            }
            yield return null;

        }
        if (!IsPopUp)
        {

            LunaManager.instance.GoToStore();
        }
        else
        {
            GameEventManager.RaisedEvent(GameEventManager.EventId.GameLose);

        }
    }

    private void OnGameWin()
    {
        pointer = 0;
        currentlevel++;
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            if (currentTimeLevels.Length > currentlevel)
            {
                TextCountDown();
                TextCurrentLevel();
            }
        }
    }

    int pointer = 0;
    int curremtPoint = 0;
    public RectTransform rectBird;
    public RectTransform recttMission;
    public RectTransform[] rectPoints;
    private void MoveRectBrid()
    {

        if (rectPoints.Length <= 0)
            return;

        pointer++;

        float progress = Mathf.Clamp01((float)pointer / (float)LevelMarket.instance.maxValueBlock);

        float targetX = Mathf.Lerp(rectPoints[currentlevel].anchoredPosition.x, rectPoints[currentlevel + 1].anchoredPosition.x, progress);


        rectBird.DOAnchorPosX(targetX, 0.25f).SetEase(Ease.OutSine);

    }


    private void OnStartTime()
    {
        foreach (var txt in txtCountDown)
        {
            Countdown(currentTimeLevel, txt);
        }
    }

    public void AppearWarning(bool onActive)
    {
    }

    private float SetCurrentTime()
    {
        currentTimeLevel--;
        return currentTimeLevel;
    }


    private void SetCurrentTimes()
    {
        if (currentTimeLevels.Length > 0)
            currentTimeLevel = currentTimeLevels[currentlevel];
    }

    public void StopTime()
    {
        IsUseFreezeTime = true;
    }

    public void RunTime()
    {

        IsUseFreezeTime = false;
    }

    private void Countdown(float currentTime, TextMeshProUGUI txtCountDown)
    {
        int hours = (int)currentTime / 3600;
        int minutes = ((int)currentTime % 3600) / 60;
        int seconds = (int)currentTime % 60;

        if (txtCountDown != null && hours == 0)
            txtCountDown.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
        else if (txtCountDown != null && hours >= 1)
            txtCountDown.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }


}
