using System;
using System.Collections;
using System.Collections.Generic;
using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using UnityEngine;

public interface MultiCTL
{
    void UpdateVelocity(Vector3 velocity);
    Vector3 FirstTouchPoint();
    Vector3 FirstPosition();
    void OnMoveDown();
    void OnMoveUp();



}
public class MultiBlockController : MonoBehaviour, MultiCTL
{
    private const float MAX_VELOCITY = 100;




    private Transform _moveTransform;
    public Transform MoveTransform => _moveTransform;
    [SerializeField] private Transform _itemHolder;
    public Transform ItemHolder => _itemHolder;
    [SerializeField] private List<BlockController> _blockControllers;
    public List<BlockController> BlockControllers => _blockControllers;
    private BlockController _mainBlock => _blockControllers[0];
    public BlockController MainBlock => _mainBlock;
    private Rigidbody _rigidbody;
    private bool _isMoving;
    private Coroutine _pickUpCoroutine;
    private bool _couldDragDrop = true;
    private float _lastMoveToGateTime = float.MinValue;



    private IKey _key;
    private IUnlockable _unlockable;

    private bool _arrowHori, _arrowVerti;
    private ArrowHori _ArrowHori;
    private ArrowVerti _ArrowVerti;
    [HideInInspector]
    public bool DestroySound;
    private Emoji _emoji;

    private void Awake()
    {
        _key = GetComponentInChildren<IKey>();
        _unlockable = GetComponentInChildren<IUnlockable>();


        _emoji = GetComponentInChildren<Emoji>();

        _rigidbody = GetComponent<Rigidbody>();
        _ArrowHori = GetComponentInChildren<ArrowHori>();
        _ArrowVerti = GetComponentInChildren<ArrowVerti>();
    }

    private void Start()
    {
        Init();
    }

    public void ActiveCollider()
    {
        _key?.MoveKey();
        _mainBlock.AppCollider();

    }
    public Emoji Emoji()
    {
        return _emoji;
    }
    private void Init()
    {

        _moveTransform = transform;

        OnSpawn();
        InitialezedMove();
        AddMulBlock();
        Setup(_moveTransform.position, _moveTransform.eulerAngles);
    }
    private void InitialezedMove()
    {
        if (_ArrowVerti != null)
        {
            _arrowVerti = true;

            _ArrowVerti.gameObject.SetActive(true);

        }
        else if (_ArrowHori != null)
        {
            _arrowHori = true;
            _ArrowHori.gameObject.SetActive(true);
        }
    }

    public Tween BlockAnim()
    {
        Sequence se = DOTween.Sequence();
        se.Append(transform.DOScale(Vector3.zero, 0.4F)).OnComplete(() =>
        {
            AppearColor(false);
        });

        return se;
      
    }

    public void AppearColor(bool isCheck)
    {
        gameObject.SetActive(isCheck);
    }

    private void AddMulBlock()
    {
        LevelMarket.instance.AddBlock(this);
    }
    private void RemoveMulBlock()
    {
        LevelMarket.instance.RemoveBlock(this);
    }

    public void OnSpawn()
    {
       // _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        DisablePhysic();
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
        if (IsIce())
            return;

        _isMoving = true;

        BeSelected();
        _mainBlock.SetColliderOnPickUp();
        PlayAnimation();
    }


    void SetUpPickUp()
    {
        if (IsIce())
            return;

        _mainBlock.SetColliderOnDrop();

        UnSelect();
        MoveToNearestTile(0.06f);
        StopAnimation();

        _isMoving = false;

    }


    public Vector3 FirstTouchPoint()
    {
        return TileMapController.Ins.GetPlanePoint(Input.mousePosition);
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
      
        if (IsIce())
            return;

        if (_unlockable != null)
        {
            if (!_unlockable.IsFullyUnlocked())
            {
                return;
            }
        }

        if (_arrowHori)
            UpdateVelocityArrow(targetVelocity, true);
        else if (_arrowVerti)
            UpdateVelocityArrow(targetVelocity, false);
        else
            _rigidbody.velocity = Vector3.ClampMagnitude(targetVelocity, 180f);
    }

    private void UpdateVelocityArrow(Vector3 targetVelocity, bool isType)
    {

        Vector3 velocity = _rigidbody.velocity;
        if (isType)
        {

            velocity.x = targetVelocity.x;
            velocity.z = 0;
            velocity.y = 0;
        }
        else
        {
            velocity.z = targetVelocity.z;
            velocity.x = 0;
            velocity.y = 0;

        }
        _rigidbody.velocity = Vector3.ClampMagnitude(velocity, 50f);
    }

