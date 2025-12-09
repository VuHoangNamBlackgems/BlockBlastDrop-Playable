using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTextAnimator : MonoBehaviour
{
    [SerializeField] private float _showDelay;
    [SerializeField] private float _intervalStart;
    [SerializeField] private float _intervalEnd;
    [SerializeField] private List<SpriteTextAnimationObject> _textObjects;


    public void ShowStart()
    {
        StartCoroutine(IEShow());
    }

    private IEnumerator IEShow()
    {
        //if (!Application.isPlaying) yield break;

        List<Coroutine> coroutines = new List<Coroutine>();
        float duration = 0f;

        for (int i = 0; i < _textObjects.Count; i++)
        {
            Coroutine coroutine = StartCoroutine(_textObjects[i].Show(_intervalStart, _intervalEnd, duration));
            coroutines.Add(coroutine);
            duration += _showDelay;
        }

        // Chờ tất cả coroutine hoàn thành
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }

        Destroy(gameObject);
    }


}
