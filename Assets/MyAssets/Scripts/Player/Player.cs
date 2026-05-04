using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System.Linq;

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacterController playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [Space] 
    [SerializeField] private WeaponHolder weaponHolder;

    private PlayerInputActions _inputActions;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialise();
        playerCamera.Initialise(playerCharacter.GetCameraTarget());

        cameraSpring.Initialise();
        cameraLean.Initialise();

        weaponHolder.Initialise();

    }

    private void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        

        // Get action input and update the weapon holder
        var actionInput = new ActionInput
        {
            // Right Click draws the engine
            //Left click revs the engine
            Action = input.Action.WasPressedThisFrame(),
            ActionSustained = input.Action.IsPressed(),
            Ready = input.Ready.IsPressed()
                ? ReadyInputs.Ready
                : ReadyInputs.Unready,
        };
        weaponHolder.UpdateInput(actionInput);
        weaponHolder.UpdateWeaponHolder(deltaTime);
        var zoomInput = weaponHolder.GetEngineRevving();

        // Get camera input and update its rotation
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        var moveInput = input.Move.ReadValue<Vector2>();
        playerCamera.UpdateRotation(cameraInput, moveInput, zoomInput, deltaTime);

        // Get character input and update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = moveInput,
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.IsPressed()
                ? CrouchInput.Crouch
                : CrouchInput.Uncrouch,
            Zoom = zoomInput,
            ZoomForce = weaponHolder.GetEngineForce(),
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var acceleration = playerCharacter.GetCharacterAcceleration();

        playerCamera.UpdatePosition(cameraTarget);   
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, acceleration, cameraTarget.up);
    }
}
