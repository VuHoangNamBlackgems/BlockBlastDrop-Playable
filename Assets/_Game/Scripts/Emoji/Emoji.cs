using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emoji : MonoBehaviour
{
    public float maxaMove = 1f;
    [SerializeField] private GameObject obj_Eye;
    [SerializeField] private GameObject obj_Mouth;

    private bool _isMove;

    private void Start()
    {
        EmojiManager.Ins.AddEmoji(this);
    }
    public bool IsMove
    {

        get
        {
            return _isMove;
        }
        set
        {
            _isMove = value;
        }
    }


    public void SetMove(bool move)
    {
        IsMove = move;
    }
    public void ResetPos()
    {
        eyeStartPos = Vector3.zero;

        obj_Eye.transform.DOLocalMove(Vector3.zero, 0.5f);
    }



    public Vector3 eyeStartPos;
    public float calcu;



    public void SetDirector(Vector3 target)
    {
        if (_isMove)
            return;

        if (eyeStartPos == Vector3.zero)
        {
            eyeStartPos = target;

        }
        else
        {
            calcu = (target.x - eyeStartPos.x);

            Vector3 currentPosition = transform.position;

            Vector3 direction = (target - currentPosition).normalized;

            Vector3 newPosition = (direction - currentPosition) * 0.05f;
            calcu = Mathf.Clamp(calcu, -2, 2);
            Vector3 pos = Vector3.Lerp(obj_Eye.transform.localPosition, newPosition * calcu, 0.1f);

            obj_Eye.transform.localPosition = new Vector3(pos.x, obj_Eye.transform.localPosition.y, obj_Eye.transform.localPosition.z);
        }
    }

    public void SetResetDirec()
    {
        obj_Eye.transform.localPosition = Vector3.Lerp(obj_Eye.transform.localPosition, Vector3.zero, 0.1f);
        eyeStartPos = Vector3.zero;
    }
}
