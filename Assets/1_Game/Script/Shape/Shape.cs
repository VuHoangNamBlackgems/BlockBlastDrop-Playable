using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Design Mode")]
    public float levelScale = 1;
    public ColorId color;
    public int numberCubePickup;

    [Header("Shape Data & Visual")]
    [SerializeField] private ShapeData CurrentShapeData;
    [SerializeField] private GameObject squareShape;
    [SerializeField] private float squareGap = 0.836f;
    [SerializeField] private float squareScale = 0.98f;
    [SerializeField] private float shapeSelectedPitch = 0.5f;
    [SerializeField] private FontController textNumberCubePickup;
    [SerializeField] private Transform Direction;
    [SerializeField] private ParticleSystem magicWandEffect;

    [Header("Movement Limits")]
    [SerializeField] public LimitDirection limitDirection = LimitDirection.None;

    [Header("Ease Snap")]
    [SerializeField] private Ease easeSnap = Ease.OutQuad;

    [Header("Swipe / Flick")]
    [SerializeField] private float swipeVelocityThreshold = 50f; // pixels/sec
    [SerializeField] private float swipeMaxTime = 0.5f; // seconds

    [HideInInspector]
    public Grid grid;

    private readonly List<GameObject> _currentShape = new List<GameObject>();
    private Transform visualShape;
    private Vector3 originVisualShape;
    private Vector3 originPosTextNumber;

    // Drag
    private Camera mainCam;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Plane dragPlane;
    private Vector3 targetPos;

    // Input tracking for swipe
    private Vector2 inputStartPos;
    private float inputStartTime;
    // snapped world position at input start (to ensure swipe moves exactly one grid cell from origin)
    private Vector3 inputStartSnapped;

    // Snap & offsets
    private float offsetX = 0;
    private float offsetZ = 0;

    // Cached colliders for collision checking while dragging
    private readonly List<Collider> _squareColliders = new List<Collider>();
    private readonly List<Vector3> _localCenterOffsets = new List<Vector3>();
    private readonly List<Vector3> _halfExtents = new List<Vector3>();
    private readonly List<Quaternion> _colliderRotations = new List<Quaternion>();

    // reusable buffer to avoid allocations during overlap checks
    private Collider[] _overlapBuffer = new Collider[64];

    private float CurrentSquareSize => grid ? (grid.squareGap + grid.squareScale) : 1f;

    BoardController boardController => BoardController.Instance;

    private void Awake()
    {
        mainCam = Camera.main;
        grid = transform.parent.parent.GetComponentInChildren<Grid>();
        dragPlane = new Plane(Vector3.up, transform.position);
        visualShape = transform.GetChild(2);
        originVisualShape = visualShape.localPosition;
        originPosTextNumber = textNumberCubePickup ? textNumberCubePickup.transform.localPosition : Vector3.zero;
    }

    private void Start()
    {
        InitialSetup();
        CreateShape();
        TrySnapToGrid();
    }

    private void InitialSetup()
    {
        UpdateNumberCubePickup();
        UpdateVisualDirection();
        gameObject.tag = color.ToString();
        if (visualShape && visualShape.childCount > 0)
        {
            var mr = visualShape.GetChild(0).GetComponent<MeshRenderer>();
            if (mr) mr.material = GameConfig.Instance.GetColorHole((int)color);
        }
    }

    #region Shape Generation
    public void CreateShape()
    {
        int totalSquareNumber = GetNumberOfSquare(CurrentShapeData);

        while (_currentShape.Count < totalSquareNumber)
        {
            var shapeSquare = Instantiate(squareShape, transform);
            _currentShape.Add(shapeSquare);
            var sq = shapeSquare.GetComponent<ShapeSquare>();
            if (sq) sq.Init(this);
        }

        foreach (var square in _currentShape)
        {
            square.transform.localPosition = Vector3.zero;
            square.transform.localScale = new Vector3(squareScale * levelScale, 1, squareScale * levelScale);
            square.SetActive(false);
        }

        var squareRect = squareShape.GetComponent<Transform>();
        var moveDistance = new Vector3(
            (squareScale + squareGap) * levelScale,
            squareRect.localScale.y + squareGap * levelScale,
            (squareScale + squareGap) * levelScale
        );

        int currentIndexInList = 0;
        for (int row = 0; row < CurrentShapeData.rows; row++)
        {
            for (int column = 0; column < CurrentShapeData.columns; column++)
            {
                if (CurrentShapeData.board[row].column[column])
                {
                    var sq = _currentShape[currentIndexInList];
                    sq.SetActive(true);
                    sq.transform.localPosition = new Vector3(
                        GetXPositionForShapeSquare(CurrentShapeData, column, moveDistance),
                        0.28f,
                        GetZPositionForShapeSquare(CurrentShapeData, row, moveDistance)
                    );
                    currentIndexInList++;
                }
            }
        }

        // Build collider cache for collision checks while dragging
        BuildColliderCache();
    }

    private void BuildColliderCache()
    {
        _squareColliders.Clear();
        _localCenterOffsets.Clear();
        _halfExtents.Clear();
        _colliderRotations.Clear();

        foreach (var sq in _currentShape)
        {
            if (!sq.activeSelf) continue;
            var col = sq.GetComponent<Collider>();
            if (col == null) continue;

            // compute local center offset relative to shape transform
            // childLocalPos is localPosition of square
            var child = sq.transform;
            Vector3 centerLocal = Vector3.zero;
            Vector3 halfExt = Vector3.one * 0.5f;
            if (col is BoxCollider box)
            {
                centerLocal = box.center;
                halfExt = box.size * 0.5f;
            }
            else if (col is MeshCollider mesh)
            {
                var mf = sq.GetComponent<MeshFilter>();
                if (mf && mf.sharedMesh != null)
                    centerLocal = mf.sharedMesh.bounds.center;
                else
                    centerLocal = col.bounds.center - child.position; // fallback
                halfExt = col.bounds.extents;
            }

            // localCenterOffset = childLocalPos + childLocalRotation * centerLocal
            Vector3 localCenterOffset = child.localPosition + (child.localRotation * centerLocal);
            Vector3 worldHalfExt = Vector3.Scale(halfExt, child.lossyScale);

            _squareColliders.Add(col);
            _localCenterOffsets.Add(localCenterOffset);
            _halfExtents.Add(worldHalfExt);
            _colliderRotations.Add(child.localRotation);
        }
    }

    private int GetNumberOfSquare(ShapeData shapeData)
    {
        int number = 0;
        foreach (var rowData in shapeData.board)
            foreach (var active in rowData.column)
                if (active) number++;
        return number;
    }

    private float GetZPositionForShapeSquare(ShapeData shapeData, int row, Vector3 moveDistance)
    {
        float shiftOnZ = 0;
        if (shapeData.rows > 1)
        {
            if (shapeData.rows % 2 != 0)
            {
                var middleSquareIndex = (shapeData.rows - 1) / 2;
                var multiplier = (shapeData.rows - 1) / 2;
                if (row < middleSquareIndex) { shiftOnZ = moveDistance.z * 1; shiftOnZ *= multiplier; }
                else if (row > middleSquareIndex) { shiftOnZ = moveDistance.z * -1; shiftOnZ *= multiplier; }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.rows == 2) ? 1 : (shapeData.rows / 2);
                var middleSquareIndex1 = (shapeData.rows == 2) ? 0 : (shapeData.rows - 2);
                var multiplier = shapeData.rows / 2;

                if (row == middleSquareIndex1 || row == middleSquareIndex2)
                {
                    if (row == middleSquareIndex2) shiftOnZ = (moveDistance.z / 2) * -1;
                    if (row == middleSquareIndex1) shiftOnZ = moveDistance.z / 2;
                }
                if (row < middleSquareIndex1 && row < middleSquareIndex2) { shiftOnZ = moveDistance.z * 1; shiftOnZ *= multiplier; }
                else if (row > middleSquareIndex1 && row > middleSquareIndex2) { shiftOnZ = moveDistance.z * -1; shiftOnZ *= multiplier; }
            }
        }
        return shiftOnZ;
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector3 moveDistance)
    {
        float shiftOnX = 0;
        if (shapeData.columns > 1)
        {
            if (shapeData.columns % 2 != 0)
            {
                var middleSquareIndex = (shapeData.columns - 1) / 2;
                var multiplier = (shapeData.columns - 1) / 2;
                if (column < middleSquareIndex) { shiftOnX = moveDistance.x * -1; shiftOnX *= multiplier; }
                else if (column > middleSquareIndex) { shiftOnX = moveDistance.x * 1; shiftOnX *= multiplier; }
            }
            else
            {
                var middleSquareIndex2 = (shapeData.columns == 2) ? 1 : (shapeData.columns / 2);
                var middleSquareIndex1 = (shapeData.columns == 2) ? 0 : (shapeData.columns - 1);
                var multiplier = shapeData.columns / 2;

                if (column == middleSquareIndex1 || column == middleSquareIndex2)
                {
                    if (column == middleSquareIndex2) shiftOnX = moveDistance.x / 2;
                    if (column == middleSquareIndex1) shiftOnX = (moveDistance.x / 2) * -1;
                }
                if (column < middleSquareIndex1 && column < middleSquareIndex2) { shiftOnX = moveDistance.x * -1; shiftOnX *= multiplier; }
                else if (column > middleSquareIndex1 && column > middleSquareIndex2) { shiftOnX = moveDistance.x * 1; shiftOnX *= multiplier; }
            }
        }
        return shiftOnX;
    }
    #endregion
    private void Update()
    {
        if (visualShape)
        {
            visualShape.localPosition = isDragging ? originVisualShape + new Vector3(0, shapeSelectedPitch, 0) : originVisualShape;
            visualShape.GetChild(1).gameObject.SetActive(isDragging);
        }


        if (textNumberCubePickup)
            textNumberCubePickup.transform.localPosition =
                isDragging ? (originPosTextNumber + new Vector3(0, shapeSelectedPitch, 0)) : originPosTextNumber;

#if UNITY_EDITOR
        HandleMouseDrag();
#else
        HandleTouchDrag();
#endif
    }

    #region Drag
    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.IsChildOf(transform))
            {
              //  Vibration.Vibrate(5);
                AudioController.Instance.SelectShapeSound();
                boardController.startCountTime = false;
                isDragging = true;
                dragOffset = transform.position - hit.point;
                dragPlane = new Plane(Vector3.up, transform.position);

                // record input start for swipe
                inputStartPos = Input.mousePosition;
                inputStartTime = Time.time;
                inputStartSnapped = GetSnappedPosition(transform.position);
            }
        }

        if (isDragging)
            MoveShapeWithRay(mainCam.ScreenPointToRay(Input.mousePosition));

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
              //  Vibration.Vibrate(5);
                AudioController.Instance.DeselectShapeSound();
                // handle quick swipe (mouse)
                TryHandleSwipe(Input.mousePosition, Time.time);
                boardController.startCountTime = true;
                isDragging = false;

                // always snap after release; if swipe moved the shape this will snap it to the grid
                TrySnapToGrid();
            }
        }

    }

    private void HandleTouchDrag()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        Ray ray = mainCam.ScreenPointToRay(touch.position);

        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.IsChildOf(transform))
            {
             //   Vibration.Vibrate(5);
                AudioController.Instance.SelectShapeSound();
                boardController.startCountTime = false;
                isDragging = true;
                dragOffset = transform.position - hit.point;
                dragPlane = new Plane(Vector3.up, transform.position);

                // record input start for swipe
                inputStartPos = touch.position;
                inputStartTime = Time.time;
                inputStartSnapped = GetSnappedPosition(transform.position);
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            MoveShapeWithRay(ray);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (isDragging)
            {
              //  Vibration.Vibrate(5);
                AudioController.Instance.DeselectShapeSound();
                // handle quick swipe (touch)
                TryHandleSwipe(touch.position, Time.time);
                boardController.startCountTime = true;
                isDragging = false;
                TrySnapToGrid();
            }
        }
    }

    private void MoveShapeWithRay(Ray ray)
    {
        if (!dragPlane.Raycast(ray, out float enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        targetPos = hitPoint + dragOffset;
        targetPos.y = transform.position.y;

        OnLimitDirection();
        //CalculatorBoundary();

        // Try to move while preventing penetration. If blocked, try sliding along X or Z.
        TryMoveWithCollision(targetPos);
    }


    private void TryMoveWithCollision(Vector3 desiredWorldPos)
    {
        if (_squareColliders.Count == 0)
        {
            // nothing cached, do normal move
            transform.position = desiredWorldPos;
            return;
        }

        Vector3 originalPos = transform.position;

        // Try stepping toward desired position
        Vector3 lastValid = StepTowards(originalPos, desiredWorldPos);
        if (lastValid != originalPos)
        {
            transform.position = lastValid;
            return;
        }

        // If we couldn't move at all, try sliding along single axes using the same stepping logic
        Vector3 xOnly = new Vector3(desiredWorldPos.x, originalPos.y, originalPos.z);
        Vector3 zOnly = new Vector3(originalPos.x, originalPos.y, desiredWorldPos.z);

        Vector3 lastValidX = StepTowards(originalPos, xOnly);
        if (lastValidX != originalPos)
        {
            // moved along X; now try to advance Z as far as possible
            transform.position = lastValidX;
            Vector3 tryZTarget = new Vector3(transform.position.x, transform.position.y, desiredWorldPos.z);
            Vector3 lastAfterZ = StepTowards(transform.position, tryZTarget);
            transform.position = lastAfterZ;
            return;
        }

        Vector3 lastValidZ = StepTowards(originalPos, zOnly);
        if (lastValidZ != originalPos)
        {
            // moved along Z; now try to advance X as far as possible
            transform.position = lastValidZ;
            Vector3 tryXTarget = new Vector3(desiredWorldPos.x, transform.position.y, transform.position.z);
            Vector3 lastAfterX = StepTowards(transform.position, tryXTarget);
            transform.position = lastAfterX;
            return;
        }

        // otherwise stay where we are
        transform.position = originalPos;
    }

    private Vector3 StepTowards(Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        if (dist <= 0.0001f) return to;

        float baseStep = (grid != null) ? CurrentSquareSize : 1f;
        float stepSize = Mathf.Max(0.02f, Mathf.Min(baseStep * 0.25f, 0.05f));
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / stepSize));

        Vector3 lastValid = from;
        for (int s = 1; s <= steps; s++)
        {
            Vector3 candidate = Vector3.Lerp(from, to, s / (float)steps);
            if (CanPlaceAt(candidate))
            {
                lastValid = candidate;
            }
            else
            {
                break;
            }
        }

        return lastValid;
    }

    private bool CanPlaceAt(Vector3 candidateWorldPos)
    {
        // For each child collider, compute its world center and do an overlap test
        Transform shapeT = transform;
        Vector3 shapeDelta = candidateWorldPos - shapeT.position;

        float dist = shapeDelta.magnitude;
        float baseStep = (grid != null) ? CurrentSquareSize : 0.5f;
        float stepSize = Mathf.Max(0.01f, baseStep * 0.25f);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / stepSize));

        for (int s = 0; s <= steps; s++)
        {
            float t = (steps == 0) ? 1f : (s / (float)steps);
            Vector3 deltaStep = shapeDelta * t;

            for (int i = 0; i < _squareColliders.Count; i++)
            {
                Vector3 worldCenter = shapeT.position + (shapeT.rotation * _localCenterOffsets[i]) + deltaStep;
                Vector3 halfExt = _halfExtents[i] * 0.99f;
                Quaternion rot = shapeT.rotation * _colliderRotations[i];

                int hitCount = Physics.OverlapBoxNonAlloc(worldCenter, halfExt / levelScale, _overlapBuffer, rot);
                for (int h = 0; h < hitCount; h++)
                {
                    var col = _overlapBuffer[h];
                    if (col == null) continue;
                    if (col.transform.IsChildOf(shapeT)) continue; // ignore self
                    if (col.gameObject.layer == LayerMask.NameToLayer("Grid")) continue; // ignore grid
                    if (col.gameObject.layer == LayerMask.NameToLayer("Cube") && col.gameObject.tag == color.ToString()) continue; // same-color cubes allowed
                    // If we hit anything else at any sample, placement invalid
                    return false;
                }
            }
        }

        return true;
    }

    private Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        if (grid == null) return worldPos;

        float squareSize = CurrentSquareSize;
        CalculatorOffset(squareSize);

        Vector3 localPos = worldPos - grid.transform.position;
        float snapX = Mathf.Round((localPos.x - offsetX) / squareSize) * squareSize + offsetX;
        float snapZ = Mathf.Round((localPos.z - offsetZ) / squareSize) * squareSize + offsetZ;

        return new Vector3(
            snapX + grid.transform.position.x,
            transform.position.y,
            snapZ + grid.transform.position.z
        );
    }

    public void TrySnapToGrid()
    {
        Vector3 snapped = GetSnappedPosition(transform.position);
        transform.DOMove(snapped, 0.05f).SetEase(easeSnap);
    }

    private void OnLimitDirection()
    {
        targetPos.z = limitDirection == LimitDirection.Horizontal ? transform.position.z : targetPos.z;
        targetPos.x = limitDirection == LimitDirection.Vertical ? transform.position.x : targetPos.x;
    }

    private void CalculatorOffset(float squareSize)
    {
        if (grid.numberRows % 2 != 0 && grid.numberColumns % 2 != 0)
        {
            if (CurrentShapeData.rows % 2 == 0) offsetZ = squareSize * 0.5f;
            else offsetZ = 0;

            if (CurrentShapeData.columns % 2 == 0) offsetX = squareSize * 0.5f;
            else offsetX = 0;
        }
        else if (grid.numberRows % 2 == 0 && grid.numberColumns % 2 != 0)
        {
            if (CurrentShapeData.rows % 2 == 0) offsetZ = 0;
            else offsetZ = squareSize * 0.5f;

            if (CurrentShapeData.columns % 2 == 0) offsetX = squareSize * 0.5f;
            else offsetX = 0;
        }
        else if (grid.numberRows % 2 != 0 && grid.numberColumns % 2 == 0)
        {
            if (CurrentShapeData.rows % 2 == 0) offsetZ = squareSize * 0.5f;
            else offsetZ = 0;

            if (CurrentShapeData.columns % 2 == 0) offsetX = 0;
            else offsetX = squareSize * 0.5f;
        }
        else
        {
            if (CurrentShapeData.rows % 2 == 0) offsetZ = 0;
            else offsetZ = squareSize * 0.5f;

            if (CurrentShapeData.columns % 2 == 0) offsetX = 0;
            else offsetX = squareSize * 0.5f;
        }
    }

    /// <summary>
    /// Convert a screen position to a point on the current dragPlane (y plane at shape's height)
    /// </summary>
    private Vector3 ScreenToWorldOnDragPlane(Vector2 screenPos)
    {
        if (mainCam == null) return transform.position;
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (dragPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        return transform.position;
    }

    /// <summary>
    /// Try to detect a quick swipe/flick and move the shape one grid cell in that direction.
    /// Returns true if a fling was detected (not used presently but left for future use).
    /// </summary>
    private bool TryHandleSwipe(Vector2 endScreenPos, float endTime)
    {
        float dt = Mathf.Max(0.0001f, endTime - inputStartTime);
        Vector2 delta = endScreenPos - inputStartPos;
        float velocity = delta.magnitude / dt; // pixels per second

        if (dt <= swipeMaxTime && velocity >= swipeVelocityThreshold)
        {
            // compute world delta along drag plane to get correct world-axis direction
            Vector3 startWorld = ScreenToWorldOnDragPlane(inputStartPos);
            Vector3 endWorld = ScreenToWorldOnDragPlane(endScreenPos);
            Vector3 worldDelta = endWorld - startWorld;

            if (worldDelta.magnitude < 0.001f) return false;

            Vector3 dir = Vector3.zero;
            float absX = Mathf.Abs(worldDelta.x);
            float absZ = Mathf.Abs(worldDelta.z);

            // Detect diagonal when both components are significant and roughly equal
            // loosened thresholds so diagonal is easier to detect
            const float diagRatioMin = 0.4f; // allow more tolerance
            const float diagRatioMax = 2.5f;
            if (absX > 0.001f && absZ > 0.001f && absX / absZ >= diagRatioMin && absX / absZ <= diagRatioMax)
            {
                dir = new Vector3(Mathf.Sign(worldDelta.x), 0f, Mathf.Sign(worldDelta.z));
            }
            else
            {
                // pick dominant axis in world space (x or z)
                if (absX > absZ) dir = new Vector3(Mathf.Sign(worldDelta.x), 0f, 0f);
                else dir = new Vector3(0f, 0f, Mathf.Sign(worldDelta.z));
            }

            // Respect directional limits
            if (limitDirection == LimitDirection.Horizontal) dir.z = 0f;
            if (limitDirection == LimitDirection.Vertical) dir.x = 0f;

            // if direction wiped out by limits, nothing to do
            if (Mathf.Approximately(dir.x, 0f) && Mathf.Approximately(dir.z, 0f)) return false;

            float step = (grid != null) ? CurrentSquareSize : 1f;

            // Ensure movement is exactly one grid cell from the snapped start cell recorded at input begin
            Vector3 snappedStart = inputStartSnapped;
            Vector3 desired = snappedStart + dir * step;

            // set targetPos so OnLimitDirection and CalculatorBoundary can operate
            targetPos = desired;
            // apply direction limits
            OnLimitDirection();
            // clamp to grid bounds
            //CalculatorBoundary();

            // attempt to move with collision-aware step logic
            TryMoveWithCollision(targetPos);

            return true;
        }

        return false;
    }
    #endregion

    

    #region Visual
    public void UpdateNumberCubePickup()
    {
        if (textNumberCubePickup) textNumberCubePickup.ShowText(numberCubePickup);
    }

    void UpdateVisualDirection()
    {
        if (Direction)
        {
            Direction.GetChild(0).gameObject.SetActive(limitDirection == LimitDirection.Horizontal);
            Direction.GetChild(1).gameObject.SetActive(limitDirection == LimitDirection.Vertical);
        }
    }
    #endregion

    private void OnDestroy()
    {
        transform.DOKill();
    }
}

[Serializable]
public enum LimitDirection
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
}
