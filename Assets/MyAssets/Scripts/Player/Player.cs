using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System.Linq;

// The player state struct is basically a snapshot of the state of the player - most of this is movement data but it could contain other stuff we need, too!
// This basically allows us to pass through the information to other classes related to the player without constantly using get methods
// Its readonly so that no other entitiy can change the fields within - this should just be a snapshot that gets read for info 
public readonly struct PlayerState
{
    public CharacterState State { get; }
    public Vector3 Acceleration { get; } 
    public Vector3 LocalVelocity { get; }

    public bool EngineOn { get; }

    public float EngineForce { get; }

    // This constructor method is how the struct will get formed, by using this we can keep the struct readonly
    public PlayerState
        (
        CharacterState state,
        Vector3 acceleration,
        Vector3 localVelocity,
        bool engineOn,
        float engineForce
        )
    {
        State = state;
        Acceleration = acceleration;
        LocalVelocity = localVelocity;
        EngineOn = engineOn;
        EngineForce = engineForce;
    }
}

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacterController playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private CameraLean cameraLean;
    [Space] 
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private WeaponSway weaponSway;

    private PlayerInputActions _inputActions;

    private PlayerState _playerState;

    private PlayerState BuildPlayerState()
    {
        return new PlayerState
            (
                playerCharacter._currentState,
                playerCharacter.GetCharacterAcceleration(),
                playerCharacter.GetCharacterLocalVelocity(),
                weaponHolder.GetEngineRevving(),
                weaponHolder.GetEngineForce()
            );
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialise();
        playerCamera.Initialise(playerCharacter.GetCameraTarget());
        cameraSpring.Initialise();

        weaponHolder.Initialise();
        weaponSway.Initialise();
    }

    private void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };

        playerCamera.UpdateRotation(cameraInput, _playerState.EngineOn, deltaTime);
        weaponSway.UpdateLag(deltaTime, _playerState, playerCamera.LookDelta);

        // Get action input and update the weapon holder
        var actionInput = new ActionInput
        {
            // Right Click draws the engine
            //Left click revs the engine
            Action = input.Action.WasPressedThisFrame(),
            ActionSustained = input.Action.IsPressed(),
            Ready = input.Ready.WasPressedThisFrame()
                    ? ReadyInputs.Toggle
                    : ReadyInputs.None,
        };

        if (_playerState.State == CharacterState.Bouncing)
        {
            weaponHolder.StowEngine();
        }
        else
        {

            weaponHolder.UpdateInput(actionInput);
        }

        weaponHolder.UpdateWeaponHolder(deltaTime);

        // Get character input and update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.IsPressed()
                ? CrouchInput.Crouch
                : CrouchInput.Uncrouch,
            Zoom = weaponHolder.GetEngineRevving(),
            ZoomForce = weaponHolder.GetEngineForce(),
        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

        // Build a snapshot of the player's current state
        _playerState = BuildPlayerState();

    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();

        playerCamera.UpdatePosition(cameraTarget);
        //cameraShake.UpdateShake();
        cameraSpring.UpdateSpring(deltaTime, _playerState, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, _playerState, playerCamera.LookStick);
        weaponHolder.RollEngine(cameraLean.CurrentRoll, deltaTime);

    }
}
