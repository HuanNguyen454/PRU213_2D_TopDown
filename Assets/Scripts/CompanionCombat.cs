using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CompanionFollow))]
public class CompanionCombat : MonoBehaviour
{
    [Header("Detect Enemy")]
    [SerializeField] private float detectRadius = 4f;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float hitRadius = 0.6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.6f;

    private Animator anim;
    private SpriteRenderer sr;
    private CompanionFollow follow;

    private Transform currentTarget;
    private float nextAttackTime;

    // ===== Animator param (PascalCase) =====
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        follow = GetComponent<CompanionFollow>();
    }

    private void Update()
    {
        if (currentTarget == null)
            FindNewTarget();

        HandleAttackLogic();
    }

    private void FindNewTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        float bestDist = Mathf.Infinity;
        Transform bestTarget = null;
 
        foreach (var col in hits)
        {
            if (col.isTrigger) continue;

            Transform root = col.transform.root;
            if (!root.CompareTag(enemyTag)) continue;

            float d = Vector2.Distance(transform.position, root.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestTarget = root;
            }
        }   

        currentTarget = bestTarget;

        if (follow != null)
        {
            if (currentTarget != null) follow.SetCombatTarget(currentTarget);
            else follow.ClearCombatTarget();
        }
    }

    private void HandleAttackLogic()
    {
        bool enemyNear = false;

        if (currentTarget != null)
        {
            float distToEnemy = Vector2.Distance(transform.position, currentTarget.position);

            if (distToEnemy > detectRadius * 1.5f || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                if (follow != null) follow.ClearCombatTarget();
            }
            else if (distToEnemy <= attackRange)
            {
                enemyNear = true;

                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + attackCooldown;
                    StartAttack();
                }
            }
        }

        if (follow != null) follow.IsEnemyNear = enemyNear;
        if (anim != null) anim.SetBool(IsAttackingHash, enemyNear);
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds; // Sử dụng mảng để chứa nhiều file âm thanh
    private int lastSoundIndex = -1; // Để theo dõi âm thanh vừa phát

    private void StartAttack()
    {
        // Nếu dùng Trigger attack riêng:
        // anim.SetTrigger("Attack");
    }

    // Animation Event gọi ở frame cắn
    public void AnimEvent_DealDamage()
    {
        PlayAttackSound();

        if (currentTarget == null) return;

        // 1. Tính toán hướng từ con chó đến mục tiêu hiện tại
        Vector2 directionToEnemy = (currentTarget.position - transform.position).normalized;

        // 2. Điểm gây sát thương (hitPos) sẽ nằm theo hướng đó, cách con chó 1 khoảng attackRange
        Vector3 hitPos = transform.position + (Vector3)(directionToEnemy * attackRange);

        // 3. Quét vòng tròn sát thương tại vị trí mới này
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, hitRadius);
        var damaged = new HashSet<Enemy_Health>();

        foreach (var col in hits)
        {
            if (col.isTrigger) continue;

            Transform root = col.transform.root;
            if (!root.CompareTag(enemyTag)) continue;

            Enemy_Health hp = root.GetComponent<Enemy_Health>();
            if (hp != null && damaged.Add(hp))
            {
                hp.TakeDamage(damage);
                Debug.Log("Companion hit: " + root.name);
            }
        }
    }

    private void PlayAttackSound()
    {
        if (audioSource == null || attackSounds == null || attackSounds.Length == 0) return;

        int randomIndex;

        // Logic để không phát trùng 1 âm thanh 2 lần liên tiếp (xen kẽ)
        if (attackSounds.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, attackSounds.Length);
            } while (randomIndex == lastSoundIndex);
        }
        else
        {
            randomIndex = 0;
        }

        lastSoundIndex = randomIndex;

        // Thay đổi pitch một chút để âm thanh tự nhiên hơn (tùy chọn)
        audioSource.pitch = Random.Range(0.9f, 1.1f);

        audioSource.PlayOneShot(attackSounds[randomIndex]);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (sr != null)
        {
            Vector2 facingDir = sr.flipX ? Vector2.left : Vector2.right;
            Vector2 hitPos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hitPos, hitRadius);
        }
    }
}