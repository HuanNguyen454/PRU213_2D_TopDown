using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int reserveAmmo = 12;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private float pickupVolume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player_Gun playerGun = other.GetComponent<Player_Gun>();
        if (playerGun == null) return;

        playerGun.PickupGun(magazineSize, reserveAmmo);

        PlayPickupSfx2D();

        Destroy(gameObject);
    }

    private void PlayPickupSfx2D()
    {
        if (pickupSfx == null) return;

        GameObject audioObj = new GameObject("PickupSFX");
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = pickupSfx;
        source.volume = pickupVolume;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D sound

        source.Play();

        Destroy(audioObj, pickupSfx.length + 0.1f);
    }
}