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

    [Header("Reload Settings")]
    [SerializeField] private float reloadDuration = 0.8f;
    [SerializeField] private bool autoReloadWhenEmpty = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer gunOverlayRenderer;
    [SerializeField] private Sprite shootDownSprite;
    [SerializeField] private Sprite shootSideSprite;
    [SerializeField] private float shootVisualDuration = 0.12f;

    [Header("Bullet Spawn")]
    [SerializeField] private Transform firePointDown;
    [SerializeField] private Transform firePointSide;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioClip shootSfx;
    [SerializeField] private AudioClip reloadSfx;
    [SerializeField] private AudioClip emptySfx;
    [SerializeField] private float shootVolume = 1f;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private float emptyVolume = 1f;
    [SerializeField] private float emptySfxCooldown = 0.15f;

    private float lastFireTime;
    private float lastEmptySfxTime = -999f;
    private bool isReloading = false;

    private Player_Movement playerMovement;
    private Coroutine gunVisualRoutine;
    private Coroutine reloadRoutine;

    public event Action<int, int> OnAmmoChanged;

    public bool HasGun => hasGun;
    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;

    private void Awake()
    {
        playerMovement = GetComponent<Player_Movement>();

        if (gunAudioSource == null)
            gunAudioSource = GetComponent<AudioSource>();

        if (gunAudioSource != null)
        {
            gunAudioSource.playOnAwake = false;
            gunAudioSource.loop = false;
            gunAudioSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        if (gunOverlayRenderer != null)
            gunOverlayRenderer.enabled = false;

        NotifyAmmoChanged();
    }

    public void PickupGun(int magSize, int reserve)
    {
        if (!hasGun)
        {
            hasGun = true;
            magazineSize = magSize;
            currentAmmo = magSize;
            reserveAmmo = reserve;
        }
        else
        {
            // Nh?t thêm thì c?ng vào ??n d? tr?
            reserveAmmo += reserve;
        }

        Debug.Log($"Picked up gun! Ammo now: {currentAmmo}/{reserveAmmo}");
        NotifyAmmoChanged();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryShoot();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryReload();
    }

    private void TryShoot()
    {
        if (!hasGun)
        {
            Debug.Log("No gun!");
            return;
        }

        if (isReloading)
            return;

        if (Time.time < lastFireTime + fireCooldown)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            PlayEmptySfx();

            if (autoReloadWhenEmpty)
                TryReload();

            return;
        }

        lastFireTime = Time.time;
        currentAmmo--;

        ShowGunOverlay();
        SpawnBullet();
        PlayShootSfx();

        Debug.Log($"Ammo: {currentAmmo}/{reserveAmmo}");
        NotifyAmmoChanged();

        if (currentAmmo <= 0 && autoReloadWhenEmpty)
        {
            TryReload();
        }
    }

    private void TryReload()
    {
        if (!hasGun) return;
        if (isReloading) return;
        if (currentAmmo >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        if (reloadRoutine != null)
            StopCoroutine(reloadRoutine);

        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        NotifyAmmoChanged();
        PlayReloadSfx();

        yield return new WaitForSeconds(reloadDuration);

        int needed = magazineSize - currentAmmo;
        int amountToLoad = Mathf.Min(needed, reserveAmmo);

        currentAmmo += amountToLoad;
        reserveAmmo -= amountToLoad;

        isReloading = false;

        Debug.Log($"Reloaded! Ammo: {currentAmmo}/{reserveAmmo}");
        NotifyAmmoChanged();
    }

    private void ShowGunOverlay()
    {
        if (gunOverlayRenderer == null) return;

        Vector2 dir = playerMovement != null ? playerMovement.LastMoveDirection : Vector2.down;

        if (dir == Vector2.zero)
            dir = Vector2.down;

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
        if (bulletPrefab == null) return;

        Vector2 dir = playerMovement != null ? playerMovement.LastMoveDirection : Vector2.down;

        if (dir == Vector2.zero)
            dir = Vector2.down;

        Vector3 spawnPos;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (firePointSide == null) return;

            if (dir.x > 0f)
            {
                spawnPos = firePointSide.position;
            }
            else
            {
                Vector3 localOffset = firePointSide.localPosition;
                spawnPos = transform.TransformPoint(new Vector3(-localOffset.x, localOffset.y, localOffset.z));
            }
        }
        else
        {
            if (firePointDown == null) return;
            spawnPos = firePointDown.position;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Collider2D bulletCol = bulletObj.GetComponent<Collider2D>();
        Collider2D playerCol = GetComponent<Collider2D>();
        if (bulletCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(bulletCol, playerCol);
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet == null) return;

        bullet.Launch(dir);
    }

    private void PlayShootSfx()
    {
        if (gunAudioSource == null || shootSfx == null) return;
        gunAudioSource.PlayOneShot(shootSfx, shootVolume);
    }

    private void PlayReloadSfx()
    {
        if (gunAudioSource == null || reloadSfx == null) return;
        gunAudioSource.PlayOneShot(reloadSfx, reloadVolume);
    }

    private void PlayEmptySfx()
    {
        if (gunAudioSource == null || emptySfx == null) return;

        if (Time.time < lastEmptySfxTime + emptySfxCooldown)
            return;

        lastEmptySfxTime = Time.time;
        gunAudioSource.PlayOneShot(emptySfx, emptyVolume);
    }

    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (firePointDown != null)
            Gizmos.DrawSphere(firePointDown.position, 0.05f);

        if (firePointSide != null)
            Gizmos.DrawSphere(firePointSide.position, 0.05f);
    }
}