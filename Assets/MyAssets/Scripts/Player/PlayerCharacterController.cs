using KinematicCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.LightAnchor;

public enum CrouchInput
{
    None, Toggle, Crouch, Uncrouch
}

public enum Stance
{
    Stand, Crouch
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}

public class PlayerCharacterController : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [Header("Stable Movement")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float crouchSpeed = 7f;
    [Tooltip("The responsiveness of grounded walking movement - basically grounded acceleration.")]
    [SerializeField] private float walkResponse = 25f;
    [Tooltip("The responsiveness of grounded crouch walking movement - basically grounded acceleration.")]
    [SerializeField] private float crouchWalkResponse = 20f;
    [Space]
    [Header("Air Movement")]
    [SerializeField] private Vector3 gravity = new Vector3(0f, -50f, 0f);
    [SerializeField] private float airMoveSpeed = 5f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [Header("Jumping")]
    [Tooltip("Allow the player to jump while in midair.")]
    [SerializeField] private bool allowAirJumps = true;
    [Tooltip("Whether or not the player can hold the jump input to jump higher.")]
    [SerializeField] private bool allowJumpSustain = true;
    [Tooltip("Whether or not the player can jump while sliding down slopes too steep to walk up.")]
    [SerializeField] private bool allowJumpsOnSlopes = false;
    [SerializeField] private float jumpSpeed = 15f;
    [SerializeField] private float airJumpSpeed = 15f;
    [Tooltip("The grace period just before landing and right after leaving the ground where the player can still jump.")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float slopeJumpSpeed = 5f;
    [Range(0f, 1f)]
    [Tooltip("Scalar determining how much jumping on a steep slope will push you away from the slope instead of straight up.")]
    [SerializeField] private float slopeJumpKickOffScale = 0.5f;
    [Range(0, 1f)]
    [Tooltip("The modifier to gravity while the jump input is held - lower value means holding down jump takes you higher!")]
    [SerializeField] private float jumpSustainGravity = 0.5f;
    [Tooltip("How many times the player can jump while midair.")]
    [SerializeField] private int airJumps = 1;
    [Space]
    [Header("Crouching and Height")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;    
    [Tooltip("How sharply the player should move from standing to crouched.")]
    [SerializeField] private float crouchHeightResponse = 15f;
    [Tooltip("The camera height while the player is standing.")]
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Tooltip("The camera height while the player is standing.")]
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;


    private Vector3 acceleration;

    private Stance _stance;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private int _airJumpCount;
    private bool _jumpedThisFrame;
    private bool _requestedCrouch;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;

    // When the player starts revving the engine, the player input is updated with its power.
    // We then use this power value to apply force to the player character
    private bool _engineRevved;
    private float _EnginePower;

    private Collider[] _uncrouchOverlapResults; 

    public void Initialise()
    {
        _stance = Stance.Stand;
        _uncrouchOverlapResults = new Collider[8];
        _airJumpCount = airJumps; 
        
        //Assign to motor
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        // Take the 2d input vector and create the 3D movement vector on the XZ plane
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp the length to 1 to prevent moving faster diagonally with WASD input
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        // Orient the input so it's relative to the direction the player is facing
        _requestedMovement = input.Rotation * _requestedMovement;

        // _requestedJump is set AFTER _wasRequestingJump, so we can check if they're different
        // to see if the player is requesting to jump this exact frame
        var _wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !_wasRequestingJump)
        {
            _timeSinceJumpRequest = 0f;
        }
        _requestedSustainedJump = input.JumpSustain;

        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Crouch => true,
            CrouchInput.Uncrouch => false,
            _ => _requestedCrouch
        };
    }


    // This is called when the motor wants to know what its rotation should be right now

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // Update the character's rotation to face in the same direction as the requested rotation (camera rotation)
        // We don't want the character to pitch up and down, so the direction the character looks should be "flattened"

        // This is done by projecting a vector, pointing in the same direction that the player is looking, onto a flat ground plane
        var forward = Vector3.ProjectOnPlane
            (
                _requestedRotation * Vector3.forward,
                // We use character up instead of world up direction to project the vector
                // in case we have funky gravity later on!
                motor.CharacterUp
            );

        currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }


    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalisedHeight = currentHeight / standHeight;

        var cameraTargetHeight = currentHeight *
            (
                _stance is Stance.Stand
                    ? standCameraTargetHeight
                    : crouchCameraTargetHeight
            );
        
        // Get the intended scale for the character's root capsule to scale it up and down when standing / crouching
        var rootTargetScale = new Vector3(1f, normalisedHeight, 1f);

        //lerp the camera and character capsule root changes in height so crouching is smooth!
        cameraTarget.localPosition = Vector3.Lerp
            (
                a: cameraTarget.localPosition,
                b: new Vector3(0f, cameraTargetHeight, 0f),
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
        root.localScale = Vector3.Lerp
            (
                a: root.localScale,
                b: rootTargetScale,
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
    }


    // This is called when the motor wants to know what its velocity should be right now
    
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        acceleration = Vector3.zero;

        // If on the ground...
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0f;
            _ungroundedDueToJump = false;
            
            // Snap the requested movement direction to the angle of the surface
            // the character is currently walking on
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            // Calculate the speed and responsiveness of movement based
            // on the character's stance
            var speed = _stance is Stance.Stand
                ? walkSpeed
                : crouchSpeed;
            var response = _stance is Stance.Stand
                ? walkResponse
                : crouchWalkResponse;
            // And smoothly move along the ground in that direction
            var targetVelocity = groundedMovement * speed;
            var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );

            acceleration = (moveVelocity - currentVelocity) / deltaTime;
            currentVelocity = moveVelocity;
        }
        // Else, in the air...
        else
        {
            _timeSinceUngrounded += deltaTime;
            
            // Air Movement
            var targetPlanarVelocity = Vector3.zero;
            if (_requestedMovement.sqrMagnitude > 0)
            {
                // Requested movement projected onto a plane made with the direction of movement
                // and the character's up vector (as we have no ground to get a normal tanmgent from)
                var planarMovement = Vector3.ProjectOnPlane
                    (
                        vector: _requestedMovement,
                        planeNormal: motor.CharacterUp
                    ) *_requestedMovement.magnitude;

                // Current velocity on movement plane
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                    (
                        vector: currentVelocity,
                        planeNormal: motor.CharacterUp
                    );

                // Calculate movement force
                var movementForce = planarMovement * airAcceleration * deltaTime;

                // If moving slower than the max air speed, treat movementForce as a simple steering force
                if (currentPlanarVelocity.magnitude < airMoveSpeed)
                {
                    // Add it to the current planar velocity for a target velocity
                    targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    // Limit target velocity to air speed
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airMoveSpeed);

                    // Steer towards target velocity
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                // Otherwise, nerf the movement force when it is in the direction of the current planar velocity 
                // to prevent accelerating even further beyond the max air speed
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    //Project movement force onto the plane whose normal is the current planar velocity
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                        (
                            vector: movementForce,
                            planeNormal: currentPlanarVelocity.normalized
                        );

                    movementForce = constrainedMovementForce;
                }

                    // Steer towards current velocity
                    currentVelocity += movementForce;

                // Prevent air-climbing steep slopes
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
                    targetPlanarVelocity = Vector3.ProjectOnPlane(targetPlanarVelocity, perpenticularObstructionNormal);
                }

                currentVelocity += movementForce;

            }

            
            
            // Gravity
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            // If the player is holding the jump input, sustain their jump by reducing the effective gravity 
            // SO LONG AS they are still moving upwards and not on any unstable ground (to further prevent air-climbing steep slopes)
            if (_requestedSustainedJump && allowJumpSustain && verticalSpeed > 0f && (motor.GroundingStatus.FoundAnyGround ? motor.GroundingStatus.IsStableOnGround : true ))
            {
                effectiveGravity *= jumpSustainGravity;
            }
            currentVelocity += effectiveGravity * deltaTime;
        }

        // Jumping!
        _jumpedThisFrame = false;
        if (_requestedJump)
        {
            var grounded = (motor.GroundingStatus.FoundAnyGround);
            var canCoyoteJump = (_timeSinceUngrounded < coyoteTime) && !_ungroundedDueToJump;
            _requestedJump = false;
            Vector3 jumpDirection = motor.CharacterUp;

            if (grounded)
            {

                // If the player is sliding on a steep slope and we've allowed them to jump, make em jump with reduced jump force
                if (!motor.GroundingStatus.IsStableOnGround && allowJumpsOnSlopes)
                {
                    // Calculate the jump direction using the slopeJumpKickOffScale
                    // to determine how much the jump should push the player away from the slope instead of straight up
                    var slopeJumpDirection = Vector3.Lerp
                        (
                            a: motor.CharacterUp,
                            b: motor.GroundingStatus.GroundNormal,
                            t: slopeJumpKickOffScale
                        );
                    // Calculate the current XZ velocity of the player
                    // and the target XZ velocity of the jump kicking off the slope
                    var currentPlanarVelocity = Vector3.ProjectOnPlane
                    (
                        vector: currentVelocity,
                        planeNormal: motor.CharacterUp
                    );
                    var targetPlanarVelocity = Vector3.ProjectOnPlane
                        (
                            vector: slopeJumpDirection,
                            planeNormal: motor.CharacterUp
                        );

                    // Unstick the player from the ground and JUMP, setting XZ velocity to angle away from the slope
                    currentVelocity += targetPlanarVelocity - Vector3.Project(currentVelocity, currentPlanarVelocity);
                    motor.ForceUnground(time: 0.1f);
                    _ungroundedDueToJump = true;
                    currentVelocity += (slopeJumpDirection * slopeJumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                    _jumpedThisFrame = true;
                }
                else
                {
                    motor.ForceUnground(time: 0.1f);
                    _ungroundedDueToJump = true;
                    currentVelocity += (jumpDirection * jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                    _jumpedThisFrame = true;
                }
                   
            }
            // Else if airborne...
            else
            {
                // If still within coyote time, use a regular jump instead of one of the limited air jumps.
                if (canCoyoteJump)
                {
                    motor.ForceUnground(time: 0.1f);
                    currentVelocity += (jumpDirection * jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                    _jumpedThisFrame = true;
                }
                else 
                {
                    // Buffer airborne jump requests in case the player is within the grace period for jumping right before landing
                    _timeSinceJumpRequest += deltaTime;
                    var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                    _requestedJump = canJumpLater;

                    // AIR JUMPING - outside of coyote time, jumping while airborne requires air jumps to be enabled
                    if (allowAirJumps && (_airJumpCount < airJumps))
                    {
                        motor.ForceUnground(time: 0.1f);
                        currentVelocity += (jumpDirection * airJumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
                        _airJumpCount++;
                    }
                }
               
            }
        }
    }

    // This is called before the motor does anything
    
    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Crouch
        if (_requestedCrouch && _stance is Stance.Stand)
        {
            _stance = Stance.Crouch;
            motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
        }
    }

    // This is called after the motor has finished everything in its update

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle jump related values
        {
            if (allowJumpsOnSlopes ? motor.GroundingStatus.FoundAnyGround : motor.GroundingStatus.IsStableOnGround)
            {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame)
                {
                    _airJumpCount = 0;
                }
            }
        }
        
        // Uncrouch
        if (!_requestedCrouch && _stance is not Stance.Stand)
        {
            // tentatively "standup" the character capsule
            motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: standHeight,
                    yOffset: standHeight * 0.5f
                );

            // Then see if the capsule overlaps any colliders before actually
            // allowing the character to standup
            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                // Re-crouch 
                _requestedCrouch = true;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
            else
            {
                _stance = Stance.Stand;
            }
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        // This is called after when the motor wants to know if the collider can be collided with (or if we can just go through it)
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        // This is called when the motor's ground probing detects a ground hit
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        // This is called when the motor's movement logic detects a hit 
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        // This is called after every hit detected in the motor, to give you a chance to modify the HitStabilityReport any way you want
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc... handling
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        // This is called by the motor when it is detecting a collision that did not result from a "movement hit"
    }

   
    public Transform GetCameraTarget() => cameraTarget;

    public Vector3 GetCharacterAcceleration() => acceleration;
}
