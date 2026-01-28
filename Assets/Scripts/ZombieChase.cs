using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieChase : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 0.4f;

    [Header("Detect Zone (Circle Trigger)")]
    [SerializeField] private CircleCollider2D detectCollider; // Is Trigger
    [SerializeField] private float detectOffsetDistance = 1.0f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackWindup = 0.35f;   // ? d?ng 1 nh?p tr??c khi ?ánh
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float hitRadius = 0.8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Transform target;
    private bool isChasing;

    private Vector2 facingDir = Vector2.right;

    private bool isAttacking;
    private float nextAttackTime;

    private bool isWindingUp;        // ? ?ang “l?y ?à” tr??c khi ?ánh
    private float windupEndTime;     // ? khi nào h?t l?y ?à

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (detectCollider == null) detectCollider = GetComponent<CircleCollider2D>();
        ApplyDetectOffset();
    }

    private void FixedUpdate()
    {
        if (!isChasing || target == null)
        {
            StopMove();
            isWindingUp = false; // reset
            return;
        }

        // ? ?ang Attack => ??ng yên
        if (isAttacking)
        {
            StopMove();
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;

        UpdateFacing(toTarget);

        // ? trong t?m ?ánh
        if (dist <= attackRange)
        {
            StopMove();

            // n?u ch?a t?i cooldown thì thôi
            if (Time.time < nextAttackTime)
                return;

            // B?t ??u windup n?u ch?a windup
            if (!isWindingUp)
            {
                isWindingUp = true;
                windupEndTime = Time.time + attackWindup;
                return;
            }

            // N?u Player ch?y ra kh?i t?m trong lúc windup thì h?y (hit&run)
            if (dist > attackRange)
            {
                isWindingUp = false;
                return;
            }

            // H?t windup => ?ánh
            if (Time.time >= windupEndTime)
            {
                isWindingUp = false;
                isAttacking = true;

                nextAttackTime = Time.time + attackCooldown;

                if (anim != null) anim.SetTrigger(AttackHash);
            }

            return;
        }
        else
        {
            // ra kh?i t?m ?ánh thì h?y windup
            isWindingUp = false;
        }

        if (dist <= stopDistance)
        {
            StopMove();
            return;
        }

        Vector2 dir = toTarget.normalized;
        rb.velocity = dir * moveSpeed;

        if (anim != null) anim.SetBool(IsMovingHash, true);
    }

    private void UpdateFacing(Vector2 toTarget)
    {
        if (Mathf.Abs(toTarget.x) < 0.01f) return;

        facingDir = toTarget.x > 0 ? Vector2.right : Vector2.left;

        if (sr != null) sr.flipX = (facingDir == Vector2.left);

        ApplyDetectOffset();
    }

    private void ApplyDetectOffset()
    {
        if (detectCollider == null) return;
        detectCollider.offset = new Vector2(facingDir.x * detectOffsetDistance, 0f);
    }

    private void StopMove()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, false);
    }

    // ===== Animation Events =====
    public void AnimEvent_DealDamage()
    {
        Vector2 hitPos = rb.position + new Vector2(facingDir.x * attackRange, 0f);

        Collider2D col = Physics2D.OverlapCircle(hitPos, hitRadius, playerLayer);
        if (col == null) return;

        Player_Health hp = col.GetComponent<Player_Health>();
        if (hp != null) hp.TakeDamage(damage);
    }

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
            isWindingUp = false; // reset
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 pos = (Vector2)transform.position + new Vector2(facingDir.x * attackRange, 0f);
        Gizmos.DrawWireSphere(pos, hitRadius);
    }
}