using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAnimatorService : MonoBehaviour
{

    [SerializeField] GameObject[] _textAnimators;

    public void ShowTextAnimator( Vector3 worldPosition)
    {
        GameObject prefab = _textAnimators[UnityEngine.Random.Range(0, _textAnimators.Length)];
        SpriteTextAnimator spriteTextAnimator = Instantiate(prefab, worldPosition, prefab.transform.localRotation).GetComponent<SpriteTextAnimator>();
        spriteTextAnimator.ShowStart();
    }
}
