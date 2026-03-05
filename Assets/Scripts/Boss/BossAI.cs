using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [Header("Detect (Circle Trigger)")]
    [SerializeField] private CircleCollider2D detectCollider; // Is Trigger
    [SerializeField] private float detectOffsetDistance = 1.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 0.4f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float windup = 0.25f;

    [Header("Throw Axe (Second Attack)")]
    [SerializeField] private float throwMinDistance = 2.5f; // ? ch? ném khi Player ?? xa ?? nhìn rìu bay
    [SerializeField] private float throwMaxDistance = 8.0f; // ? tùy ch?n: quá xa thì không ném
    [SerializeField] private float throwWindup = 0.35f;     // ? l?y ?à ném (có th? b?ng/khác windup melee)

    [Header("Pattern")]
    [SerializeField] private int firstAttackCountBeforeSecond = 6;

    [Header("Damage (Melee)")]
    [SerializeField] private int firstAttackDamage = 12;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.6f;

    [Header("Axe Projectile")]
    [SerializeField] private AxeProjectile axePrefab;
    [SerializeField] private Transform axeSpawnPoint;
    [SerializeField] private float axeSpawnForwardOffset = 0.8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private BossHealth hp;

    private Transform target;
    private bool isChasing;

    private Vector2 facingDir = Vector2.right;

    private bool isAttacking;
    private bool isWindingUp;
    private float windupEndTime;
    private float nextAttackTime;

    private int firstAttackCounter = 0;
    private bool pendingSecond = false; // ? ?ã ?? 6 hit, ?ang ch? ném rìu

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int FirstAttackHash = Animator.StringToHash("FirstAttack");
    private static readonly int SecondAttackHash = Animator.StringToHash("SecondAttack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        hp = GetComponent<BossHealth>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (detectCollider == null) detectCollider = GetComponent<CircleCollider2D>();
        ApplyDetectOffset();
    }

    private void FixedUpdate()
    {
        if (hp != null && hp.IsDead)
        {
            StopMove();
            if (anim != null) anim.SetTrigger(DieHash);
            enabled = false;
            return;
        }

        if (!isChasing || target == null)
        {
            StopMove();
            isWindingUp = false;
            return;
        }

        if (isAttacking)
        {
            StopMove();
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;

        UpdateFacing(toTarget);

        // ? N?u ?ang "ch? ném rìu" và Player ? xa ?? ?? ph?n ?ng => ?u tiên ném (Second Attack)
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

            // Player ch?y g?n l?i thì h?y ý ??nh ném, quay v? melee/chase
            if (dist < throwMinDistance)
            {
                isWindingUp = false;
                return;
            }

            if (Time.time >= windupEndTime)
            {
                isWindingUp = false;
                isAttacking = true;
                nextAttackTime = Time.time + attackCooldown;

                pendingSecond = false;  // ? ném xong thì clear
                firstAttackCounter = 0; // ? reset chu k? 6 hit

                if (anim != null) anim.SetTrigger(SecondAttackHash);
            }
            return;
        }

        // ? N?u vào melee range: ?ánh First Attack (và sau 6 l?n s? b?t pendingSecond)
        if (dist <= meleeRange)
        {
            StopMove();

            if (Time.time < nextAttackTime) return;

            if (!isWindingUp)
            {
                isWindingUp = true;
                windupEndTime = Time.time + windup;
                return;
            }

            if (dist > meleeRange)
            {
                isWindingUp = false;
                return;
            }

            if (Time.time >= windupEndTime)
            {
                isWindingUp = false;
                isAttacking = true;
                nextAttackTime = Time.time + attackCooldown;

                if (anim != null) anim.SetTrigger(FirstAttackHash);

                // ? ??m s? l?n First Attack ?ã th?c hi?n
                firstAttackCounter++;
                if (firstAttackCounter >= firstAttackCountBeforeSecond)
                {
                    pendingSecond = true; // ? ?? 6 l?n thì "ch? ném"
                }
            }
            return;
        }
        else
        {
            isWindingUp = false;
        }

        if (dist <= stopDistance)
        {
            StopMove();
            return;
        }

        // Chase
        Vector2 dir = toTarget.normalized;
        rb.velocity = dir * moveSpeed;
        if (anim != null) anim.SetBool(IsMovingHash, true);
    }

    private void UpdateFacing(Vector2 toTarget)
    {
        // n?u player ? bên ph?i -> facingDir = right, bên trái -> left
        if (toTarget.x > 0.01f) facingDir = Vector2.right;
        else if (toTarget.x < -0.01f) facingDir = Vector2.left;

        if (sr != null) sr.flipX = (facingDir == Vector2.left);

        ApplyDetectOffset(); // luôn update offset theo facingDir
    }

    private void ApplyDetectOffset()
    {
        if (detectCollider == null) return;

        // offset X ??i theo h??ng m?t
        detectCollider.offset = new Vector2(facingDir.x * detectOffsetDistance, detectCollider.offset.y);
    }
    private void StopMove()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, false);
    }

    // ===== Animation Events =====

    // G?n vào clip First_Attack_Boss, ?úng frame trúng ?òn
    public void AnimEvent_FirstAttackDealDamage()
    {
        Vector2 hitPos = rb.position + new Vector2(facingDir.x * meleeRange, 0f);
        Collider2D col = Physics2D.OverlapCircle(hitPos, hitRadius, playerLayer);
        if (col == null) return;

        Player_Health ph = col.GetComponent<Player_Health>();
        if (ph != null) ph.TakeDamage(firstAttackDamage);
    }

    // G?n vào clip Second_Attack_Boss, ?úng frame "ném"
    public void AnimEvent_SpawnAxe()
    {
        if (axePrefab == null) return;

        Vector2 spawnPos = axeSpawnPoint != null
            ? (Vector2)axeSpawnPoint.position
            : rb.position + new Vector2(facingDir.x * axeSpawnForwardOffset, 0f);

        AxeProjectile axe = Instantiate(axePrefab, spawnPos, Quaternion.identity);
        axe.Launch(facingDir);
    }

    // G?n vào frame cu?i c?a c? 2 clip attack
    public void AnimEvent_AttackFinished()
    {
        isAttacking = false;
    }

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
            isWindingUp = false;
        }
    }
}