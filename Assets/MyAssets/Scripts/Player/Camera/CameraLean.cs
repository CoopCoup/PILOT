using UnityEngine;

public class CameraLean : MonoBehaviour
{
    // This script uses the acceleration of the player character to calculate a 'lean' animation for the player character, where their view tilts in the direction of their acceleration

    [Tooltip("Used when the target acceleration of the camera lean is greater than our dampened acceleration, a value to add to the dampened acceleration until it catches up with the target.")]
    [SerializeField] private float addToDamping = 0.5f;
    [Tooltip("Used when the target acceleration of the camera lean is less than our dampened acceleration, a value to reduce the dampened acceleration until it matches the target.")]
    [SerializeField] private float decayDamping = 0.3f;
    [Tooltip("How strong the camera lean effect should be.")]
    [SerializeField] private float strength = 0.075f;

    // A smoothed version of the acceleration value passed from the player character so the camera
    // lean animations are smoother 
    private Vector3 _dampedAcceleration;
    // The damped accleration is being smoothly dampened to the input acceleration, so it needs a 'velocity'
    private Vector3 _dampedAccelerationVel;
    
    public void Initialise()
    {

    }

    public void UpdateLean(float deltaTime, Vector3 acceleration, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
            ? addToDamping 
            : decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp
            (
                current: _dampedAcceleration,
                target: planarAcceleration,
                currentVelocity: ref _dampedAccelerationVel,
                smoothTime: damping,
                maxSpeed: float.PositiveInfinity,
                deltaTime: deltaTime

            );

        // Get the roation axis for the camera based on the direction of the acceleration vector
        var leanAxis = -Vector3.Cross(_dampedAcceleration.normalized, up).normalized;

        //Reset the camera lean game object's rotation to that of its parent
        transform.localRotation = Quaternion.identity;

        // Rotate around the lean axis based on the magnitude of the character's acceleration
        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * strength, leanAxis) * transform.rotation;
    }
}
