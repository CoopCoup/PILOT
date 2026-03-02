using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    // This script adds a springy smoothness to the camera as it follows the player
    
    [Tooltip("How long (in seconds) it should take for the spring's oscillation to be reduced by half")]
    [Min(0.01f)]
    [SerializeField] private float halfLife = 0.075f;
    [Space]
    [Tooltip("How quickly the spring resonates.")]
    [SerializeField] private float frequency = 18f;
    [Space]
    [Tooltip("How much the spring should offset the camera's rotation.")]
    [SerializeField] private float angularDisplacement = 2f;
    [Space]
    [Tooltip("How much the spring should affect the camera's position.")]
    [SerializeField] private float linearDisplacement = 0.05f;
    private Vector3 _springPosition;
    private Vector3 _springVelocity;


    public void Initialise()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        transform.localPosition = Vector3.zero;

        Spring(ref _springPosition, ref _springVelocity, transform.position, halfLife, frequency, deltaTime);

        var relativeSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(relativeSpringPosition, up);

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
        transform.localPosition += relativeSpringPosition * linearDisplacement;
    }

    // https://allenchou.net/2015/04/game-math-more-on-numeric-springing/
    // "frequency" determines how quickly the spring resonates
    // "halfLife" determines how long it takes for the strength of the spring's oscillation to be reduced by 50 percent
    // At a halfLife of 2, the oscillation will be dampened by half every 2 seconds
    private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}
