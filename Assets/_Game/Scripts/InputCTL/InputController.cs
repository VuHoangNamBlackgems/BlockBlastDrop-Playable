using BlackGemsGlobal.SeatAway.GamePlayEvent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    [SerializeField] BoosterManager boosterManager;

    private LayerMask layerMaskGame;

    public MultiCTL multiBlockController;
    private Vector3 _firstTouchPoint;
    private Vector3 _firstPosition;
    private Vector3 _firstEulerAngles;
    private int valueGotoStore;
    public int maxClickStore = 1;

    public bool isStartGame;
    private float Speed = 2;

    public bool isStartHammer;
    public bool isStartVacuum;
    public bool IsBooster = false;

    public static InputController Instance;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
       //  GameEventManager.RegisterEvent(GameEventManager.EventId.GameWin, ShowLose);
        GameEventManager.RegisterEvent(GameEventManager.EventId.GameLose, ShowLose);
    }
    private void OnDestroy()
    {
       //  GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameWin, ShowLose);
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.GameLose, ShowLose);
    }

    void Start()
    {
        layerMaskGame = 1 << 12;
    }
    public void SetBoosterHammer(bool isCheck)
    {
        isStartHammer = isCheck;
    }

    public void SetBoosterVaCum(bool isCheck)
    {
        isStartVacuum = isCheck;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LunaManager.instance.StartGame();
        }


        if (Input.GetMouseButtonDown(0) && multiBlockController == null && !IsOverUI())
        {
            BlockDown();
        }
        else if (Input.GetMouseButtonUp(0) && MoveBlock())
        {
            BlockUp();
        }


        if (Input.GetMouseButtonUp(0) && isStartGame)
        {
            GotoStore();
        }

    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && MoveBlock())
        {

            BlockUpdate();

        }
    }
    private int indexStore;



    private bool MoveBlock()
    {
        return multiBlockController != null && CickMove() && !IsOverUI();
    }

    private void BlockDown()
    {
        if (GetPlanePoint(Input.mousePosition, out multiBlockController))
        {

            if (isStartHammer)
            {

                boosterManager.UpdateHammerMove((MultiBlockController)multiBlockController);
                multiBlockController = null;
                return;
            }
            else if (isStartVacuum)
            {
                boosterManager.UpdateVacuumMove((MultiBlockController)multiBlockController);
                multiBlockController = null;

                return;
            }

            PlayVFX(SoundType.PickDown);

            multiBlockController.OnMoveDown();

            _firstTouchPoint = TileMapController.Ins.GetPlanePoint(Input.mousePosition);
            _firstPosition = multiBlockController.FirstPosition();

        }
    }

    private void BlockUp()
    {
        multiBlockController.OnMoveUp();
        multiBlockController = null;
        PlayVFX(SoundType.PickUp);
        if (GameManager.Ins.isBooster && IsBooster)
        {

          //  GameEventManager.RaisedEvent(GameEventManager.EventId.Idea);
        }

    }

    private void BlockUpdate()
    {


        var planePosition = TileMapController.Ins.GetPlanePoint(Input.mousePosition);
        var offset = planePosition - _firstTouchPoint;
        offset.y = 0;

        var targetPosition = _firstPosition + offset;

        float deltaX = Mathf.Abs(targetPosition.x - _firstPosition.x);
        float deltaZ = Mathf.Abs(targetPosition.z - _firstPosition.z);

        var velocity = TileMapController.Ins.GetVelocityBaseOnDistance(multiBlockController.FirstPosition(), targetPosition);
        var direction = (targetPosition - multiBlockController.FirstPosition()).normalized;
        direction.y = 0;

        Vector3 targetVelocity = velocity * direction * Speed;


        multiBlockController.UpdateVelocity(targetVelocity);

    }



    private bool CickMove()
    {
        if (!isStartHammer && !isStartVacuum)
        {
            return true;
        }
        return false;

    }
    public bool IsOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);


        return results.Count > 0;
    }

    private void GotoStore()
    {
        valueGotoStore++;

        if (valueGotoStore >= maxClickStore)
        {
            LunaManager.instance.GoToStore();
        }
    }

    private void ShowLose()
    {
        isStartGame = true;
    }

    private void PlayVFX(SoundType soundType)
    {
        SoundManager.Ins.PlayVFX(soundType);
    }

    public bool GetPlanePoint(Vector3 screenPosition, out MultiCTL multi)
    {
        multi = null;
        var ray = Camera.main.ScreenPointToRay(screenPosition);
        var isHit = Physics.Raycast(ray, out var hit, 1000, layerMaskGame);
        if (isHit)
        {
            var component = hit.collider.gameObject.GetComponentInParent<MultiCTL>();
            if (component != null)
            {
                multi = component;
                return true;
            }
        }
        return false;
    }
}
