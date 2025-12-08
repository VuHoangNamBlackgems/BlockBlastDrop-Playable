using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class UITutorial : MonoBehaviour
{
    public static UITutorial Instance;
    [SerializeField] RectTransform CanvasRect;
    [SerializeField] RectTransform Hand;
    [SerializeField] float distance = 150f;
    [SerializeField] float duration = 0.6f;

    private Camera mainCam;
    private Vector2 startPos;
    private Vector2 direction;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

//    private void Update()
//    {
//#if UNITY_EDITOR
//        if (Input.GetMouseButtonDown(0))
//            gameObject.SetActive(false);
//#else
//        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
//            gameObject.SetActive(false);
//#endif
//    }

    public void PrepareShow(TutorialStep tutorialStep)
    {
        Vector2 localPos;
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(mainCam, tutorialStep.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect,screenPos,null,out localPos);

        startPos = localPos;
        Hand.anchoredPosition = startPos;

        switch (tutorialStep.stepDirection)
        {
            case StepDir.Up:
                {
                    direction = Vector2.up;
                    break;
                }
            case StepDir.Down:
                {
                    direction = Vector2.down;
                    break;
                }
            case StepDir.Left:
                {
                    direction = Vector2.left;
                    break;
                }
            case StepDir.Right:
                {
                    direction = Vector2.right;
                    break;
                }
        }
    }

    public void PlayHandAnim()
    {
        Vector2 endPos = startPos + direction.normalized * distance;
        Hand.DOKill();
        Hand.DOAnchorPos(endPos, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart);
    }
}
