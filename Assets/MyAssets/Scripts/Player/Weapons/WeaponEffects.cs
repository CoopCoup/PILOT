using UnityEngine;

public struct WeaponEffects
{
    // Impulses / Recoil-like Forces
    public Vector3 MovementForce {  get; }

    // Engine State
    public bool EngineOn {  get; }
    public float EngineForce { get;  } // 0-1 Throttle

    //Constructor method
    public WeaponEffects
        (
            Vector3 movementForce,
            bool engineOn,
            float engineForce
        )
    {
        MovementForce = movementForce;
        EngineOn = engineOn;
        EngineForce = engineForce;
    }


}
