using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Damage : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player_Health hp = collision.collider.GetComponent<Player_Health>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
    }
}
