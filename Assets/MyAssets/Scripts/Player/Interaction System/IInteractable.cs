using UnityEngine;

public interface IInteractable
{
    // Is this thing interactable?
    bool CanInteract();

    // What should the interaction UI for this interaction say? 
    string InteractionPrompt {  get; }

    // Interact pretty please
    void Interact(Player player);

    Transform GetUIHandle();
}
