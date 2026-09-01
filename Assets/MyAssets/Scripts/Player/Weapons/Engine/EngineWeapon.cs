using UnityEngine;

public class EngineWeapon : WeaponBase
{
    public override WeaponID ID => WeaponID.Engine;
    [SerializeField] Animator engineHolderAnimator;
    [SerializeField] EngineRevver engineRevver;

    

    private bool _canRev = false;


    public override void Equip()
    {
        engineRevver.Initialise();
        SetVisible(true);
        engineHolderAnimator.SetBool("Draw", true);
        VisualState = WeaponVisualState.Equipping;
    }

    public override void Unequip()
    {
        EnableRevving(false);
        engineRevver.StopEngine();
        engineHolderAnimator.SetBool("Draw", false);
        VisualState = WeaponVisualState.Unequipping;
    }

    public override void UpdateWeapon(float deltaTime, WeaponManagerInput input)
    {
        _requestedActionPressed = input.ActionPressed;
        _requestedActionDown = input.ActionDown;
        _requestedReadyPressed = input.ReadyPressed;
        _requestedReadyHeld = input.ReadyDown;

        if (!IsBusy)
        {
            // Rev the engine if not busy and left click is pressed.
            // Later we can maybe make the rev anim a half animation, where it stops at full pull of the rip cord. 
            // If the rev input is held, we can can transition to a new anim where its held and shakes at full pull.
            // If released / just pressed, we can transition to a new anim where the rip cord resets. 
            // For now we'll just stay with pressed. 
            if (_requestedActionPressed)
            {
                // Play the rev animation. The rev will trigger the engine Revver script to start the engine.
                if (_canRev) engineRevver.RevEngine();
            }
        }
    }

    // Enable the player to revv the engine (starting it, or potentially restarting / boosting it depending on if we wanna add that) - and maybe REFUEL the engine as well.
    // This is called by animation event within the Engine Draw anim - if we don't know whats calling this - CHECK ANIMS!!!
    public void OnEngineUseable()
    {
        EnableRevving(true);
    }

    public void EnableRevving(bool enable)
    {
        
        _canRev = enable;
    }

    public void OnEquipAnimFinish()
    {
        VisualState = WeaponVisualState.Equipped;
    }

    public void OnStowAnimFinish()
    {
        SetVisible(false);
        VisualState = WeaponVisualState.Unequipped;
    }

    public override WeaponEffects GetEffects()
    {
        return new WeaponEffects
            (
                Vector3.zero,

                (engineRevver.engineForce > 0f),
                engineRevver.engineForce,
                false,
                new CameraShake.CamShake(0, 0)
            );
    }
}
