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
    [SerializeField] private CircleCollider2D detectCollider; // CircleCollider2D (Is Trigger)
    [SerializeField] private float detectOffsetDistance = 1.0f; // vùng quét n?m tr??c m?t bao xa

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Transform target;
    private bool isChasing;

    // V?i side-walk: m?c ??nh nhìn sang ph?i, và ch? c?n trái/ph?i
    private Vector2 facingDir = Vector2.right;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // N?u b?n quên kéo detectCollider, t? tìm CircleCollider2D trên object
        if (detectCollider == null) detectCollider = GetComponent<CircleCollider2D>();

        ApplyDetectOffset(); // m?c ??nh vùng quét bên ph?i
    }

    private void FixedUpdate()
    {
        if (!isChasing || target == null)
        {
            Stop();
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float dist = toTarget.magnitude;

        if (dist <= stopDistance)
        {
            Stop();
            return;
        }

        Vector2 dir = toTarget.normalized;

        // Side-walk: ch? update h??ng theo tr?c X (trái/ph?i)
        UpdateFacing(dir);

        rb.velocity = dir * moveSpeed;

        if (anim != null)
            anim.SetBool(IsMovingHash, true);
    }

    private void UpdateFacing(Vector2 moveDir)
    {
        // N?u không có input ?áng k? theo X thì không ??i h??ng m?t
        if (Mathf.Abs(moveDir.x) < 0.01f) return;

        facingDir = moveDir.x > 0 ? Vector2.right : Vector2.left;

        // Flip sprite theo h??ng (m?c ??nh sprite nhìn sang ph?i)
        if (sr != null)
            sr.flipX = (facingDir == Vector2.left);

        // ??y vùng quét sang ?úng phía tr??c m?t
        ApplyDetectOffset();
    }

    private void ApplyDetectOffset()
    {
        if (detectCollider == null) return;

        // V?i side-walk ch? c?n offset theo X
        detectCollider.offset = new Vector2(facingDir.x * detectOffsetDistance, 0f);
    }

    private void Stop()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, false);
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
        }
    }
}