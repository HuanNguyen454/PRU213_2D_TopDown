using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    // Dùng cho Player Input (Behavior = Invoke Unity Events)
    // Trong Player Input > Events > Move:
    // - Performed  -> g?i hàm này
    // - Canceled   -> g?i hàm này
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            moveInput = context.ReadValue<Vector2>();
        else if (context.canceled)
            moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector2 dir = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
        rb.velocity = dir * moveSpeed;

        // Animator: c?n Bool "IsMoving" (Idle/Walk)
        if (anim != null)
        {
            bool isMoving = dir.sqrMagnitude > 0.01f;
            anim.SetBool("IsMoving", isMoving);
        }

        // Sprite m?c ??nh nhìn sang ph?i -> ?i trái thì flipX
        if (sr != null)
        {
            if (dir.x > 0.01f) sr.flipX = false;
            else if (dir.x < -0.01f) sr.flipX = true;
        }
    }
}
