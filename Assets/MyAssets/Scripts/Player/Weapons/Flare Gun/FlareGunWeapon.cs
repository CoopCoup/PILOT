using UnityEngine;

public class FlareGunWeapon : WeaponBase
{
    public override WeaponID ID => WeaponID.FlareGun;
    [SerializeField] private Animator HolderAnimator;
    [SerializeField] private Animator GunAnimator;
    [SerializeField] private float fireRate = 0.75f;
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private Transform firePoint;
    [Space]
    [Header("Flare Variables")]
    [SerializeField] private float flareSpeed = 30f;
    [SerializeField] private float flareGravity = -25f;
    [SerializeField] private float flareLifetime = 4f;
    [SerializeField] private CameraShake.CamShake firingShake;

    private float fireCooldown;
    private bool _cameraShakeRequested;

    public override void Equip()
    {
        base.Equip();
    }

    public override void Unequip()
    {
        base.Unequip();
    }

    public override void UpdateWeapon(float deltaTime, WeaponManagerInput input)
    {
        _requestedActionPressed = input.ActionPressed;
        _requestedActionDown = input.ActionDown;
        _requestedReadyPressed = input.ReadyPressed;
        _requestedReadyHeld = input.ReadyDown;

        _playerCam = input.PlayerCamera;

        if (!IsBusy)
        {
            if (_requestedActionDown && fireCooldown <= 0f) FireFlare();
        }

        if (fireCooldown > 0f)
        {
            fireCooldown -= deltaTime;
        }
    }

    private void FireFlare()
    {
        _cameraShakeRequested = true;
        fireCooldown = fireRate;
        Ray ray = _playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
        else
        {
            targetPoint = ray.GetPoint(150); // Just a point far away from the player.
        }

        // Calculate direction from the firePoint at the gun barrel to the centre of the crosshairs - the target point
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        GameObject flareObject = Instantiate(flarePrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
        Flare flare = flareObject.GetComponent<Flare>();

        GunAnimator.SetTrigger("Fire");
        flare.Initialise(fireDirection, flareSpeed, flareGravity, flareLifetime);
    }

    public override WeaponEffects GetEffects()
    {
        WeaponEffects effects = new WeaponEffects
            (
                Vector3.zero,
                false,
                0f,                
                _cameraShakeRequested,
                firingShake
            );

        _cameraShakeRequested = false;
        return effects;
    }
}
