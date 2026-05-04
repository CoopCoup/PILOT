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
    Ready, Unready
}

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private EngineRevver engineRevver;
    [SerializeField] private GameObject engineHolder;

    // Input variables
    private bool _requestedEngineRev;
    private bool _requestedDraw;

    private bool _engineRevving = false;

    // Engine variables
    private Animator engineHolderAnimator;

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
            ReadyInputs.Ready => true,
            ReadyInputs.Unready => false,
            _ => _requestedDraw,
        };
    }

    public void UpdateWeaponHolder(float deltaTime)
    {
        // If the player is holding the action button, start drawing out the engine
        // Don't want to do this if the engine is in its revving animation, so maybe hold off on updating this while revving
        engineHolderAnimator.SetBool("Draw", _requestedDraw);
        if (engineHolderAnimator.GetBool("CanRev") && _requestedEngineRev)
        {
            engineRevver.RevEngine();
        }
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
