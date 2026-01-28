using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player_Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;

    [SerializeField] private int currentHP;   // ?�y m?i l� c�i Unity serialize ???c
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public bool IsDead => currentHP <= 0;
    public event Action OnDied;
    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        Debug.Log($"Player HP: {currentHP}/{maxHP}");

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        Debug.Log($"Player HP: {currentHP}/{maxHP}");
    }

    private void Die()
    {
        Debug.Log("Player died!");
        OnDied?.Invoke();
        // d?ng chuy?n ??ng
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // bi?n m?t ngay
        gameObject.SetActive(false);
    }
}

