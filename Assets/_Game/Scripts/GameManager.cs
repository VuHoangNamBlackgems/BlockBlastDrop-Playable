using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] objLevel;
    private int indexLevel = 0;

    public GameObject[] objWin;
    public GameObject[] objLose;


    public static GameManager Ins;
    public bool isBooster;
    private void Awake()
    {
        Ins = this;
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameWin, ShowWin);
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameLose, ShowLose);
    }

    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameWin, ShowWin);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameLose, ShowLose);
    }
    // Start is called before the first frame update
    void Start()
    {
        objLevel[indexLevel].SetActive(true);
    }

    public void ShowWin()
    {
        PlayVFX(SoundType.Win);
        DOVirtual.DelayedCall(0.5F, () =>
        {
            OnNextLevel();
        });

    }
    public void ShowLose()
    {
        PlayVFX(SoundType.Lose);
         AppearActive(objLose);
    }


    private void OnNextLevel()
    {
        GameEventManager.RaisedEvent(GameEventManager.EventId.Effect_2);

        if (indexLevel < objLevel.Length - 1)
        {
            isBooster = true;
            var currentLevel = objLevel[indexLevel];
            var nextLevel = objLevel[indexLevel + 1];

            Vector3 centerPos = Vector3.zero;
            Vector3 leftPos = Vector3.left * 20f;
            Vector3 rightPos = Vector3.right * 20f;

            // Level mới xuất hiện ở trái, level cũ ở giữa
            nextLevel.transform.position = leftPos;
            nextLevel.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.3f);
            seq.Append(
                DOTween.To(
                    () => 0f,t =>
                    {
                        currentLevel.transform.position = Vector3.Lerp(centerPos, rightPos, t);
                        nextLevel.transform.position = Vector3.Lerp(leftPos, centerPos, t);
                    },
                    1f,
                    0.6f
                ).SetEase(Ease.OutQuad)
            );
            seq.OnComplete(() =>
            {
                GameEventManager.RaisedEvent(GameEventManager.EventId.Idea);

                currentLevel.SetActive(false);
                indexLevel++;
            });
        }
        else
        {
            LunaManager.instance.GoToStore();
        }
    }



    private void AppearActive(GameObject[] obj)
    {
        foreach (var item in obj)
        {
            item.gameObject.SetActive(true);
        }
    }
    private void PlayVFX(SoundType soundType)
    {
        SoundManager.Ins.PlayVFX(soundType);
    }


}
