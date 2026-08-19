using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private Image promptImage;

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }


    public void SetPosition(Vector3 screenPosition)
    {
        promptImage.transform.position = screenPosition;
    }
}
