using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [Header ("Leaning while Walking")] 
    [ SerializeField] private float maxLean = 1.6f;
    [SerializeField] private float leanSpeed = .5f;
    [Header ("Camera Roll while Flying")]
    [ SerializeField] private float maxRoll = 60f;
    [ SerializeField] private float rollSpeed = 2f;

    private float _currentRoll;

    public float CurrentRoll => _currentRoll;

    public void UpdateLean(float deltaTime, PlayerState playerState, Vector2 lookStick)
    {

        Vector3 acceleration = playerState.Acceleration;
        Vector3 localVelocity = playerState.LocalVelocity;
        bool zooming = (playerState.State is CharacterState.Zooming);
        
        // ROLL - While the player is flying, roll the camera to simulate a plane turn, using a joystick version of their look input to see how much to roll
        float targetRoll;

        if (zooming)
        {
            targetRoll = -lookStick.x * maxRoll; 
        }
        else
        {
            targetRoll = Mathf.Clamp(-localVelocity.x, -maxLean, maxLean);
        }

        // If the player isn't zooming then their camera should roll at the usual speed used for leaning left and right 
        float newRollSpeed = zooming ? rollSpeed : leanSpeed;
        _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, newRollSpeed * deltaTime);

        // Final Rotation
        transform.localRotation = Quaternion.AngleAxis(_currentRoll, Vector3.forward);
    }
}
