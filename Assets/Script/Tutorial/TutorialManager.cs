using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private List<TutorialStep> listTutorial = new List<TutorialStep>();

    private void Awake()
    {
        Instance = this;
    }

    public void ShowTutorial()
    {
        if (listTutorial.Count > 0)
            listTutorial.Clear();
        listTutorial = new List<TutorialStep>(GetComponentsInChildren<TutorialStep>());
        listTutorial = listTutorial.OrderBy(s => s.stepOrder).ToList();
        if (listTutorial.Count > 0 && UITutorial.Instance != null)
        {
            UITutorial.Instance.PrepareShow(listTutorial[0]);
            UITutorial.Instance.gameObject.SetActive(true);
            UITutorial.Instance.PlayHandAnim();
        }
    }    
}
