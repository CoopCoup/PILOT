using UnityEngine; 


public abstract class WeaponBase : MonoBehaviour 
{
    public abstract WeaponID ID { get; }
    public WeaponVisualState VisualState { get; protected set; }

    private Renderer[] _renderers;
    public bool IsVisible { get; private set; }

    public bool IsBusy =>
        VisualState == WeaponVisualState.Unequipping;

    protected virtual void Awake()
    {
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;
        
        _renderers = GetComponentsInChildren<Renderer>(true);

        SetVisible(false);
        VisualState = WeaponVisualState.Unequipped;
    }

    protected void SetVisible(bool visible)
    {
        IsVisible = visible;
        
        foreach (var r in _renderers)
        {
            r.enabled = visible;    
        }
    }

    // Default Equip and Unequip methods instantly equip / unequip.
    public virtual void Equip()
    {
        SetVisible(true);
        VisualState = WeaponVisualState.Equipped;
    }
    public virtual void Unequip()
    {
        SetVisible(false);
        VisualState = WeaponVisualState.Unequipped;
    }

    // Each weapon implements it's own Update method as they all do things differently
    public abstract void UpdateWeapon(float deltaTime, WeaponManagerInput input);

    // Optional additonal effects this weapon has on the player, child weapons override if they do anything special
    public virtual WeaponEffects GetEffects() => default;
}
