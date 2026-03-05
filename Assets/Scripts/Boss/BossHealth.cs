using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 200;
    [SerializeField] private int currentHP;

    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
        }
    }
}