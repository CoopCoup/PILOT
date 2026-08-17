using UnityEngine;

public class EnginePickup : MonoBehaviour, IInteractable
{
    public bool CanInteract(Player player)
    {
        return true;
    }

    public string InteractionPrompt => "E to Pickup ENGINE";

    public void Interact(Player player)
    {
        player?.PickupWeapon(WeaponID.Engine);
        Destroy(gameObject);
    }

}
