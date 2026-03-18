using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 200;
    [SerializeField] private int currentHP;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.red;

    private Color originalColor;
    private Coroutine flashRoutine;

    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        currentHP = maxHP;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;

        currentHP -= dmg;

        PlayHitFlash();

        if (currentHP <= 0)
        {
            currentHP = 0;
        }
    }

    private void PlayHitFlash()
    {
        if (spriteRenderer == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}