using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private DoorController DoorPrefab;
    [SerializeField] private GameObject DoorParent;
    [SerializeField] private DoorController[] DoorList;
    public float doorSpacing = 1f;
    [HideInInspector] public List<int> StandStatus = new List<int>();

    private void Start()
    {
        SpawnDoors();
    }
    public void SpawnDoors()
    {
        foreach (Transform child in DoorParent.transform) Destroy(child.gameObject);
        DoorList = new DoorController[5];
        StandStatus = new List<int>();
        float middleIndex = (5 - 1) / 2f;
        for (int i = 0; i < 5; i++)
        {
            StandStatus.Add(-1);
            DoorController door = Instantiate(DoorPrefab, DoorParent.transform);
            door.name = $"Door_{i + 1}";
            float xPosition = (i - middleIndex) * doorSpacing;
            door.transform.localPosition = new Vector3(xPosition, 0f, 0f);
            DoorList[i] = door;

            door.SetupTheme(BoardController.Instance.currentTheme);
        }
        for (int i = 0; i < StandStatus.Count; i++) StandStatus[i] = -1;
    }

    private void ClearAllStands()
    {
        for (int i = 0; i < DoorList.Length; i++)
        {
            Transform sp = DoorList[i].StandPosition.transform;
            for (int c = sp.childCount - 1; c >= 0; c--) Destroy(sp.GetChild(c).gameObject);
        }
    }

    public void UpdateAllDoorColors()
    {
        for (int i = 0; i < DoorList.Length && i < StandStatus.Count; i++) UpdateDoorColor(i);
    }

    private void UpdateDoorColor(int standIndex)
    {
        if (standIndex < 0 || standIndex >= DoorList.Length || standIndex >= StandStatus.Count) return;
        DoorController door = DoorList[standIndex];
        int colorId = StandStatus[standIndex];
        if (colorId == -1) door.ChangeBorderColor((ColorId)(-1));
        else door.ChangeBorderColor((ColorId)colorId);
    }

    public void OpenDoor(int standIndex)
    {
        if (standIndex >= 0 && standIndex < DoorList.Length) DoorList[standIndex].doorAnimator.SetTrigger("OpenDoor");
    }

    public Transform GetStandPosition(int standIndex)
    {
        if (standIndex >= 0 && standIndex < DoorList.Length) return DoorList[standIndex].StandPosition.transform;
        return null;
    }

    public int GetStandCount()
    {
        return DoorList.Length;
    }
    public int FindEmptyStand()
    {
        for (int i = 0; i < StandStatus.Count; i++) if (StandStatus[i] == -1) return i;
        return -1;
    }
    public void SetStandStatus(int standIndex, int colorId)
    {
        if (standIndex >= 0 && standIndex < StandStatus.Count)
        {
            StandStatus[standIndex] = colorId;
            UpdateDoorColor(standIndex);
        }
    }
    public void PrintStandStatus()
    {
        string status = "Stand Status: ";
        for (int i = 0; i < StandStatus.Count; i++)
        {
            if (StandStatus[i] == -1) status += "Empty ";
            else
            {
                string colorName = GetColorName(StandStatus[i]);
                status += $"{colorName}(ID:{StandStatus[i]}) ";
            }
        }
        //TODO: enable debug
        // Debug.Log(status);
    }

    private string GetColorName(int colorId)
    {
        if (colorId < 0 || colorId >= GameConfig.Instance.GetColorCount()) return "Unknown";
        if (System.Enum.IsDefined(typeof(ColorId), colorId)) return ((ColorId)colorId).ToString();
        return GameConfig.Instance.GetColorCanon(colorId).name;
    }

    public void ResetLobby()
    {
        ClearAllStands();
        SpawnDoors();
    }
}
