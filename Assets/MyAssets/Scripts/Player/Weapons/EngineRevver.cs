using System;
using UnityEngine;

public class EngineRevver : MonoBehaviour
{

    public static event Action OnEngineStart;
    
    [Header("Revving")]
    [SerializeField] private GameObject RipCord;

    private Animator animator;
    public float engineForce = 0f;

    public void MoveEngine(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }

    public void RotateEngine(Quaternion rot)
    {
        this.transform.localRotation = rot;
    }

    public void Initialise()
    {
        // Set the rip chord and the propellers to their start positions and rotations
        animator = GetComponent<Animator>();

    }

    // Start the rev up animation, which in turn will trigger StartEngine once the rip cord is fully extended in the animation
    public void RevEngine()
    {
        animator.Play("EngineRev", 0);
    }

    // Invoke OnEngineStart, start the propeller spinning animations
    private void StartEngine()
    {
        animator.SetBool("EngineStarted", true);
        OnEngineStart?.Invoke();
    }

    public void StopEngine()
    {
        animator.SetBool("EngineStarted", false);
        engineForce = 0f;
    }

    public bool IsEngineRevving()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("EngineRev"))
        {
            return true;
        }
        else
            return false;
    }
}
