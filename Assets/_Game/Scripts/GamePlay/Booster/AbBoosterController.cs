//using datmonolib;
using UnityEngine;

public abstract class AbBoosterController : MonoBehaviour
{
   // [SerializeField, InspectorReadOnly] protected BoosterDataSO _boosterData;
    public bool EnableOtherBoosterWhenDone = true;
    #if UNITY_EDITOR
   /* public void SetBoosterData(BoosterDataSO boosterData)
    {
        _boosterData = boosterData;
      //  UnityEditorExtensions.RenameAsset(gameObject, _boosterData.BoosterName);
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }*/
    #endif
    public virtual void StartUseBooster()
    {
      //  GamePlayManager.EventStartUseBooster.Invoke(_boosterData);
       // GameAnalManager.Ins.g_start_use_booster(_boosterData.BoosterName);
    }

    public virtual void CancelUsingBooster()
    {
        if(!CouldCancel()) return;
      //  GamePlayManager.EventCancelUseBooster.Invoke(_boosterData);
     //   GameAnalManager.Ins.g_cancel_use_booster(_boosterData.BoosterName);
    }

    public abstract bool CouldCancel();
    public virtual void StopUsingBooster()
    {
     //   GamePlayManager.EventEndUseBooster.Invoke(_boosterData);
    }
    public virtual void UseBoosterSuccess()
    {
      //  UserManager.Ins.RemoveItem(_boosterData,1,"UseBooster");
       // GameAnalManager.Ins.g_complete_use_booster(_boosterData.BoosterName);
    }
}
