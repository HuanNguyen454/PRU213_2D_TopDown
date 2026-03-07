using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [Header("Detect (Circle Trigger)")]
    [SerializeField] private CircleCollider2D detectCollider; // MUST be Is Trigger

    [Header("Origin (Optional)")]
    [SerializeField] private Transform attackOrigin; // n?u ?? tr?ng s? dùng rb.position

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 0.6f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 2.5f;        // ?i?u ki?n b?t ??u ?ánh
    [SerializeField] private float meleeHitOffset = 1.0f;    // tâm hitbox ??t tr??c m?t
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float meleeWindup = 0.25f;

    [Header("Throw Axe (Second Attack)")]
    [SerializeField] private float throwMinDistance = 3.5f;  // ch? ném khi player ?? xa
    [SerializeField] private float throwMaxDistance = 9.0f;
    [SerializeField] private float throwWindup = 0.35f;

    [Header("Pattern")]
    [SerializeField] private int firstAttackCountBeforeSecond = 6;

    [Header("Melee Damage")]
    [SerializeField] private int firstAttackDamage = 12;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.8f;

    [Header("Axe Projectile")]
    [SerializeField] private AxeProjectile axePrefab;
    [SerializeField] private Transform axeSpawnPoint;           // child empty
    [SerializeField] private float axeSpawnForwardOffset = 0.8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private BossHealth health;

    private Transform target;
    private bool isChasing;

    private Vector2 facingDir = Vector2.right;
    private float lockedFacingX = 1f;

    private bool isAttacking;
    private bool isWindingUp;
    private float windupEndTime;
    private float nextAttackTime;

    private int firstAttackCounter;
    private bool pendingSecond;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int FirstAttackHash = Animator.StringToHash("FirstAttack");
    private static readonly int SecondAttackHash = Animator.StringToHash("SecondAttack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Vector2 OriginPos => attackOrigin != null ? (Vector2)attackOrigin.position : rb.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<BossHealth>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (detectCollider == null) detectCollider = GetComponent<CircleCollider2D>();
        if (detectCollider != null)
        {
            detectCollider.offset = Vector2.zero;
            // nh? tick Is Trigger trong Inspector
        }
    }

    private void FixedUpdate()
    {
        // Dead
        if (health != null && health.IsDead)
        {
            StopMove();
            if (anim != null) anim.SetTrigger(DieHash);
            enabled = false;
            return;
        }

        // Không có target
        if (!isChasing || target == null)
        {
            StopMove();
            ResetWindup();
            return;
        }

        // ?ang ?ánh => ??ng yên
        if (isAttacking)
        {
            StopMove();
            return;
        }

        Vector2 origin = OriginPos;
        Vector2 toTarget = (Vector2)target.position - origin;
        float dist = toTarget.magnitude;

        UpdateFacing(toTarget);

        // 1) Ném rìu khi pendingSecond và player ?? xa
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

        // 2) Melee
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

        // 3) Chase (dùng origin ?? tính dir ?n ??nh)
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
        lockedFacingX = facingDir.x; // khóa h??ng m?t t?i th?i ?i?m ra ?òn

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (isSecond)
        {
            pendingSecond = false;
            firstAttackCounter = 0;
            if (anim != null) anim.SetTrigger(SecondAttackHash);
            return;
        }

        if (anim != null) anim.SetTrigger(FirstAttackHash);

        firstAttackCounter++;
        if (firstAttackCounter >= firstAttackCountBeforeSecond)
            pendingSecond = true;
    }

    private void ResetWindup() => isWindingUp = false;

    private void UpdateFacing(Vector2 toTarget)
    {
        facingDir = (toTarget.x >= 0f) ? Vector2.right : Vector2.left;
        if (sr != null) sr.flipX = (facingDir == Vector2.left);
    }

    private void StopMove()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, false);
    }

    // ===== Animation Events =====

    // AOE tr??c m?t (player ??ng sau l?ng không ?n)
    public void AnimEvent_FirstAttackDealDamage()
    {
        Vector2 origin = OriginPos;
        Vector2 hitPos = origin + new Vector2(lockedFacingX * meleeHitOffset, 0f);

        Collider2D col = Physics2D.OverlapCircle(hitPos, hitRadius, playerLayer);
        if (col == null) return;

        Player_Health ph = col.GetComponent<Player_Health>();
        if (ph != null) ph.TakeDamage(firstAttackDamage);
    }

    // G?n event vào clip Second_Attack_Boss t?i frame ném
    public void AnimEvent_SpawnAxe()
    {
        if (axePrefab == null) return;

        Vector2 spawnPos;

        if (axeSpawnPoint != null)
        {
            // Mirror spawnpoint theo h??ng m?t (vì child không t? flip)
            Vector3 local = axeSpawnPoint.localPosition;
            float lx = Mathf.Abs(local.x) * lockedFacingX;
            spawnPos = transform.TransformPoint(new Vector3(lx, local.y, local.z));
        }
        else
        {
            spawnPos = OriginPos + new Vector2(lockedFacingX * axeSpawnForwardOffset, 0f);
        }

        AxeProjectile axe = Instantiate(axePrefab, spawnPos, Quaternion.identity);
        axe.Launch(new Vector2(lockedFacingX, 0f));
    }

    // G?n event cu?i clip First/Second attack
    public void AnimEvent_AttackFinished() => isAttacking = false;

    // ===== Detect Player via Trigger =====

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

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = attackOrigin != null ? (Vector2)attackOrigin.position : (Vector2)transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, meleeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + new Vector2(facingDir.x * meleeHitOffset, 0f), hitRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, throwMinDistance);
    }
}