    public void Setup(Vector3 transformDataPosition, Vector3 rotation)
    {       SetRigidbodyConstant(true, true);

       // StartCoroutine(IeSetup(transformDataPosition, rotation));
    }

    private IEnumerator IeSetup(Vector3 transformDataPosition, Vector3 rotation)
    {
        transformDataPosition = RoundToIntAndHalf(transformDataPosition);
        rotation = RoundToIntAndHalf(rotation);
        _moveTransform.position = transformDataPosition;
        _moveTransform.eulerAngles = rotation;
       SetRigidbodyConstant(true, true);
        yield return new WaitForSeconds(1);
        MoveToNearestTile(0);
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

    private bool IsIce()
    {
        return _mainBlock.IsIceBlock;
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
    public bool CouldDragDrop()
    {
        return _couldDragDrop;
    }

    public void ContinueDragDrop()
    {
        _couldDragDrop = true;
        _isMoving = false;
    }

    private void PlayAnimation()
    {
       // _mainBlock.PlayAnimation();

        LevelMarket.instance.PlayGateAnimation(_mainBlock.TColor);
    }

    private void StopAnimation()
    {
        //_mainBlock.StopAnimation();
        LevelMarket.instance.StopGateAnimation();

    }


    private void BeSelected()
    {
        EnablePhysic();
    }
    private void UnSelect()
    {
        DisablePhysic();
    }
    private void MoveToNearestTile(float duration)
    {
        var nearestTile = TileMapController.Ins.GetNearestTile(_mainBlock.CenterOfSingleBlock.position);
        MoveToTile(nearestTile, duration);

    }
    public void MoveToTile(TileController tileController, float duration)
    {
        var offset = tileController.MoveTransform.position - _mainBlock.CenterOfSingleBlock.position;

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
    public BlockController MoveToGate(GateController gate, int blockSize)
    {
        _lastMoveToGateTime = Time.time;

        if (_pickUpCoroutine != null)
            StopCoroutine(_pickUpCoroutine);

        _couldDragDrop = false;
        DisablePhysic();
        UnSelect();

        var tile = TileMapController.Ins.GetNearestTileGate(MainBlock.CenterOfSingleBlock.position, MainBlock.CenterTf.position, gate, blockSize);
        MoveToTile(tile, CONSTANTS.MOVE_TO_GATE_DURATION);
        return MainBlock;
    }
    public void AddBlock(BlockController blockController)
    {
        if (!_blockControllers.Contains(blockController))
        {
            _blockControllers.Add(blockController);
            var offsetY = 0.85f * (_blockControllers.Count - 1);
            blockController.SetYOffset(offsetY);
        }
    }
    public void RemoveBlock(BlockController blockController, float delay = -1)
    {
        if (_pickUpCoroutine != null)
            StopCoroutine(_pickUpCoroutine);
        if (_blockControllers.Contains(blockController))
        {

            if (_blockControllers.Count > 0)
            {
                RemoveBlock(blockController);

            }

            if (_blockControllers.Count == 0)
            {

                RemoveMulBlock();
                DestroySelf();
            }
            else
            {
                DestroySound = false;
                _mainBlock.SetUpStart();
    
            }

        }
    }

    private void RemoveBlock(BlockController blockController)
    {
        _blockControllers.Remove(blockController);

    }

    private void DestroySelf()
    {
        _key?.MoveKey();
        LevelMarket.instance.StopGateAnimation();
        InputController.Instance.multiBlockController = null;
    }
    public bool ShouldCheckByGate()
    {
        return _isMoving && (Time.time - _lastMoveToGateTime) > 0.4f;
    }
    public void SetRigidbodyConstant(bool x, bool z)
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        if (!x)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ
                | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;
        }
        if (!z)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ
                                   | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void LockMove()
    {
        _couldDragDrop = false;
    }

    public void UnLockMove()
    {
        _couldDragDrop = true;
    }




    public List<BlockController> GetVocuumBlocksByColor(ColorDataSO color)
    {
        var removeBlocks = new List<BlockController>();
        foreach (var blockController in _blockControllers)
        {
            if (!blockController.IsHideColor && color.IsContainColor(blockController.ColorDataSo))
            {
                removeBlocks.Add(blockController);
            }
        }
        return removeBlocks;
    }
}
