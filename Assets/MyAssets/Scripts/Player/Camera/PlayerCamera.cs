using System;
using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float zoomSensitivity = 0.001f;
    [Space]

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 70f;
    [SerializeField] private float turnInputDamping = 1f;

    [Header("Roll")]
    [SerializeField] private float maxRoll = 70f;
    [SerializeField] private float rollAmount = 1f;
    [SerializeField] private float rollSpeed = 2f;

    private Vector3 _eulerAngles;

    // This variable behaves like a joystick with a -1 to 1 range
    private Vector2 lookStick;

    public void Initialise(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput mouseInput, bool Zooming, float deltaTime)
    {
        float yaw;
        float pitch;

        if (Zooming)
        {
            // Accumulate mouse input into virtual joystick
            Vector2 input = mouseInput.Look * zoomSensitivity;
            lookStick += input;

            // Clamp stick magnitude
            lookStick = Vector2.ClampMagnitude(lookStick, 1f);

            // Apply Damping (virtual joystick recentres over time)
            lookStick = Vector2.Lerp(lookStick, Vector2.zero, turnInputDamping * deltaTime);

            // Apply rotation from stick
            yaw = lookStick.x * turnSpeed * deltaTime;
            pitch = -lookStick.y * turnSpeed * deltaTime;

            _eulerAngles.x += pitch;
            _eulerAngles.y += yaw;

            // Clamp vertical look
            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89f, 89f);
        }
        else
        {
            // Normal mouse look
            _eulerAngles += new Vector3(-mouseInput.Look.y, mouseInput.Look.x) * sensitivity;

            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89f, 89f);

            // Reset virtual stick when exiting zoom
            lookStick = Vector2.zero;
        }

        // ---- FINAL ROTATION ----

        transform.eulerAngles = _eulerAngles;

    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }

    public Vector2 GetLookStick => lookStick;
}