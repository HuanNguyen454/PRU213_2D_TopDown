using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CompanionFollow : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float followRadius = 5f;

    [Header("Move Speed")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeedMultiplier = 2f;

    [Header("Visual")]
    [SerializeField] private bool useFlipForLeftRight = true;

    [Header("Sprite Facing")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    private Transform player;

    // Runtime state (set from CompanionCombat)
    private bool isEnemyNear;
    private Transform combatTarget;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    // ===== Animator params (PascalCase) =====
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    public bool IsEnemyNear
    {
        get => isEnemyNear;
        set => isEnemyNear = value;
    }

    public void SetCombatTarget(Transform target) => combatTarget = target;
    public void ClearCombatTarget() => combatTarget = null;

    public bool IsFacingLeft => sr != null && sr.flipX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // 🔥 Tự tìm Player bằng tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy object có tag 'Player'");
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            rb.velocity = Vector2.zero;
            SetAnim(false, false, false);
            return;
        }

        Transform moveTarget = combatTarget != null ? combatTarget : player;

        Vector2 toTarget = (Vector2)moveTarget.position - rb.position;
        float distance = toTarget.magnitude;

        float speed = 0f;
        bool walking = false;
        bool running = false;

        // ===== Movement =====
        if (isEnemyNear)
        {
            // Attack => đứng yên
            speed = 0f;
            walking = false;
            running = false;
        }
        else if (combatTarget != null)
        {
            // Đang đuổi enemy => chạy
            if (distance > 0.05f)
            {
                running = true;
                speed = walkSpeed * runSpeedMultiplier;
            }
        }
        else
        {
            // Follow player
            if (distance > followRadius)
            {
                running = true;
                speed = walkSpeed * runSpeedMultiplier;
            }
            else if (distance > followDistance)
            {
                walking = true;
                speed = walkSpeed;
            }
        }

        Vector2 dir = distance > 0.001f ? toTarget.normalized : Vector2.zero;
        rb.velocity = dir * speed;

        // ===== Animator =====
        SetAnim(walking, running, isEnemyNear);

        // ===== Flip =====
        if (useFlipForLeftRight && sr != null && speed > 0.01f && Mathf.Abs(dir.x) > 0.01f)
        {
            if (dir.x > 0.01f) sr.flipX = !spriteFacesRightByDefault;
            else if (dir.x < -0.01f) sr.flipX = spriteFacesRightByDefault;
        }
    }

    private void SetAnim(bool walking, bool running, bool attacking)
    {
        if (anim == null) return;

        if (attacking)
        {
            anim.SetBool(IsAttackingHash, true);
            anim.SetBool(IsWalkingHash, false);
            anim.SetBool(IsRunningHash, false);
            return;
        }

        anim.SetBool(IsAttackingHash, false);
        anim.SetBool(IsRunningHash, running);
        anim.SetBool(IsWalkingHash, walking && !running);
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, followRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, followDistance);
    }
}