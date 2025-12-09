using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Lock : MonoBehaviour, IUnlockable
{

    public int requiredOpens = 3;
    private bool isUnlocked;

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshPro txtPro;

    [SerializeField] private UnityEvent completed;
    private void Start()
    {
        LockManager.Instance.Register(this);
        OnUpdateText();
    }


    public bool TryUnlock()
    {
        if (isUnlocked) return false;

        requiredOpens--;
        OnUpdateText();
        if (requiredOpens <= 0)
        {   

            OnRunAni();
            isUnlocked = true;
            completed?.Invoke();
            return true;
        }

        return false;
    }

    private void OnUpdateText()
    {
        txtPro.text = requiredOpens.ToString();

    }


    private void OnRunAni()
    {
        animator.Play("LockOpen");
    }

  


    public bool IsFullyUnlocked() => isUnlocked;

    public Transform GetTransform() => transform;

}
