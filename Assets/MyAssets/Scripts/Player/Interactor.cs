using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactionRange;
    [SerializeField] private LayerMask interactionMask;

    private IInteractable _currentInteractable;
    public IInteractable CurrentInteractable => _currentInteractable;

    public void UpdateInteractor(Transform camera)
    {
        Vector3 origin = camera.position;
        Vector3 direction = camera.forward;

        _currentInteractable = null;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, interactionRange, interactionMask))
        {
            _currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        }
    }

    public void Interact(Player player)
    {
        _currentInteractable?.Interact(player);
    }
}
