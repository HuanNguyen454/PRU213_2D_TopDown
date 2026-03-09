using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AxeProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2.5f;
    [SerializeField] private int damage = 15;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private Vector2 dir = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction)
    {
        dir = direction.sqrMagnitude < 0.001f ? Vector2.right : direction.normalized;
        rb.velocity = dir * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only hit Player layer
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        Player_Health hp = other.GetComponent<Player_Health>();
        if (hp != null) hp.TakeDamage(damage);

        Destroy(gameObject);
    }
}