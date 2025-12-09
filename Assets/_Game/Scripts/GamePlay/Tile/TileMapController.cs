using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMapController : MonoBehaviour
{
    public static TileMapController Ins { get; private set;}
    [SerializeField] private AnimationCurve _velocityBaseDistacneCurve;
    [SerializeField] private LayerMask _planeLayer;
    [SerializeField] private LayerMask _detechColliderLayer;
    public LayerMask DetechColliderLayer { get { return _detechColliderLayer; } }
    [SerializeField]private List<TileController> _activeTiles = new List<TileController>();
    public List<TileController> ActiveTiles { get { return _activeTiles; } } 
    private void Awake()
    {
        if (Ins != null)
        {
            Destroy(gameObject);
            return;
        }
        Ins = this;
    }
    public void AddActiveTile(TileController tile)
    {
        _activeTiles.Add(tile);
    }

    public void RemoveActiveTile(TileController tile)
    {
        _activeTiles.Remove(tile);
    }

    public Vector3 GetPlanePoint(Vector3 screenPosition)
    {
        var ray= Camera.main.ScreenPointToRay(screenPosition);
        var isHit = Physics.Raycast(ray, out var hit, 1000, _planeLayer);
        if (isHit)
        {
            return hit.point;
        }
        return Vector3.zero;
    }


    public List<TileController> GetColumn(Vector3 position)
    {
        var x = position.x;
        var min = _activeTiles.Min(tile =>
        {
            return Mathf.Abs(tile.MoveTransform.position.x - x);
        });
        var results=new List<TileController>();
        foreach (var tile in _activeTiles)
        {
            if (Mathf.Abs(tile.MoveTransform.position.x - x) < (0.01f + min))
            {
                results.Add(tile);
            }
        }
        return results;
    }
    
    public List<TileController> GetRow(Vector3 position)
    {
        var z = position.z;
        var min = _activeTiles.Min(tile =>
        {
            return Mathf.Abs(tile.MoveTransform.position.z - z);
        });
        var results=new List<TileController>();
        foreach (var tile in _activeTiles)
        {
            if (Mathf.Abs(tile.MoveTransform.position.x - z) < (0.01f + min))
            {
                results.Add(tile);
            }
        }
        return results;
    }
    
    public float GetVelocityBaseOnDistance(Vector3 position,Vector3 targetPosition)
    {
        position.y = 0;
        targetPosition.y = 0;
        var distance = Vector3.Distance(position, targetPosition);
        var velocity = _velocityBaseDistacneCurve.Evaluate(distance);
        if (velocity * Time.fixedDeltaTime > distance)
        {
            velocity= velocity * distance * Time.fixedDeltaTime;
        }
        return velocity;
    }


    public TileController GetNearestTile(Vector3 position)
    {
        TileController nearestTile = null;
        float minDistance = float.MaxValue;

        foreach (TileController tile in _activeTiles)
        {
            float distance = Vector3.Distance(tile.transform.position, position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = tile;
            }
        }
        return nearestTile;
    }
    public TileController GetNearestTileGate(
        Vector3 position
        ,Vector3 centerPosition
        ,GateController gateController,int blockSize)
    {
        TileController nearestTile = null;
        float minDistance = float.MaxValue;
        var gateBound = gateController.TileCount * 1f*CONSTANTS.OFFSET_BETWEEN_TILE / 2;
        
        foreach (var tile in _activeTiles)
        {
            var offset = tile.MoveTransform.position - position;
            var newCenterPosition = centerPosition + offset;
            var localPositionInGate = gateController.MoveTransform.InverseTransformPoint(newCenterPosition);
            var blockBound = Mathf.Abs(localPositionInGate.x) + blockSize * 1f * CONSTANTS.OFFSET_BETWEEN_TILE / 2f;
            var distance = offset.magnitude;
            if (blockBound - 0.2f > gateBound)
            {
                distance += 100;
            }
      
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = tile;
            }
        }
        return nearestTile;


        /*var addOffset = Vector3.zero;
        var offsetFake = centerPosition - gateController.MoveTransform.position;
        if (Mathf.Abs(gateController.MoveTransform.forward.z) < 0.5f)
        {
            addOffset.x = offsetFake.z > 0 ? -0.3f : 0.3f;
        }
        else
        {
            addOffset.x = offsetFake.x > 0 ? -0.3f : 0.3f;
        }
        position+=addOffset;
        centerPosition+=centerPosition;
        TileController nearestTile = null;
        float minDistance = float.MaxValue;
        foreach (TileController tile in _activeTiles)
        {
            float distance = Vector3.Distance(tile.transform.position, position);
            var offset = tile.transform.position - position;
            var newCenterPosition=centerPosition+offset;
            var newCenterPositionInGateLocal = gateController.MoveTransform.InverseTransformPoint(newCenterPosition);
            distance+=Mathf.Abs(newCenterPositionInGateLocal.x)/3.2f;

            var currentDistance=Vector3.Distance(gateController)

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = tile;
            }
        }
        return nearestTile;*/
    }
}
