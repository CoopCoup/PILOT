using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System.Linq;

public struct WeaponManagerInput
{
    // Which weapon we want equipped at any given point, including 'None' for no weapon equipped
    public bool SwapWeapon;
    public WeaponID PlayerRequestedWeapon;
    
    public bool ActionPressed;
    public bool ActionHeld;
    public bool ReadyPressed;
    public bool ReadyHeld;
}

// The player state struct is basically a snapshot of the state of the player - most of this is movement data but it could contain other stuff we need, too!
// This basically allows us to pass through the information to other classes related to the player without constantly using get methods
// Its readonly so that no other entitiy can change the fields within - this should just be a snapshot that gets read for info 
public readonly struct PlayerState
{
    public CharacterState State { get; }
    public Vector3 Acceleration { get; } 
    public Vector3 LocalVelocity { get; }
    public bool EngineOn { get; }


    // This constructor method is how the struct will get formed, by using this we can keep the struct readonly
    public PlayerState
        (
        CharacterState state,
        Vector3 acceleration,
        Vector3 localVelocity,
        bool engineOn
        )
    {
        State = state;
        Acceleration = acceleration;
        LocalVelocity = localVelocity;
        EngineOn = engineOn;
    }
}

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacterController playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Interactor interactor;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private CameraLean cameraLean;
    [Space] 
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private WeaponSway weaponSway;

    private PlayerInputActions _inputActions;

    private PlayerState _playerState;

    private WeaponEffects _weaponEffects;
    private WeaponID _requestedWeapon = WeaponID.NoChange;
    private bool _requestLastWeapon = false;

    private PlayerState BuildPlayerState()
    {
        return new PlayerState
            (
                playerCharacter._currentState,
                playerCharacter.GetCharacterAcceleration(),
                playerCharacter.GetCharacterLocalVelocity(),
                _weaponEffects.EngineOn
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

        weaponManager.Initialise();
        weaponSway.Initialise();

        playerCharacter.OnImpact += cameraShake.AddShake;
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
        weaponSway.UpdateSway(deltaTime, _playerState, playerCamera.AngularVelocity);

        // Update the interactor, and try and interact if the player presses the interact key 
        interactor.UpdateInteractor(playerCamera.transform);
        if (input.Interact.WasPressedThisFrame())
        {
            interactor.Interact(this);
        }
        

        // Set unequip requests here before bundling up the weapon inputs to pass to the weapon manager
        if (_playerState.State == CharacterState.Bouncing) // --------------------------------------------------------------ORRRRR anything else that means we wanna unequip our weapon.
        {
            _requestedWeapon = WeaponID.None;
            _requestLastWeapon = true;
        }
        else
        {
            if (_requestLastWeapon)
            {
                _requestedWeapon = WeaponID.LastWeapon;
                _requestLastWeapon = false;
            }
            else _requestedWeapon = WeaponID.NoChange;

        }

        // Get weapon inputs and update the Weapon Manager
        var weaponInput = new WeaponManagerInput
        {
            SwapWeapon = input.SwapWeapon.WasPressedThisFrame(),
            PlayerRequestedWeapon = _requestedWeapon,

            ActionHeld = input.Action.IsPressed(),
            ActionPressed = input.Action.WasPressedThisFrame(),
            ReadyHeld = input.Ready.IsPressed(),
            ReadyPressed = input.Ready.WasPressedThisFrame(),
        }; 
        

        // Update weapon manager and store currently active weapon effects
        weaponManager.UpdateWeapons(deltaTime, weaponInput);
        _weaponEffects = weaponManager.GetCurrentEffects();

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
            Zoom = _playerState.EngineOn, 
            ZoomForce = _weaponEffects.EngineForce, 
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
        cameraSpring.UpdateSpring(deltaTime, _playerState, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, _playerState, playerCamera.LookStick);
        cameraShake.UpdateShake(deltaTime);

    }

    public void PickupWeapon(WeaponID weaponID)
    {
        weaponManager.PickupWeapon(weaponID);
    }
}
