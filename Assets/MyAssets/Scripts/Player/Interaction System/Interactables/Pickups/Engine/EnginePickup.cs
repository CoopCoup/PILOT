using UnityEngine;

public class EnginePickup : MonoBehaviour, IInteractable
{
    [SerializeField] Transform uiHandle;
    
    public bool CanInteract()
    {
        return true;
    }

    public string InteractionPrompt => "E to Pickup ENGINE";

    public void Interact(Player player)
    {
        player?.PickupWeapon(WeaponID.Engine);
        Destroy(gameObject);
    }

    public Transform GetUIHandle() => uiHandle;
}
