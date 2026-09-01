using UnityEngine;

public class FlareGunPickup : MonoBehaviour, IInteractable
{

    [SerializeField] private Transform uiHandle;

    public string InteractionPrompt => "E to Pickup FLARE GUN";

    public bool CanInteract()
    {
        return true;
    }

    public Transform GetUIHandle() => uiHandle;
    public void Interact(Player player)
    {
        player?.PickupWeapon(WeaponID.FlareGun);
        Destroy(gameObject);
    }
}
