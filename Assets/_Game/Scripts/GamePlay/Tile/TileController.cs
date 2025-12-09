using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField]private Transform _moveTransform;
    public Transform MoveTransform{get{return _moveTransform;}}

    private void Awake()
    {
        TileMapController.Ins?.AddActiveTile(this);
    }

    private void OnDestroy()
    {
        TileMapController.Ins?.RemoveActiveTile(this);
    }

}
