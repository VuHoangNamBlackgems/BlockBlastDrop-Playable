using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class LevelMarket : MonoBehaviour
{
    [SerializeField]
    private GateController[] gateController;
    private List<MultiBlockController> multiBlockController = new List<MultiBlockController>();

    Dictionary<string, int> dicTarget = new Dictionary<string, int>();
    public static LevelMarket instance;


    public List<MultiBlockController> MultiBlockControllerList => multiBlockController;

    public MultiBlockController[] multiBlockControllers;
    public ColorDataSO colorDataSo;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeSO()
    {
        foreach (var item in multiBlockControllers)
        {
           
            item.MainBlock.ChangeSO(colorDataSo);
        }
    }

    public int maxValueBlock;

    public void AddBlock(MultiBlockController multiBlock)
    {
        multiBlockController.Add(multiBlock);
        maxValueBlock = multiBlockController.Count;
    }

    public void RemoveBlock(MultiBlockController multiBlock)
    {
        multiBlockController.Remove(multiBlock);
        OnCheckWinRate();
    }

    private void OnCheckWinRate()
    {
        if(multiBlockController.Count == 0)
        {
            GameEventManager.RaisedEvent(GameEventManager.EventId.GameWin);
         //   LunaManager.instance.GoToStore();
        }
    }


    public void PlayGateAnimation(TColor color)
    {
        foreach (var gates in gateController)
        {
            foreach (var colors in gates.TColors)
            {
                if (colors == color)
                    gates.PlayAnimation();
            }

        }
    }

    public void StopGateAnimation()
    {
        foreach (var item in gateController)
        {
            item.StopAnimation();
        }
    }

}
