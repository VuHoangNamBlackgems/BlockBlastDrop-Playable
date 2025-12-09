using System;
using System.Collections;
using System.Linq;
using BlackGemsGlobal.SeatAway.GamePlayEvent;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    private const float SCALE_ON_TOP_MULTIPLIER = 0.7f;

    [SerializeField] private Transform _colliderHolder;
    [SerializeField] private Transform _centerOfSingleBlock;
    public Transform CenterOfSingleBlock => _centerOfSingleBlock;
    [SerializeField] private Transform _centerTf;
    public Transform CenterTf => _centerTf;
    [SerializeField] public MeshRenderer _colorRenderer;
    [SerializeField] private Transform _moveTransform;

    public Transform MoveTransform => _moveTransform;
    [SerializeField] private ColorDataSO _colorDataSo;

    [SerializeField] private ColorDataSO _colorDataSoIce;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] TextMeshPro _textBlock;
    [SerializeField] int _countIce;


    public ColorDataSO ColorDataSo { get => _colorDataSo; set => _colorDataSo = value; }
    [SerializeField] private int _blockWidth;
    public int BlockWidth { get { return _blockWidth; } }
    [SerializeField] private int _blockHeight;
    public int BlockHeight { get { return _blockHeight; } }

    [SerializeField] private Transform _itemHolder;
    [SerializeField] private Outline _outline;
    [SerializeField] private MeshRenderer[] _screwRenderes;

    private MultiBlockController _multiBlockController;
    private GameObject _colorFBX;
    public TColor _tColor;

    public TColor TColor => _tColor;
    private bool _isHideColor;
    public bool IsHideColor => _isHideColor;


    private bool _IceBlock;
    public bool IsIceBlock => _IceBlock;
    private void Awake()
    {
        GameEventManager.RegisterEvent(GameEventManager.EventId.Ice, DestroyIce);
    }
    private void OnDestroy()
    {

        GameEventManager.UnRegisterEvent(GameEventManager.EventId.Ice, DestroyIce);
    }


    public void ChangeSO(ColorDataSO color)
    {
        _colorDataSo = color;
        _colorDataSo.SetBlockColor(this);
        _colorDataSo.SetScreenColor(this);
        _colorDataSo.SetScrewnBlockColor(this);
    }

    private void DestroyIce()
    {
        if (_countIce <= 0)
            return;


        _countIce--;
        SetTxtIce();
        PlayVFXIce();
        if (_countIce == 0)
        {
            NormalBlock();
        }

    }
    private void PlayVFXIce()
    {
        _particleSystem.Play();

    }

    private void NormalBlock()
    {

        SetIce(false);

        _colorDataSo.SetBlockColor(this);
        _colorDataSo.SetScreenColor(this);
        _colorDataSo.SetScrewnBlockColor(this);
    }


    private void OnEnable()
    {
        Init();
        Setup();
    }

    public void AppCollider()
    {
        _colliderHolder.gameObject.SetActive(false);
    }

    public void PlayAnimation()
    {
        _colorFBX.transform.DOScale(Vector3.one * 1.1f, 0.2f).SetLoops(-1, LoopType.Yoyo);
    }

    public void StopAnimation()
    {
        DOTween.Kill(_colorFBX.transform);
        _colorFBX.transform.DOScale(Vector3.one, 0.2f);
    }

    private void Init()
    {
        _colorFBX = _colorRenderer.gameObject;
        _isHideColor = false;
        ChangeRenderLayerToDefault();
    }

    private void ChangeRenderLayerToDefault()
    {
        _colorRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void ChangeRenderLayerToMask()
    {
        _colorRenderer.gameObject.layer = LayerMask.NameToLayer("mask");
    }


    private void Setup()
    {
        _multiBlockController = GetComponentInParent<MultiBlockController>();
        if (Mathf.Abs(_moveTransform.forward.z) < 0.5f)
        {
            (_blockWidth, _blockHeight) = (_blockHeight, _blockWidth);
        }
        if (_multiBlockController != null)
        {

            _multiBlockController.AddBlock(this);
        }
        SetNewColor();

    }

    public void SetYOffset(float offsetY)
    {
        var localPosition = _moveTransform.localPosition;
        localPosition.y += offsetY;
        _moveTransform.localPosition = localPosition;
        if (offsetY > 0.1f)
        {
            _moveTransform.localScale = Vector3.one * SCALE_ON_TOP_MULTIPLIER;
            var colliders = _colliderHolder.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.isTrigger = true;
            }

        }
    }

    public void SetUpStart()
    {
        var duration = 0.4f;

        Sequence se = DOTween.Sequence();
        se.AppendInterval(0.3f);
        se.Append(_moveTransform.DOLocalMoveY(0, duration));
        se.Join(_moveTransform.DOScale(Vector3.one, duration));

        se.OnComplete(() =>
        {
            var nearestTile = TileMapController.Ins.GetNearestTile(CenterOfSingleBlock.position);

            _multiBlockController.MoveToTile(nearestTile, 0.2f);

            var colliders = _colliderHolder.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.isTrigger = false;
            }
        });
    }



    public void SetColliderOnPickUp()
    {
        _colliderHolder.localScale = Vector3.one * 0.9f;
    }

    public void SetColliderOnDrop()
    {
        _colliderHolder.localScale = Vector3.one;
    }


    public float MoveOutGate(GateController gate, Vector3 targetPosition)
    {
        // EmojiManager.Ins.RemoveEmoji(_multiBlockController.Emoji());

        ChangeRenderLayerToMask();
        UnSetParent();
        PlayVFX(SoundType.BlockMove);

        var tw = PositionAtSpeed(_moveTransform, targetPosition, CONSTANTS.MOVE_OUT_GATE_SPEED);

        var duration = Vector3.Distance(targetPosition, _moveTransform.position) / CONSTANTS.MOVE_OUT_GATE_SPEED;
        duration = tw.Duration();
        StartCoroutine(IeMoveOutGate(duration));
        return duration;
    }
    public Tween ScaleBlock()
    {

        return _moveTransform.DOScale(Vector3.up * 0.5f, 0.5f);
    }

    private void PlayVFX(SoundType sound)
    {
        SoundManager.Ins.PlayVFX(sound);
    }


    public Tween PositionAtSpeed(Transform target, Vector3 endValue, float averageSpeed,
    Ease ease = Ease.Linear, System.Action A = null)
    {
        float duration = Vector3.Distance(target.position, endValue) / averageSpeed;

        Tween tween = target.DOMove(endValue, 0.5f)
            .SetEase(ease).OnComplete(() =>
            {
                A?.Invoke();
            });

        return tween;
    }

    public void UnSetParent()
    {
        _moveTransform.SetParent(null);
    }


    private IEnumerator IeMoveOutGate(float waitDuration)
    {
        if (_multiBlockController != null)
        {
            _multiBlockController.RemoveBlock(this, waitDuration);
        }
        yield return new WaitForSeconds(waitDuration);
        yield return new WaitForSeconds(0.2f);
        DestroySelf();

    }


    public void SetNewColor()
    {
        if (_colorDataSoIce == null)
        {
            _colorDataSo.SetBlockColor(this);
            _colorDataSo.SetScreenColor(this);
            _colorDataSo.SetScrewnBlockColor(this);
        }
        else
        {
            SetTxtIce();
            SetIce(true);
            SetRendernerIce();
            _colorDataSoIce.SetBlockColor(this);
            _colorDataSoIce.SetScreenColor(this);
            _colorDataSoIce.SetScrewnBlockColor(this);
        }
        _tColor = _colorDataSo.GetColorType();

    }
    private void SetIce(bool isStart)
    {
        _IceBlock = isStart;
    }
    private void SetTxtIce()
    {
        _textBlock.text = _countIce.ToString();
        if (_countIce == 0)
            _textBlock.gameObject.SetActive(false);
    }
    private void SetRendernerIce()
    {
        var shape = _particleSystem.shape;
        Mesh mesh = _colorRenderer.GetComponent<MeshFilter>()?.sharedMesh;

        shape.mesh = mesh;
    }

    public void SetMaterialBlock(Material material)
    {
        _colorRenderer.sharedMaterial = material;
    }
    public void SetMaterialScreen(int index, Material material)
    {
        foreach (var item in _screwRenderes)
        {
            var mats = item.sharedMaterials;

            if (mats.Length > 0)
            {
                mats[index] = material;
                item.sharedMaterials = mats;
            }
        }
    }

    public void SetHideColor(bool b)
    {
        _isHideColor = b;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetColorByTool(ColorDataSO colorDataSo)
    {
        _colorDataSo = colorDataSo;
    }

    public ColorDataSO GetCurrentColorData()
    {
        return _colorDataSo;
    }

    public void SetScrewsMat(Material screwMaterial, Material screwInsideMaterial)
    {
        var sharedRenders = new[] { screwMaterial, screwInsideMaterial };
        foreach (var renderer in _screwRenderes)
        {
            renderer.sharedMaterials = sharedRenders;
        }
    }
}

