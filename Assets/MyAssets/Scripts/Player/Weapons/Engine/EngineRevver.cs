using System;
using UnityEngine;

public class EngineRevver : MonoBehaviour
{

    public static event Action OnEngineStart;
    
    [Header("Revving")]
    [SerializeField] private GameObject RipCord;

    private Animator animator;
    public float engineForce = 0f;
    public void Initialise()
    {
        animator = GetComponent<Animator>();
    }

    // Start the rev up animation, which in turn will trigger StartEngine once the rip cord is fully extended in the animation
    public void RevEngine()
    {
        animator.Play("EngineRev", 0);
    }

    // start the propeller spinning animations
    private void StartEngine()
    {
        animator.SetBool("EngineStarted", true);
    }

    public void StopEngine()
    {
        animator.SetBool("EngineStarted", false);
        engineForce = 0f;
    }
}
