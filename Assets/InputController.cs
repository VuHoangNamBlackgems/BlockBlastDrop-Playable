using BlackGemsGlobal.SeatAway.GamePlayEvent;
using Luna.Unity.FacebookInstantGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayer;
    Shape shape = null;
    void Update()
    {
       /* if (Input.GetMouseButtonDown(0) && shape == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10000f, clickableLayer))
            {
                shape = hit.transform.GetComponentInParent<Shape>();
                if (shape != null)
                {
                    shape.OnMouseButtonDown(hit.point);
                    GameEventManager.RaisedEvent(GameEventManager.EventId.StartGame);

                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && shape != null)
        {
            shape.OnMouseButtonUp();
            shape = null;
        }


        if (Input.GetMouseButton(0) && shape != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            shape.MoveShapeWithRay(ray);

        }*/
    }
}
