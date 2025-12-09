using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlockTimeConstantController : MonoBehaviour
{
    [SerializeField] ParticleSystem _explosion;
    [SerializeField] Transform objBomb;
    [SerializeField] TextMeshPro txtTime;
    [SerializeField] int _currentTime = 30;
    bool isStartGame = false;
    private Coroutine countdownCoroutine;
    // [SerializeField] private
    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.StartGame, StartGame);
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameLose, GameLose);

    }
    private void OnDestroy()
    {   
        
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.StartGame, StartGame);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameLose, GameLose);

    }


    private void Start()
    {
        txtTime.text = _currentTime.ToString();
    }
    private void GameLose()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }

    private void StartGame()
    {
        if (isStartGame)
            return;

        isStartGame = true;

        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (_currentTime > 0)
        {

            SetCurrentTime();
            OnStartTime();
            yield return new WaitForSeconds(1f);


            yield return null;

        }
     //   Debug.Log("SSSS");
      //  GameEventManager.RaisedEvent(GameEventManager.EventId.GameLose);

        _explosion.Play();
        objBomb.gameObject.SetActive(false);
        txtTime.gameObject.SetActive(false);
       // LunaManager.instance.GoToStore();
    }

    private float SetCurrentTime()
    {
        _currentTime--;
        return _currentTime;
    }

    private void OnStartTime()
    {
       // Countdown(_currentTime, txtTime);
        txtTime.text = _currentTime.ToString();
    }

    private void Countdown(float currentTime, TextMeshPro txtCountDown)
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
