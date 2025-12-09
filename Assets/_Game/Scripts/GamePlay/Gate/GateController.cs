using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum TColor
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Orange = 3,
    Purple = 4,
    Yellow = 5,
    Pink = 6,
    Ryan = 7,
    DaskBlue = 10,
    DaskGreen = 9,
    Black,
    White,
    DarkYellow,
    DarkPink

}
public class GateController : MonoBehaviour
{

    [SerializeField] private Transform _gateTriggers;
    [SerializeField] private Transform _skinMesh;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _blockOutParticleHolder;
    [SerializeField] private GameObject objSkinMesh;
    [SerializeField] private GameObject objMesh;

    [SerializeField] private ColorDataSO[] _colorDatas;

    [SerializeField] private int _tileCount = 1;
    public int TileCount => _tileCount;
    private DirecType _direcType;

    private TColor[] _tColors;
    public TColor[] TColors => _tColors;

    private Transform _moveTransform;
    public Transform MoveTransform => _moveTransform;

    private int _cycleCount;
    private List<Transform> _currentColliders = new List<Transform>();
    private SkinnedMeshRenderer[] _colorSkinRenderers;
    private MeshRenderer[] _colorRenderers;
    private GateTriggetController _gateTriggetCTL;
    public WallGate _wallGate;
    [SerializeField] bool _isMoveBlock = true;
    public bool IsMoveBlock
    {
        get => _isMoveBlock;
        set => _isMoveBlock = value;
    }

