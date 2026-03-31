using System.Collections;
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

    public int CurrentHP => currentHP;
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
            Die();
        }
    }

    private void Die()
    {
        // Gửi thông báo đến TẤT CẢ các script gắn trên cùng Object này
        // Con Boss 1 có hàm "OnBossDead" sẽ chạy, Boss 2 có cũng sẽ chạy.
        SendMessage("OnBossDead", SendMessageOptions.DontRequireReceiver);

        // Vô hiệu hóa Collider để không nhận thêm damage hoặc chặn đường
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log(gameObject.name + " đã bị tiêu diệt!");
    }

    private void PlayHitFlash()
    {
        if (spriteRenderer == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}