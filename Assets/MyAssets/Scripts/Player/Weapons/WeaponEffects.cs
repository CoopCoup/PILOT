using UnityEngine;

public struct WeaponEffects
{
    // Impulses / Recoil-like Forces
    public Vector3 MovementForce {  get; }

    // Engine State
    public bool EngineOn {  get; }
    public float EngineForce { get;  } // 0-1 Throttle

    // FX
    public bool CameraShakeRequested { get; }
    public CameraShake.CamShake WeaponCamShake;



    //Constructor method
    public WeaponEffects
        (
            Vector3 movementForce,
            bool engineOn,
            float engineForce,
            bool cameraShakeRequested,
            CameraShake.CamShake cameraShake
        )
    {
        MovementForce = movementForce;
        EngineOn = engineOn;
        EngineForce = engineForce;
        CameraShakeRequested = cameraShakeRequested;
        WeaponCamShake = cameraShake;

    }


}
