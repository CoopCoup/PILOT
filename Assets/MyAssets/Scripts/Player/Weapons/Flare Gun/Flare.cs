using UnityEngine;

public class Flare : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Initialise(Vector3 shootDirection, float shootSpeed, float gravity, float lifetime)
    {
        rb.linearVelocity = (shootDirection * shootSpeed);

        Destroy(gameObject, lifetime);
    }
}