    private void Awake()
    {

        _moveTransform = transform;
        InitGet();
        IeSetup(_moveTransform.position, _moveTransform.eulerAngles);
        RegisterEvents();
    }


    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents() 
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.ToggleGate, ToggleGate);

    }

    private void UnregisterEvents()
    {
        GameEventManager.UnRegisterEvent(GameEventManager.EventId.ToggleGate, ToggleGate);

    }


    private void InitGet()
    {
        if (_gateTriggers != null)
        {
            _gateTriggetCTL = _gateTriggers.GetComponentInChildren<GateTriggetController>();
        }

        if (objSkinMesh != null)
        {
            _wallGate = objSkinMesh.GetComponentInChildren<WallGate>();
            _colorSkinRenderers = objSkinMesh.GetComponentsInChildren<SkinnedMeshRenderer>();
        }
        else if (objMesh != null)
        {

            _colorRenderers = objMesh.GetComponentsInChildren<MeshRenderer>();
        }



    }

    private void ToggleGate()
    {
        if(_gateTriggetCTL == null) return;


        if (IsMoveBlock)
            _gateTriggetCTL.OpenGate();
        else
            _gateTriggetCTL.CloseGate();


        IsMoveBlock = !IsMoveBlock;
    }



    private void GateOpen()
    {
      //  _wallGate.PlayGateAnimation(_tileCount);
     //   _animator.SetTrigger("open");
    }

    private void GateClose()
    {
       // _animator.SetTrigger("close");
    }
    public void PlayAnimation()
    {
        _skinMesh.transform.DOScaleY(1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo);
        _skinMesh.transform.DOScaleZ(1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo);

    }

    public void StopAnimation()
    {
        DOTween.Kill(_skinMesh.transform);
        _skinMesh.transform.DOScale(Vector3.one, 0.4f);

    }



    private void IeSetup(Vector3 pos, Vector3 euler)
    {
        pos = RoundToIntAndHalf(pos);
        euler = RoundToIntAndHalf(euler);

        //   _renderTransform.localScale = new Vector3(_tileCount , 1, 1);


        _direcType = DirecType.Row;
        if (Mathf.Abs(_moveTransform.forward.z) > 0.5f)
        {
            _direcType = DirecType.Column;
        }
        SetNewColor();
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

    public void SetNewColor()
    {
        _tColors = new TColor[_colorDatas.Length];

        for (int i = 0; i < _colorDatas.Length; i++)
        {
            _colorDatas[i].SetGateColor(this);
            _tColors[i] = _colorDatas[i].GetColorType();
        }
    }


    int colorRender = 0;

    public void SetMaterial(Material material)
    {
        _colorRenderers[colorRender].sharedMaterial = material;
        colorRender++;
    }

    private List<Collider> _colliders = new List<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (_colliders.Contains(other)) return;
        _colliders.Add(other);
        StartCoroutine(Check(other.attachedRigidbody.transform));

        var blockCTL = other.gameObject.GetComponentInParent<BlockController>();
        if (blockCTL != null)
        {
            // Debug.Log("Other " + other.name);
            StartCoroutine(CheckMulti(other.attachedRigidbody.transform, blockCTL));
        }

        if (_currentColliders.Contains(other.attachedRigidbody.transform)) return;
        _currentColliders.Add(other.attachedRigidbody.transform);

        if (_currentColliders.Count((v) => { return v == other.attachedRigidbody.transform; }) <= 1)
        {
            StartCoroutine(Check(other.attachedRigidbody.transform));

        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            _colliders.Remove(other);
            _currentColliders.Remove(other.attachedRigidbody.transform);
        }
    }

    private IEnumerator Check(Transform hitTf)
    {
        if (!hitTf.TryGetComponent<MultiBlockController>(out var multiBlockController) || !_isMoveBlock)
            yield break;


        var MainBlock = multiBlockController.MainBlock;
        var ColorDataSo = MainBlock.ColorDataSo;
        var colorBlock = false;

        foreach (var color in _colorDatas)
        {
            if (color.IsContainColor(ColorDataSo))
                colorBlock = true;
        }

        if (!colorBlock)
            yield break;

        // container check( size check)
        var blockSize = _direcType == DirecType.Row
            ? MainBlock.BlockHeight : MainBlock.BlockWidth;



        if (blockSize > _tileCount)
        {
            yield break;
        }
        while (_currentColliders.Contains(hitTf))
        {
            if (!multiBlockController.ShouldCheckByGate()) yield break;

            var leftPoint = -1 * ((_tileCount - 1f) / 2) * CONSTANTS.OFFSET_BETWEEN_TILE;

            var rightPoint = -leftPoint;
            rightPoint += Mathf.Epsilon;
            var hitBlockCount = 0;
            for (var i = leftPoint; i <= rightPoint; i += CONSTANTS.OFFSET_BETWEEN_TILE)
            {
                for (int j = -1; j <= 1; j++)
                {
                    var worldPoint = _moveTransform.position + _moveTransform.right * i + _moveTransform.right * j * 0.1f;
                    worldPoint += Vector3.up;
                    var direction = _moveTransform.forward * -1;
                    var ray = new Ray(worldPoint, direction);
                    var isHit = Physics.Raycast(ray, out var hitInfo, 50, TileMapController.Ins.DetechColliderLayer,
                        QueryTriggerInteraction.Ignore);
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10, Color.red, 2f);
                    if (isHit)
                    {
                        if (hitInfo.rigidbody != null)
                        {
                            if (hitInfo.rigidbody.TryGetComponent<MultiBlockController>(out var checkMultiBlock))
                            {
                                if (checkMultiBlock == multiBlockController)
                                {
                                    hitBlockCount++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (hitBlockCount >= blockSize)
            {
                _cycleCount++;
                var cacheCycle = _cycleCount;
                // done
                // 
                if (!multiBlockController.ShouldCheckByGate()) yield break;
                var block = multiBlockController.MoveToGate(this, blockSize);
                var blockMoveOutSize = _direcType != DirecType.Row
                                    ? block.BlockHeight : block.BlockWidth;



                multiBlockController.DestroySound = true;
                EmojiManager.Ins.ResetPos();

                GateOpen();
                yield return new WaitForSeconds(CONSTANTS.MOVE_TO_GATE_DURATION);
                var targetMoveOutPosition = block.MoveTransform.position + _moveTransform.forward * (blockMoveOutSize * CONSTANTS.OFFSET_BETWEEN_TILE + 1);
                block.ScaleBlock();
                CreateEffect(ColorDataSo, SetDirector());
                ActionEvent();

                var moveOutDuration = block.MoveOutGate(this, targetMoveOutPosition);
                var xLocalOffset = _blockOutParticleHolder.InverseTransformPoint(block.CenterTf.position).x;




                yield return new WaitForSeconds(moveOutDuration);

                if (multiBlockController != null)
                {
                    multiBlockController.ContinueDragDrop();
                }
                yield break;
            }
            yield return null;
        }
    }


    private IEnumerator CheckMulti(Transform hitTf, BlockController blockCTL)
    {
        if (!hitTf.TryGetComponent<DoubleMultiController>(out var multiBlockController))
            yield break;




        // color check#
        var MainBlock = multiBlockController.MainBlock;

        var ColorDataSo = blockCTL.ColorDataSo;
        var colorBlock = false;
        foreach (var color in _colorDatas)
        {
            if (color.IsContainColor(blockCTL.ColorDataSo))
                colorBlock = true;
        }
        if (!colorBlock)
        {
            yield break;
        }

        // container check( size check)
        var blockSize = _direcType == DirecType.Row
            ? blockCTL.BlockHeight : blockCTL.BlockWidth;

        if (blockSize > _tileCount)
        {

            yield break;
        }



        var leftPoint = -1 * ((_tileCount - 1f) / 2) * CONSTANTS.OFFSET_BETWEEN_TILE;
        var rightPoint = -leftPoint;
        rightPoint += Mathf.Epsilon;
        var hitBlockCount = 0;
        for (var i = leftPoint; i <= rightPoint; i += CONSTANTS.OFFSET_BETWEEN_TILE)
        {
            for (int j = -1; j <= 1; j++)
            {
                var worldPoint = _moveTransform.position + _moveTransform.right * i + _moveTransform.right * j * 0.1f;
                worldPoint += Vector3.up;
                var direction = _moveTransform.forward * -1;
                var ray = new Ray(worldPoint, direction);
                var isHit = Physics.Raycast(ray, out var hitInfo, 50, TileMapController.Ins.DetechColliderLayer,
                    QueryTriggerInteraction.Ignore);
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10, Color.red, 2f);
                if (isHit)
                {
                    if (hitInfo.rigidbody != null)
                    {
                        if (hitInfo.rigidbody.TryGetComponent<DoubleMultiController>(out var checkMultiBlock))
                        {
                            if (checkMultiBlock == multiBlockController)
                            {
                                hitBlockCount++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (hitBlockCount >= blockSize)
        {

            multiBlockController.AppearActive(blockCTL);
            var block = blockCTL;
            var blockMoveOutSize = _direcType != DirecType.Row
                                ? block.BlockHeight : block.BlockWidth;

            GateOpen();

            yield return new WaitForSeconds(CONSTANTS.MOVE_TO_GATE_DURATION);
            var targetMoveOutPosition = block.MoveTransform.position + _moveTransform.forward * (blockMoveOutSize * CONSTANTS.OFFSET_BETWEEN_TILE + 1);


            block.ScaleBlock();
            CreateEffect(ColorDataSo, SetDirector());

            var moveOutDuration = block.MoveOutGate(this, targetMoveOutPosition);
            var xLocalOffset = _blockOutParticleHolder.InverseTransformPoint(block.CenterTf.position).x;
            ActionEvent();

            yield return new WaitForSeconds(moveOutDuration);
            GateClose();

            yield break;
        }
 
    }


    private void ActionEvent()
    {
        GameEventManager.RaisedEvent(GameEventManager.EventId.Effect);
        GameEventManager.RaisedEvent(GameEventManager.EventId.BirdIQ);
        GameEventManager.RaisedEvent(GameEventManager.EventId.Idea_3);
        GameEventManager.RaisedEvent(GameEventManager.EventId.Idea_4);
        GameEventManager.RaisedEvent(GameEventManager.EventId.Ice);
        GameEventManager.RaisedEvent(GameEventManager.EventId.ToggleGate);
    }

    private Vector3 SetDirector()
    {
        Vector3 director = Vector3.zero;

        if (_moveTransform.forward == Vector3.forward)
        {
        }
        else if (_moveTransform.forward == Vector3.back)
        {
            director = Vector3.up * 180;
        }
        else if (_moveTransform.forward == Vector3.right)
        {
            director = Vector3.up * 90;

        }
        else if (_moveTransform.forward == Vector3.left)
        {
            director = Vector3.up * -90;
        }



        return director;
    }

    private void CreateEffect(ColorDataSO colorDataSO, Vector3 Rota)
    {
        Vector3 Pos = transform.position;

        foreach (var colorData in _colorDatas)
        {
            if (colorData == colorDataSO)
            {
                colorData.CreateEffect(Pos, Rota);
            }
        }

    }


    /*  public void SetColorByTool(ColorDataSO colorDataSo)
      {
          _colorData = colorDataSo;
          SetNewColor(colorDataSo);
      }

      public ColorDataSO GetCurrentColorData()
      {
          return _colorData;
      }*/
}
