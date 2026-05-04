using System;
using UnityEngine;

public class EngineHolder : MonoBehaviour
{
    private Animator animator;
    public static event Action OnEngineStow;
    public void Initialise()
    {
        animator = GetComponent<Animator>();
    }

    private void EnableRevving()
    {
        animator.SetBool("CanRev", true);
    }

    private void StopEngine()
    {
        OnEngineStow?.Invoke();
    }
}
