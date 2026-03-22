using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossHealth))] // Đổi từ Boss2Health thành BossHealth
public class FastBossAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Detection & Ranges")]
    [SerializeField] private float detectRadius = 12f;
    [SerializeField] private float meleeRange = 1.8f;
    [SerializeField] private float biteRange = 0.8f;
    [SerializeField] private float jumpMinDist = 5f;
    [SerializeField] private float jumpMaxDist = 10f;

    [Header("Attack Stats")]
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float jumpCooldown = 4f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private int biteDamage = 25;
    [SerializeField] private LayerMask playerLayer;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip dieSound;

    // References
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private BossHealth health; // Đổi type thành BossHealth
    private Transform target;

    // Logic Variables
    private bool isAttacking;
    private bool canJump = true;
    private int comboStep = 0;
    private float nextAttackTime;

    // Animation Hashes
    private static readonly int RunHash = Animator.StringToHash("IsRunning");
    private static readonly int BiteHash = Animator.StringToHash("Bite");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DieHash = Animator.StringToHash("Dead");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<BossHealth>(); // Lấy script BossHealth dùng chung

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Kiểm tra máu từ script dùng chung
        if (health.IsDead) return;

        if (target == null)
        {
            FindTarget();
            return;
        }

        HandleLogic();
    }

    private void FindTarget()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(transform.position, detectRadius, playerLayer);
        if (playerCol != null) target = playerCol.transform;
    }

    private void HandleLogic()
    {
        if (isAttacking) return;

        float dist = Vector2.Distance(transform.position, target.position);
        UpdateFacing();

        // 1. ƯU TIÊN NHẢY (JUMP)
        if (canJump && dist >= jumpMinDist && dist <= jumpMaxDist)
        {
            StartCoroutine(JumpRoutine());
            return;
        }

        // 2. TẤN CÔNG CẬN CHIẾN
        if (dist <= meleeRange)
        {
            StopMove();

            if (Time.time >= nextAttackTime)
            {
                if (dist <= biteRange && Random.value < 0.5f)
                {
                    ExecuteBite();
                }
                else
                {
                    ExecuteCombo();
                }
            }
        }
        // 3. ĐUỔI THEO
        else
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.velocity = dir * runSpeed;
        anim.SetBool(RunHash, true);
    }

    private void ExecuteCombo()
    {
        isAttacking = true;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        anim.SetTrigger("Atk" + comboStep);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void ExecuteBite()
    {
        isAttacking = true;
        anim.SetTrigger(BiteHash);
        nextAttackTime = Time.time + attackCooldown * 1.2f;
    }

    private IEnumerator JumpRoutine()
    {
        isAttacking = true;
        canJump = false;

        anim.SetTrigger(JumpHash);

        Vector2 jumpDir = ((Vector2)target.position - rb.position).normalized;
        rb.velocity = jumpDir * jumpForce;

        yield return new WaitForSeconds(0.6f);

        rb.velocity = Vector2.zero;
        isAttacking = false;

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void UpdateFacing()
    {
        if (target.position.x > transform.position.x) sr.flipX = false;
        else sr.flipX = true;
    }

    private void StopMove()
    {
        rb.velocity = Vector2.zero;
        anim.SetBool(RunHash, false);
    }

    public void AnimEvent_MeleeDamage()
    {
        CheckHit(meleeRange, meleeDamage);
    }

    public void AnimEvent_BiteDamage()
    {
        CheckHit(biteRange, biteDamage);
    }

    private void CheckHit(float range, int dmg)
    {
        if (target == null) return;
        if (Vector2.Distance(transform.position, target.position) <= range)
        {
            Player_Health ph = target.GetComponent<Player_Health>();
            if (ph != null) ph.TakeDamage(dmg);
        }
    }

    public void AnimEvent_AttackFinished()
    {
        isAttacking = false;
    }

    // Hàm này sẽ được BossHealth gọi thông qua SendMessage khi máu = 0
    public void OnBossDead()
    {
        StopMove();

        // 1. Kích hoạt trigger chết
        anim.SetTrigger(DieHash);

        // 2. Tắt các Bool khác để tránh xung đột logic
        anim.SetBool("IsRunning", false);
        // Nếu bạn có các bool khác như IsAttacking, hãy tắt hết ở đây

        // 3. VÔ HIỆU HÓA HOÀN TOÀN AI
        this.enabled = false;

        // Mẹo: Tắt Animator sau 0.1s nếu nó vẫn cứng đầu quay về Idle
        // Hoặc đơn giản là để clip Dead KHÔNG nối đi đâu cả (không có Exit Time)
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpMinDist);
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        audioSource.pitch = Random.Range(0.9f, 1.1f); // Giúp tiếng boss tự nhiên hơn
        audioSource.PlayOneShot(clips[randomIndex]);
    }

    // Hàm này sẽ được gọi từ Animation Event
    public void AnimEvent_PlayAttackSound()
    {
        PlayRandomSound(attackSounds);
    }

    public void AnimEvent_PlayJumpSound()
    {
        PlayRandomSound(jumpSounds);
    }
}