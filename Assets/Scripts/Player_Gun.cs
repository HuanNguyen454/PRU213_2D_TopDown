using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Gun : MonoBehaviour
{
    [Header("Gun State")]
    [SerializeField] private bool hasGun = false;
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int reserveAmmo = 0;

    [Header("Shoot Settings")]
    [SerializeField] private float fireCooldown = 0.2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer gunOverlayRenderer;
    [SerializeField] private Sprite shootDownSprite;
    [SerializeField] private Sprite shootSideSprite;
    [SerializeField] private float shootVisualDuration = 0.12f;

    [Header("Bullet Spawn")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    private float lastFireTime;
    private Player_Movement playerMovement;
    private Coroutine gunVisualRoutine;

    public event Action<int, int> OnAmmoChanged;

    public bool HasGun => hasGun;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;

    private void Awake()
    {
        playerMovement = GetComponent<Player_Movement>();
    }

    private void Start()
    {
        if (gunOverlayRenderer != null)
            gunOverlayRenderer.enabled = false;

        NotifyAmmoChanged();
    }

    public void PickupGun(int magSize, int reserve)
    {
        hasGun = true;
        magazineSize = magSize;
        currentAmmo = magSize;
        reserveAmmo = reserve;

        Debug.Log("Picked up gun!");
        NotifyAmmoChanged();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryShoot();
    }

    private void TryShoot()
    {
        if (!hasGun)
        {
            Debug.Log("No gun!");
            return;
        }

        if (Time.time < lastFireTime + fireCooldown)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        lastFireTime = Time.time;
        currentAmmo--;

        ShowGunOverlay();
        SpawnBullet();

        Debug.Log($"Ammo: {currentAmmo}/{reserveAmmo}");
        NotifyAmmoChanged();
    }

    private void ShowGunOverlay()
    {
        if (gunOverlayRenderer == null) return;

        Vector2 dir = playerMovement != null ? playerMovement.LastMoveDirection : Vector2.down;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            gunOverlayRenderer.sprite = shootSideSprite;
            gunOverlayRenderer.flipX = dir.x < 0f;
        }
        else
        {
            gunOverlayRenderer.sprite = shootDownSprite;
            gunOverlayRenderer.flipX = false;
        }

        gunOverlayRenderer.enabled = true;

        if (gunVisualRoutine != null)
            StopCoroutine(gunVisualRoutine);

        gunVisualRoutine = StartCoroutine(HideGunOverlayAfterDelay());
    }

    private IEnumerator HideGunOverlayAfterDelay()
    {
        yield return new WaitForSeconds(shootVisualDuration);

        if (gunOverlayRenderer != null)
            gunOverlayRenderer.enabled = false;
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet == null) return;

        Vector2 dir = playerMovement != null ? playerMovement.LastMoveDirection : Vector2.down;

        if (dir == Vector2.zero)
            dir = Vector2.down;

        bullet.Launch(dir);
    }

    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
}