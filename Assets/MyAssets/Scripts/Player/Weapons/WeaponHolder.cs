using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public struct ActionInput
{
    public InputAction Action;
    public bool Ready;
}


public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private EngineRevver engineRevver;
    [SerializeField] private GameObject engineHolder;
    [Header("Engine Positions")]
    [SerializeField] private Vector3 engineIdlePos = new Vector3 (0f, -0.43f, -0.5f);
    [SerializeField] private Vector3 engineEquipPos = new Vector3 (0f, -0.43f, 0.6f);
    [SerializeField] private Vector3 engineIdleRotEuler = new Vector3 (93f, 0f, 0f);
    [SerializeField] private Vector3 engineEquipRotEuler = new Vector3 (-9f, 0f, 0f);
    [Space]
    [Header("Engine Actions")]
    [Tooltip("The speed that the engine moves from its stowed position to its equipped position.")]
    [SerializeField] private float engineDrawSpeed = 0.3f;
    [Tooltip("The speed that the engine moves from its equipped position to its stowed position.")]
    [SerializeField] private float engineStowSpeed = 3f;
    [SerializeField] private float engineDrawRotationSpeed = 0.3f;
    [Tooltip("The engine slows down as it arrives at its equip position. This value is how close it needs to be to the equip position to start revving.")]
    [SerializeField] private float engineRevStartThreshold = 0.002f;

    // Input variables
    private bool _requestedActionTap;
    private bool _requestedActionHold;
    private bool _requestedActionHoldCancelled = false;
    private bool _requestedReady;

    // Engine variables
    private bool _engineEquipped = false;

    public void Initialise()
    {
        engineRevver.Initialise();
        engineHolder.transform.localPosition = engineIdlePos;
        engineHolder.transform.localEulerAngles = engineIdleRotEuler;
    }

    public void UpdateInput(ActionInput input)
    {
        // If the player has held the action button past the 'held' trigger and then releases the action button
        // request to cancel their action button held input. 
        if ((!input.Action.IsPressed()) && _requestedActionHold)
            _requestedActionHoldCancelled = true;
        input.Action.performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                _requestedActionHoldCancelled = false;
                _requestedActionHold = true;
               // Debug.Log("Held!!");
            }
            else
            {
                _requestedActionTap = true;
               // Debug.Log("Tapped!!");
            }
        };

    }

    public void UpdateWeaponHolder(float deltaTime)
    {
        // If the player is holding the action button or cancelling the action hold either equip or put away the engine.
        if (_requestedActionHold || _requestedActionHoldCancelled)
        {
            // get the start and end goals for moving and rotating the engine,
            // changing the target pos and rot depending on whether the input is held or cancelled 
            var currentEnginePos = engineHolder.transform.localPosition;
            var currentEngineRot = engineHolder.transform.localRotation;
            var returning = (_requestedActionHoldCancelled && !engineRevver.IsEngineRevving()) is true ? true : false;
            var targetEnginePos = Vector3.zero;
            var targetEngineRot = (_requestedActionHoldCancelled && !_engineEquipped) is true ? Quaternion.Euler(engineIdleRotEuler) : Quaternion.Euler(engineEquipRotEuler);
            Vector3 newEnginePos;
            Quaternion newEngineRot;

            if (!returning)
            {
                targetEnginePos = engineEquipPos;
                newEnginePos = Vector3.Lerp
                (
                    a: currentEnginePos,
                    b: targetEnginePos,
                    t: 1f - Mathf.Exp(-engineDrawSpeed * deltaTime)
                );
            }
            else
            {
                Debug.Log("RETURNING");
                targetEnginePos = engineIdlePos;
                newEnginePos = Vector3.MoveTowards
                    (
                        current: currentEnginePos,
                        target: targetEnginePos,
                        maxDistanceDelta: engineStowSpeed * deltaTime
                    );
            }


            newEngineRot = Quaternion.Lerp
                (
                    a: currentEngineRot,
                    b: targetEngineRot,
                    t: 1f - Mathf.Exp(-engineDrawRotationSpeed * deltaTime)
                );

            engineHolder.transform.localPosition = newEnginePos;
            engineHolder.transform.localRotation = newEngineRot;

            var vectorDistance = Vector3.Distance(engineHolder.transform.localPosition, targetEnginePos);

            // When the engine is in the equipped position, tell the engine to start revving up
            if ((vectorDistance < engineRevStartThreshold))
            {
                if (targetEnginePos == engineEquipPos)
                {
                    _engineEquipped = true;
                    engineRevver.RevEngine();
                }
                else
                {
                    _engineEquipped = false;
                    _requestedActionHoldCancelled = false;
                }
            }
        }
    }
}
