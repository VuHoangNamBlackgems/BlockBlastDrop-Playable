using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField]private Transform _moveTransform;
    [SerializeField] private Transform _renderTransfrom;
    [SerializeField] private Transform _leftTf, _rightTf;
    [SerializeField] private bool _isBorderWall = true;
    public Transform MoveTransform{ get { return _moveTransform; } }
    private void Awake()
    {
    //    Setup(_moveTransform.position,_moveTransform.eulerAngles,_moveTransform.localScale);
    }
    public void Setup(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        position = RoundToIntAndHalf(position);
        rotation = RoundToIntAndHalf(rotation);
     //   _moveTransform.position = position;
        _moveTransform.rotation = Quaternion.Euler(rotation);
        _moveTransform.localScale = scale;
        if (_isBorderWall)
        {
            _renderTransfrom.localScale = new Vector3(1f/scale.x, 1f/scale.y, 1f/scale.z);
            _leftTf.localPosition=new Vector3(-1*scale.z,0f,0f);
            _rightTf.localPosition = new Vector3(1*scale.z,0f,0f);
        }
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
}
