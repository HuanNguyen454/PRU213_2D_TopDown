using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    public int healAmount = 10;
    public float lifeTime = 20f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player_Health player = other.GetComponent<Player_Health>();

            if (player != null && player.CurrentHP < player.MaxHP)
            {
                player.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}