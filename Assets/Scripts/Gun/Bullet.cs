using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 10f;
    [SerializeField] private int damage = 10;

    private Vector2 moveDir;

    public void Launch(Vector2 dir)
    {
        moveDir = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;

        Destroy(gameObject);
    }
}