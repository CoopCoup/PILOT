using Unity.VisualScripting;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float maxHorizontalSway = 0.1f;
    [SerializeField] private float maxVerticalSway = 0.06f;
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private float swayRotSpeed = 10f;



    Vector3 _currentSway;
    public void Initialise()
    {
         //_swayPos = transform.localPosition;
        //_swayEulerRot = transform.localEulerAngles;

        _currentSway = transform.localPosition;
    }

    public void UpdateSway(float deltaTime, PlayerState state, Vector2 angular)
    {
        float maxHorSway;
        float maxVertSway;
        if (state.State is CharacterState.Zooming)
        {
            maxHorSway = maxHorizontalSway * 0.25f;
            maxVertSway = maxVerticalSway * 0.25f;
        }
        else
        {
            maxHorSway = maxHorizontalSway;
            maxVertSway = maxVerticalSway;
        }

        // Angular is the angular velocity of the player camera - the velocity of the camera this frame.
        // Angular is a vector2 where x is the rotation around the x axis - pitch - and y is the rotation around the y axis - yaw. 
        // Therefore we need the y for the horizontal movement and the x for the vertical
        Vector3 targetPos = new Vector3(-angular.y * maxHorizontalSway, angular.x * maxVerticalSway, 0f);
        targetPos.x = Mathf.Clamp(targetPos.x, -maxHorSway, maxHorSway);
        targetPos.y = Mathf.Clamp(targetPos.y, -maxVertSway, maxVertSway);
        
        _currentSway = Vector3.Lerp(_currentSway, targetPos, 1f - Mathf.Exp(-swaySpeed * deltaTime));

        transform.localPosition = _currentSway;
        

    }


}
