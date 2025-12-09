using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTriggetController : MonoBehaviour
{
    [SerializeField] Transform rightDoor;   
    [SerializeField] Transform leftDoor;

    [SerializeField] private float openScale = 1f;
    [SerializeField] private float closedScale = 0f;
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private bool _isOpenGate;
    private Sequence sequence;
    private GateController gateCTL;
    private void Start()
    {
        gateCTL = GetComponentInParent<GateController>();
        Initialezed();
    }

    private void Initialezed()
    {

        gateCTL.IsMoveBlock = _isOpenGate;
        if (_isOpenGate)
            CloseGate();
        else
            OpenGate();
    }


    [ContextMenu("OpenGate")]
    public void OpenGate()
    {
        PlayGateAnimation(openScale);
    }
    public void CloseGate()
    {
        PlayGateAnimation(closedScale);
    }

    private void PlayGateAnimation(float targetScale)
    {
        // Dừng sequence cũ nếu đang chạy
        sequence?.Kill();

        sequence = DOTween.Sequence();

        sequence.Append(rightDoor.DOScaleX(targetScale, _duration));
        sequence.Join(leftDoor.DOScaleX(targetScale, _duration));
    }
}
