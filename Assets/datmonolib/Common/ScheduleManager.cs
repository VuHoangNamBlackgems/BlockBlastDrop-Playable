using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleManager : MonoBehaviour
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RunTimeInit()
    {
        var newGameObject = new GameObject("ScheduleManager");
        newGameObject.AddComponent<ScheduleManager>();
    }
    
    
    public static ScheduleManager Ins;

    private void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void Schedule(MonoBehaviour monoBehaviour,float delay,Action task)
    {
        if (monoBehaviour != null)
        {
            if (delay < Mathf.Epsilon)
            {
                task?.Invoke();
                return;
            }
            monoBehaviour.StartCoroutine(IeSchedule(delay,task));
        }
    }

    public void ScheduleBeforeRender(MonoBehaviour monoBehaviour, Action task)
    {
        monoBehaviour.StartCoroutine(IeBeforeRender(task));
    }
    public Coroutine ScheduleRealTime(MonoBehaviour monoBehaviour, float delay, Action task)
    {
        if (monoBehaviour != null)
        {
            if (delay < Mathf.Epsilon)
            {
                task?.Invoke();
                return null; 
            }
            return monoBehaviour.StartCoroutine(IeRealTime(delay, task));
        }
        return null;
    }

    public Coroutine ScheduleRealTime(float delay, Action task)
    {
        if (delay < Mathf.Epsilon)
        {
            task?.Invoke();
            return null; 
        }
        return this.StartCoroutine(IeRealTime(delay, task));
        return null;
    }
    
    public void Schedule(float delay, Action task)
    {
        if (delay < Mathf.Epsilon)
        {
            task?.Invoke();
            return;
        }
        StartCoroutine(IeSchedule(delay, task));   
    }

    private IEnumerator IeSchedule(float delay, Action task)
    {
        if (delay > float.Epsilon)
        {
            yield return new WaitForSeconds(delay);
        }
        task?.Invoke();
    }

    private IEnumerator IeRealTime(float delay, Action task) {
        if (delay > float.Epsilon)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        task?.Invoke();
    }


    public Coroutine Schedule(Func<bool> condition,Action onComplete)
    {
        return StartCoroutine(IeWaitUntil(condition, onComplete));
    }
    public Coroutine Schedule(MonoBehaviour mono, Func<bool> condition, Action onComplete)
    {
        return mono.StartCoroutine(IeWaitUntil(condition, onComplete));
    }
    private IEnumerator IeWaitUntil(Func<bool> condition, Action onComplete)
    {
        yield return new WaitUntil(()=>condition.Invoke());
        onComplete?.Invoke();
    }


    private IEnumerator IeBeforeRender(Action task)
    {
        yield return null;
        task?.Invoke();
    }
}



