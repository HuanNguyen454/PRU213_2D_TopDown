using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int reserveAmmo = 12;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player_Gun playerGun = other.GetComponent<Player_Gun>();
        if (playerGun == null) return;

        playerGun.PickupGun(magazineSize, reserveAmmo);
        Destroy(gameObject);
    }
}