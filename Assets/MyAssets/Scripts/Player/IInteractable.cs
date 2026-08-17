using UnityEngine;

public interface IInteractable
{
    // Is this thing interactable?
    bool CanInteract(Player player);

    // What should the interaction UI for this interaction say? 
    string InteractionPrompt {  get; }

    // Interact pretty please
    void Interact(Player player);
}
