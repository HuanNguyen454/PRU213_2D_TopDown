using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Attack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float attackRange = 1.0f;   // ??y tâm vùng ?ánh ra tr??c m?t
    [SerializeField] private float hitRadius = 0.6f;     // bán kính AOE
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private string enemyTag = "Enemy";

    private Animator anim;
    private SpriteRenderer sr;

    private float nextAttackTime;
    private Vector2 facingDir = Vector2.right;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr != null)
            facingDir = sr.flipX ? Vector2.left : Vector2.right;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (anim != null)
            anim.SetTrigger(AttackHash);
    }

    // Animation Event: g?i ?úng frame trúng ?òn (AOE)
    public void AnimEvent_DealDamage()
    {
        Vector2 hitPos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, hitRadius);

        HashSet<Transform> damagedRoots = new HashSet<Transform>();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // B? qua trigger (ví d? detect zone)
            if (hits[i].isTrigger) continue;

            Transform root = hits[i].transform.root;
            if (root == null) continue;

            if (!root.CompareTag(enemyTag)) continue;

            // Tránh gây damage 2 l?n cho cùng 1 enemy trong 1 nhát
            if (!damagedRoots.Add(root)) continue;

            Enemy_Health enemyHp = root.GetComponent<Enemy_Health>();
            if (enemyHp != null)
            {
                enemyHp.TakeDamage(damage);
                continue;
            }

            BossHealth bossHp = root.GetComponent<BossHealth>();
            if (bossHp != null)
            {
                bossHp.TakeDamage(damage);
                continue;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 pos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
        Gizmos.DrawWireSphere(pos, hitRadius);
    }
}