using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
    public InputController inputController;
    public MultiBlockVehicle blockVehicle;
    public UIManager uIManager;
    public Transform[] objBG;
    private bool isMission;


    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.Idea_2, Init);
    }

    private void OnDestroy()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Idea_2, Init);
    }

    private void Init()
    {
        if (isMission)
            return;

        isMission = true;

        AppearActive(true);
        blockVehicle.StopTime();

    }



    public void AppearPop()
    {
        AppearActive(true);
        blockVehicle.StopTime();

    }


    /// <summary>
    /// Start  Btn
    /// </summary>
    public void OnTimeFreeze()
    {
        SelectBooster(TypeBooster.FreeTime);

    }

    public void OnDestroyBlock()
    {
        SelectBooster(TypeBooster.Destroy);
        

    }

    public void OnMagnetBlock()
    {
        SelectBooster(TypeBooster.Magnet);
    }



    /// <summary>
    /// End Btn
    /// </summary>
   
    public void UpdateHammerMove(MultiBlockController multiBlockController)
    {

        blockVehicle.HammerBoosterMove(multiBlockController, () =>
        {   
            inputController.SetBoosterHammer(false);
            SoundManager.Ins.PlayVFX(SoundType.Hammer);
            LevelMarket.instance.RemoveBlock(multiBlockController);
        });
    }


    public void UpdateVacuumMove(MultiBlockController multiBlockController)
    {

        blockVehicle.VacuumBoosterMove(multiBlockController, () =>
        {
            inputController.SetBoosterVaCum(false);

        });
    }

    private void SelectBooster(TypeBooster typeBooster)
    {
        switch (typeBooster)
        {
            case TypeBooster.FreeTime:
                AppearActive(false);
                blockVehicle.TimeFreeze();
                break;
            case TypeBooster.Destroy:
                AppearActive(false);
                blockVehicle.DestroyBlock();
                break;
            case TypeBooster.Magnet:
                AppearActive(false);
                blockVehicle.MagnetBlock();
                break;
        }
    }
    
    private void AppearActive(bool onCheck)
    {
        foreach (Transform t in objBG)
        {
            t.gameObject.SetActive(onCheck);
        }
    }
}

public enum TypeBooster
{
    FreeTime,
    Destroy,
    Magnet
}
