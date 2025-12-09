using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleMultiController : MonoBehaviour, MultiCTL
{
    private Transform _moveTransform;

    private Rigidbody _rigidbody;

    public Transform _mainBlock;

    public Transform MainBlock => (BlockControllers.Count > 0 && BlockControllers.Count <= 1)
        ? BlockControllers[0].CenterOfSingleBlock
        : _mainBlock;

    public Transform objMesh;

    public List<BlockController> BlockControllers = new List<BlockController>();




    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {

        _moveTransform = transform;
    }


    public void OnMoveDown()
    {
        GameEventManager.RaisedEvent(GameEventManager.EventId.StartGame);
        SetUpIePickDown();
    }


    public void OnMoveUp()
    {
        SetUpPickUp();
    }



    void SetUpIePickDown()
    {

        BeSelected();
    }


    void SetUpPickUp()
    {

        UnSelect();
        MoveToNearestTile(0.06f);

    }


    public void AppearActive(BlockController blockController)
    {
        objMesh.gameObject.SetActive(false);

        if (BlockControllers.Count > 0)
        {
            BlockControllers.Remove(blockController);
        }

    }


    public Vector3 FirstTouchPoint()
    {
        return TileMapController.Ins.GetPlanePoint(UnityEngine.Input.mousePosition);
    }

    public Vector3 FirstPosition()
    {
        return _rigidbody.position;
    }

    public Vector3 FirstEuler()
    {
        return _rigidbody.rotation.eulerAngles;
    }


    public void UpdateVelocity(Vector3 targetVelocity)
    {
        UpdateVelocityArrow(targetVelocity);
    }

    private void UpdateVelocityArrow(Vector3 targetVelocity)
    {
        Vector3 velocity = _rigidbody.velocity;

        _rigidbody.velocity = targetVelocity;
    }
    public Vector3 RoundToIntAndHalf(Vector3 vector)
    {
        vector *= 2f;
        vector.x = Mathf.RoundToInt(vector.x);
        vector.y = Mathf.RoundToInt(vector.y);
        vector.z = Mathf.RoundToInt(vector.z);
        vector *= 0.5f;
        return vector;
    }


    private void DisablePhysic()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
    }
    private void EnablePhysic()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void BeSelected()
    {
        EnablePhysic();
        foreach (var blockController in BlockControllers)
        {
            blockController.SetColliderOnPickUp();
        }
    }
    private void UnSelect()
    {
        DisablePhysic();
        foreach (var blockController in BlockControllers)
        {
            blockController.SetColliderOnDrop();
        }
    }
    private void MoveToNearestTile(float duration)
    {
        var nearestTile = TileMapController.Ins.GetNearestTile(MainBlock.position);
        MoveToTile(nearestTile, duration);

    }
    public void MoveToTile(TileController tileController, float duration)
    {
        var offset = tileController.MoveTransform.position - MainBlock.position;

        MoveToOffset(offset, duration);
    }
    private void MoveToOffset(Vector3 offset, float duration)
    {
        offset.y = 0;
        var targetPosition = _moveTransform.position + offset;


        if (duration < Mathf.Epsilon)
        {
            if (_moveTransform != null)
            {
                _moveTransform.DOKill();

                _moveTransform.position = RoundToIntAndHalf(targetPosition);
                _moveTransform.transform.DORotate(RoundToIntAndHalf(_moveTransform.eulerAngles), duration);
            }
        }
        else
        {

            if (_moveTransform != null)
            {
                _moveTransform.DOKill();


                _moveTransform.transform.DOMove(RoundToIntAndHalf(targetPosition), duration);
                _moveTransform.transform.DORotate(RoundToIntAndHalf(_moveTransform.eulerAngles), duration);
            }


        }
    }


}
