using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useFlipForLeftRight = true;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 lastCardinalDir = Vector2.down; 

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LastMoveXHash = Animator.StringToHash("LastMoveX");
    private static readonly int LastMoveYHash = Animator.StringToHash("LastMoveY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed) moveInput = context.ReadValue<Vector2>();
        else if (context.canceled) moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector2 dir = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;

        rb.velocity = dir * moveSpeed;

        if (anim != null)
        {
            bool isMoving = dir.sqrMagnitude > 0.01f;
            anim.SetBool(IsMovingHash, isMoving);

            anim.SetFloat(MoveXHash, dir.x);
            anim.SetFloat(MoveYHash, dir.y);

            if (isMoving)
            {
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    lastCardinalDir = new Vector2(Mathf.Sign(dir.x), 0);
                else
                    lastCardinalDir = new Vector2(0, Mathf.Sign(dir.y));

                anim.SetFloat(LastMoveXHash, lastCardinalDir.x);
                anim.SetFloat(LastMoveYHash, lastCardinalDir.y);
            }
        }

        if (useFlipForLeftRight && sr != null)
        {
            if (dir.x > 0.01f) sr.flipX = false;
            else if (dir.x < -0.01f) sr.flipX = true;
        }
    }
}