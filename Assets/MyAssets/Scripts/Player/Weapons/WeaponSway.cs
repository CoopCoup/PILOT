using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private float maxDefaultSway = 0.06f;
    [SerializeField] private float swayDamping = 0.01f;
    [SerializeField] private float maxSwayRot = 4f;
    [SerializeField] private float swayRotDamping = 4f;
    [SerializeField] private float maxFlySway = 0.01f;
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private float swayRotSpeed = 10f;
    [SerializeField] private float lookSmoothing = 10f;

    private Vector3 _swayPos;
    private Vector3 _swayEulerRot;
    private Vector3 _smoothLook;

    public void Initialise()
    {
  
    }

    public void UpdateLag(float deltaTime, PlayerState state, Vector2 lookDelta)
    {
        if (state.State is CharacterState.Zooming)
        {
            _smoothLook = lookDelta;
        }
        else
        {
            _smoothLook = Vector3.Lerp(_smoothLook, lookDelta, deltaTime * lookSmoothing);
        }

        SwayPosition();
        SwayRotation();
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos, deltaTime * swaySpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot), deltaTime * swayRotSpeed);
    }

    private void SwayPosition()
    {
        Vector3 invertLook = _smoothLook * -swayDamping;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxDefaultSway, maxDefaultSway);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxDefaultSway, maxDefaultSway);

        _swayPos = invertLook;
    }

    private void SwayRotation()
    {
        Vector2 invertLook = _smoothLook * -swayRotDamping;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxSwayRot, maxSwayRot);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxSwayRot, maxSwayRot);

        _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

}
