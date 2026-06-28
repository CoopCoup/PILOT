using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triggers an action after a certain time
public class FunctionTimer
{

    // Dummy class to have access to MonoBehaviour functions
    public class MonoBehaviourHook : MonoBehaviour
    {
        public Action onUpdate;
        private void Update()
        {
            if (onUpdate != null) onUpdate();
        }
    }


    private static List<FunctionTimer> activeTimerList; // Holds a reference to all active timers
    private static GameObject initGameObject; // Global game object used for intialising class, is destroyed on scene change 

    private static void InitIfNeeded()
    {
        if (initGameObject == null)
        {
            initGameObject = new GameObject("FunctionTimer_Global");
            activeTimerList = new List<FunctionTimer>();
        }
    }

    public static FunctionTimer Create(Action action, float timer, string timerName = null)
    {
        InitIfNeeded();
        GameObject gameObject = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));
        
        FunctionTimer funcTimer = new FunctionTimer(action, timer, timerName, gameObject);

        gameObject.GetComponent<MonoBehaviourHook>().onUpdate = funcTimer.Update;

        activeTimerList.Add(funcTimer);

        return funcTimer;
    }

    private static void RemoveTimer(FunctionTimer funcTimer)
    {
        InitIfNeeded();
        activeTimerList.Remove(funcTimer);
    }

    public static void StopTimer(string timerName)
    {
        for (int i = 0; i < activeTimerList.Count; i++)
        {
            if (activeTimerList[i].timerName == timerName)
            {
                // After going through all the active timers, stop the timer with the name we want
                activeTimerList[i].DestroySelf();
                i--;
            }
        }
    }


    private Action action;
    private float timer;
    private string timerName;   
    private GameObject gameObject;
    private bool isDestroyed;

    private FunctionTimer(Action action, float timer, string timerName, GameObject gameObject)
    {
        this.action = action;
        this.timer = timer;
        this.timerName = timerName;
        this.gameObject = gameObject;
        isDestroyed = false;
    }

    public void Update()
    {
        if (!isDestroyed)
        {
            timer -= Time.deltaTime;
            if (timer < 0 )
            {
                // Trigger the action 
                action();
                DestroySelf();
            }
        }
    }

    private void DestroySelf()
    {
        isDestroyed = true;
        UnityEngine.Object.Destroy(gameObject);
        RemoveTimer(this);
    }
}
