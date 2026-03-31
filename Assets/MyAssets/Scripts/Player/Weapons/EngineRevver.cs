using System;
using UnityEngine;

public class EngineRevver : MonoBehaviour
{

    public static event Action<float> OnEngineStart;
    public static event Action OnEngineStop;
    
    [Header("Revving")]
    [SerializeField] private GameObject RipCord;
    [Tooltip("The time it takes for the engine to rev up to maximum power")]
    [SerializeField] private float RevUpSpeed = 15f;

    private Animator _animator;

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
        _animator = GetComponent<Animator>();

    }

    // Start the rev up animation, which in turn will trigger OnEngineStart once the rip cord is fully pulled 
    public void RevEngine()
    {
        _animator.SetBool("RequestedRev", true);
    }

    // Invoke OnEngineStart, start the propeller spinning animations (maybe using the revUpSpeed as a scale for the speed of the propellers accelerating to max)
    private void StartEngine()
    {
        _animator.SetBool("EngineStarted", true);
        OnEngineStart?.Invoke(RevUpSpeed);
    }

    public void StopEngine()
    {
        _animator.SetBool("EngineStarted", false);
    }

    public bool IsEngineRevving()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("EngineRev"))
        {
            return true;
        }
        else
            return false;
    }
}
