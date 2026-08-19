using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactionRange;
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private InteractionPromptUI promptUI;

    private IInteractable _currentInteractable;
    public IInteractable CurrentInteractable => _currentInteractable;

    private bool _canInteract = false;

    public void UpdateInteractor(Camera camera)
    {
        Vector3 origin = camera.transform.position;
        Vector3 direction = camera.transform.forward;

        _currentInteractable = null;
        _canInteract = false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, interactionRange, interactionMask))
        {
            _currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
            _canInteract = _currentInteractable.CanInteract();
        }

        if (_canInteract)
        {
            Vector3 worldPosition = _currentInteractable.GetUIHandle().position;
            Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

            promptUI.SetVisibility(true);
            promptUI.SetPosition(screenPosition);
        }
        else
        {
            promptUI.SetVisibility(false);
        }
    }

    public void Interact(Player player)
    {
        _currentInteractable?.Interact(player);
    }

    public bool CanInteract() => _canInteract;

}
