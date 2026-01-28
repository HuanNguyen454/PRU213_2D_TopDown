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
        if (sr != null) facingDir = sr.flipX ? Vector2.left : Vector2.right;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (anim != null) anim.SetTrigger(AttackHash);
    }

    // Animation Event: g?i ?úng frame trúng ?òn (AOE)
    public void AnimEvent_DealDamage()
    {
        Vector2 hitPos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, hitRadius);

        var damaged = new System.Collections.Generic.HashSet<Enemy_Health>();

        for (int i = 0; i < hits.Length; i++)
        {
            // ? B? qua trigger (vòng detect c?a zombie)
            if (hits[i].isTrigger) continue;

            Transform root = hits[i].transform.root;
            if (!root.CompareTag(enemyTag)) continue;

            Enemy_Health hp = root.GetComponent<Enemy_Health>();
            if (hp != null && damaged.Add(hp))
                hp.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 pos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
        Gizmos.DrawWireSphere(pos, hitRadius);
    }
}
