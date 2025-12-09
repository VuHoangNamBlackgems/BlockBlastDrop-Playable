using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGate : MonoBehaviour
{
    private GateController _gateController;
    [SerializeField] private Transform _leftTf, _rightTf;
  //  public Animator animator;


    private void Start()
    {
        //animator = GetComponentInChildren<Animator>();
        _gateController = GetComponentInParent<GateController>();
        InitSize();
    }

    /*    public void PlayGateAnimation(int value)
        {
            animator.Play("Gate_" + value);
        }*/


    public void InitSize()
    {
        if (_gateController == null)
            return;

        int tileCount = _gateController.TileCount;
        _leftTf.localPosition = new Vector3(-tileCount, 0);
        _rightTf.localPosition = new Vector3(tileCount, 0);
    }
}
