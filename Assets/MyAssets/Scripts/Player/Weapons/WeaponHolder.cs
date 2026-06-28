using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public struct ActionInput
{
    public bool Action;
    public bool ActionSustained;
    public ReadyInputs Ready;
}

public enum ReadyInputs
{
    None, Toggle
}

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private EngineRevver engineRevver;
    [SerializeField] private GameObject engineHolder;
    [SerializeField] private float maxRoll = 60f;
    [SerializeField] private float rollSpeed = 2f;

    // Input variables
    private bool _requestedEngineRev;
    private bool _requestedDraw;

    private bool _engineRevving = false;

    // Engine variables
    private Animator engineHolderAnimator;
    private float _currentRoll;

    public void Initialise()
    {
        engineHolderAnimator = engineHolder.GetComponent<Animator>();
        engineRevver.Initialise();
        EngineHolder engineHolderScript = engineHolder.GetComponent<EngineHolder>();
        engineHolderScript.Initialise();
        EngineHolder.OnEngineStow += StowedEngine;
        EngineRevver.OnEngineStart += EngineStarted;
    }

    public void UpdateInput(ActionInput input)
    {
        _requestedEngineRev = input.Action;

        _requestedDraw = input.Ready switch
        {
            ReadyInputs.None => _requestedDraw,
            ReadyInputs.Toggle => !_requestedDraw,
            // If theres no ready input, keep _requestedDraw to what it already is!
            _ => _requestedDraw,
        };
    }

    public void UpdateWeaponHolder(float deltaTime)
    {
        // Tell the animator whether or not the player wants to draw the engine - if true, itll play the animation and draw it, if false it'll holster the engine 
        engineHolderAnimator.SetBool("Draw", _requestedDraw);
        if (engineHolderAnimator.GetBool("CanRev") && _requestedEngineRev)
        {
            engineRevver.RevEngine();
        }
    }

    public void RollEngine(Vector2 lookStick, float deltaTime)
    {
        var targetRoll = -lookStick.x * maxRoll;

        var newRollSpeed = _engineRevving ? rollSpeed : rollSpeed * 2;

        _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, newRollSpeed * deltaTime);
        var euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, euler.y, _currentRoll);
    }

    public void StowEngine()
    {
        _requestedDraw = false;
    }

    private void StowedEngine()
    {
        engineRevver.StopEngine();
        _engineRevving = false;
    }

    private void EngineStarted()
    {
        _engineRevving = true;
    }

    public bool GetEngineRevving()
    {
        return _engineRevving;
    }

    public float GetEngineForce()
    {
        return engineRevver.engineForce;
    }


    private void OnDestroy()
    {
        EngineHolder.OnEngineStow -= StowedEngine;
        EngineRevver.OnEngineStart -= EngineStarted;
    }
}
