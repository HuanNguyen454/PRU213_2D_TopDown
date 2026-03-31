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

            // --- ĐOẠN QUAN TRỌNG: Chỉ chọn mục tiêu CÒN SỐNG ---
            BossHealth h = root.GetComponent<BossHealth>();
            if (h != null && h.IsDead) continue;

            Enemy_Health eh = root.GetComponent<Enemy_Health>();
            if (eh != null && eh.IsDead) continue;
            // -----------------------------------------------

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
            // Kiểm tra xem mục tiêu có script BossHealth và đã chết chưa
            BossHealth targetHealth = currentTarget.GetComponent<BossHealth>();
            if (targetHealth != null && targetHealth.IsDead)
            {
                currentTarget = null; // Bỏ mục tiêu nếu nó đã chết
                if (follow != null) follow.ClearCombatTarget();
                return;
            }

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

        Vector2 directionToEnemy = (currentTarget.position - transform.position).normalized;
        Vector3 hitPos = transform.position + (Vector3)(directionToEnemy * attackRange);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, hitRadius);

        // Tạo danh sách để không gây sát thương trùng lặp trong 1 nhát cắn
        var damagedRoots = new HashSet<Transform>();

        foreach (var col in hits)
        {
            if (col.isTrigger) continue;

            Transform root = col.transform.root;
            if (!root.CompareTag(enemyTag)) continue;

            // Tránh gây damage 2 lần cho cùng 1 root
            if (!damagedRoots.Add(root)) continue;

            // 1. Kiểm tra máu quái thường
            Enemy_Health hp = root.GetComponent<Enemy_Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log("Companion hit Enemy: " + root.name);
                continue; // Đánh trúng rồi thì bỏ qua check dưới
            }

            // 2. Kiểm tra máu Boss (Thêm đoạn này)
            BossHealth bossHp = root.GetComponent<BossHealth>();
            if (bossHp != null)
            {
                bossHp.TakeDamage(damage);
                Debug.Log("Companion hit Boss: " + root.name);
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