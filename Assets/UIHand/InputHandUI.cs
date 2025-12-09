using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandUI : MonoBehaviour
{
    [SerializeField] AnimationHand animationHand;
    [SerializeField] private RectTransform rectHand;
    [SerializeField] private Canvas cv;



    void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cv.transform as RectTransform,
            Input.mousePosition,
            cv.worldCamera,
            out mousePosition
        );

        // Gán vị trí chuột cho UI
        rectHand.anchoredPosition = mousePosition;



        if (Input.GetMouseButtonDown(0))
        {
           
                animationHand.ClickUp();
          
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animationHand.ClickDown();


        }
    }
}
