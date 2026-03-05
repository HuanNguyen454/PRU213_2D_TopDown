using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [Header("Detect (Circle Trigger)")]
    [SerializeField] private CircleCollider2D detectCollider; // MUST be Is Trigger
    [SerializeField] private float detectOffsetDistance = 1.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 0.6f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 2.5f;   // <- t?ng n?u boss scale l?n
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float meleeWindup = 0.25f;

    [Header("Throw Axe (Second Attack)")]
    [SerializeField] private float throwMinDistance = 3.5f; // ch? ném khi Player ?? xa ?? ph?n ?ng
    [SerializeField] private float throwMaxDistance = 9.0f; // quá xa thì thôi
    [SerializeField] private float throwWindup = 0.35f;

    [Header("Pattern")]
    [SerializeField] private int firstAttackCountBeforeSecond = 6;

    [Header("Melee Damage")]
    [SerializeField] private int firstAttackDamage = 12;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.8f;

    [Header("Axe Projectile")]
    [SerializeField] private AxeProjectile axePrefab;
    [SerializeField] private Transform axeSpawnPoint;          // child empty phía tr??c tay
    [SerializeField] private float axeSpawnForwardOffset = 0.8f; // dùng khi spawnPoint null

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private BossHealth health;

    private Transform target;
    private bool isChasing;

    private Vector2 facingDir = Vector2.right;

    private bool isAttacking;
    private bool isWindingUp;
    private float windupEndTime;
    private float nextAttackTime;

    private int firstAttackCounter;
    private bool pendingSecond; // ?ã ?? 6 hit -> ch? c? h?i ném

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int FirstAttackHash = Animator.StringToHash("FirstAttack");
    private static readonly int SecondAttackHash = Animator.StringToHash("SecondAttack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<BossHealth>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (detectCollider == null) detectCollider = GetComponent<CircleCollider2D>();
        ApplyDetectOffset();
    }

    private void FixedUpdate()
    {
        // ch?t
        if (health != null && health.IsDead)
        {
            StopMove();
            if (anim != null) anim.SetTrigger(DieHash);
            enabled = false;
            return;
        }

        // không có target
        if (!isChasing || target == null)
        {
            StopMove();
            ResetWindup();
            return;
        }

        // ?ang attack => ??ng yên
        if (isAttacking)
        {
            StopMove();
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;

        UpdateFacing(toTarget);

        // 1) ?U TIÊN NÉM n?u ?ang pendingSecond và Player ?? xa
        if (pendingSecond && dist >= throwMinDistance && dist <= throwMaxDistance)
        {
            StopMove();

            if (Time.time < nextAttackTime) return;

            if (!isWindingUp)
            {
                isWindingUp = true;
                windupEndTime = Time.time + throwWindup;
                return;
            }

            // player ch?y g?n l?i -> h?y ý ??nh ném
            if (dist < throwMinDistance)
            {
                ResetWindup();
                return;
            }

            if (Time.time >= windupEndTime)
            {
                ResetWindup();
                StartAttack(isSecond: true);
            }
            return;
        }

        // 2) MELEE n?u ?? g?n
        if (dist <= meleeRange)
        {
            StopMove();

            if (Time.time < nextAttackTime) return;

            if (!isWindingUp)
            {
                isWindingUp = true;
                windupEndTime = Time.time + meleeWindup;
                return;
            }

            // player ch?y ra kh?i melee -> h?y windup
            if (dist > meleeRange)
            {
                ResetWindup();
                return;
            }

            if (Time.time >= windupEndTime)
            {
                ResetWindup();
                StartAttack(isSecond: false);
            }
            return;
        }
        else
        {
            ResetWindup();
        }

        // 3) Chase
        if (dist <= stopDistance)
        {
            StopMove();
            return;
        }

        Vector2 dir = toTarget.normalized;
        rb.velocity = dir * moveSpeed;
        if (anim != null) anim.SetBool(IsMovingHash, true);
    }

    private void StartAttack(bool isSecond)
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (isSecond)
        {
            // ném rìu
            pendingSecond = false;
            firstAttackCounter = 0;

            if (anim != null) anim.SetTrigger(SecondAttackHash);
            return;
        }

        // first attack
        if (anim != null) anim.SetTrigger(FirstAttackHash);

        firstAttackCounter++;
        if (firstAttackCounter >= firstAttackCountBeforeSecond)
        {
            pendingSecond = true; // ch? c? h?i ném khi player ch?y xa
        }
    }

    private void ResetWindup()
    {
        isWindingUp = false;
    }

    private void UpdateFacing(Vector2 toTarget)
    {
        if (toTarget.x > 0.01f) facingDir = Vector2.right;
        else if (toTarget.x < -0.01f) facingDir = Vector2.left;

        if (sr != null) sr.flipX = (facingDir == Vector2.left);

        ApplyDetectOffset();
    }

    private void ApplyDetectOffset()
    {
        if (detectCollider == null) return;
        detectCollider.offset = new Vector2(facingDir.x * detectOffsetDistance, detectCollider.offset.y);
    }

    private void StopMove()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, false);
    }

    // ===========================
    // Animation Events
    // ===========================

    // G?n vào clip First_Attack_Boss t?i frame "trúng ?òn"
    public void AnimEvent_FirstAttackDealDamage()
    {
        Vector2 hitPos = rb.position + new Vector2(facingDir.x * meleeRange, 0f);
        Collider2D col = Physics2D.OverlapCircle(hitPos, hitRadius, playerLayer);
        if (col == null) return;

        Player_Health ph = col.GetComponent<Player_Health>();
        if (ph != null) ph.TakeDamage(firstAttackDamage);
    }

    // G?n vào clip Second_Attack_Boss t?i frame "ném rìu"
    public void AnimEvent_SpawnAxe()
    {
        if (axePrefab == null) return;

        Vector2 spawnPos = axeSpawnPoint != null
            ? (Vector2)axeSpawnPoint.position
            : rb.position + new Vector2(facingDir.x * axeSpawnForwardOffset, 0f);

        AxeProjectile axe = Instantiate(axePrefab, spawnPos, Quaternion.identity);
        axe.Launch(facingDir);
    }

    // G?n vào frame cu?i c?a First/Second attack (?? boss di chuy?n l?i)
    public void AnimEvent_AttackFinished()
    {
        isAttacking = false;
    }

    // ===========================
    // Detect Player via Trigger
    // ===========================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        target = other.transform;
        isChasing = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.transform == target)
        {
            isChasing = false;
            target = null;
            isAttacking = false;
            ResetWindup();
        }
    }

    // ===========================
    // Gizmos debug
    // ===========================

    private void OnDrawGizmosSelected()
    {
        // meleeRange (?i?u ki?n b?t ??u ?ánh)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        // hitPos + hitRadius (vùng trúng ?òn)
        Gizmos.color = Color.red;
        Vector2 pos = (Vector2)transform.position + new Vector2(facingDir.x * meleeRange, 0f);
        Gizmos.DrawWireSphere(pos, hitRadius);

        // throwMinDistance (kho?ng xa m?i ném)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, throwMinDistance);
    }
}