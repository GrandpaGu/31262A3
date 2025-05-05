using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashTimer;
    private float lastDashTime = -999f;

    private Vector2 originalVelocity;





    /*──────────── Movement Settings ────────────*/
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 8f;
    public float wallJumpForce = 10f;
    public float defaultGravityScale = 3f;
    public float jumpDisableCollisionTime = 0.05f;

    /*─────────── Crouch Settings ───────────────*/
    [Header("Crouch")]
    public bool enableCrouch = true;
    public float normalColliderRadius = 0.5f;
    public float crouchColliderRadius = 0.25f;

    /*────────── Wall-Stick Settings ────────────*/
    [Header("Wall Stick")]
    public bool canStickToWall = false;

    /*────────── Ground Check ───────────────────*/
    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    /*────────── Air Jump Settings ──────────────*/
    [Header("Air Jump")]
    public int maxExtraJumps = 1;

    /*────────── Animator ───────────────────────*/
    [Header("Animator")]
    public Animator animator;

    /*────────── Lock-Input Damping ─────────────*/
    [Header("Lock-Input Damping (0.85-0.97)")]
    [Range(0.5f, 0.99f)]
    public float horizontalDamp = 0.9f;

    /*────────── Private fields ────────────────*/
    int extraJumpsRemaining;
    bool isGrounded, groundHitThisFrame;
    bool isOnWall, isWallJumping;
    Vector2 wallNormal;

    Rigidbody2D rb;
    CircleCollider2D circleCol;
    Vector3 originalScale, crouchScale;

    /*==================== Awake =================*/
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCol = GetComponent<CircleCollider2D>();

        rb.freezeRotation = true;
        rb.gravityScale = defaultGravityScale;

        if (circleCol) circleCol.radius = normalColliderRadius;
        originalScale = transform.localScale;
        crouchScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);

        if (!animator) animator = GetComponent<Animator>();
        extraJumpsRemaining = maxExtraJumps;
    }

    /*============= 首帧锁定 3 秒 ===============*/
    IEnumerator Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 记录游戏中应保持的约束（只锁旋转）
        RigidbodyConstraints2D originalConstraints = RigidbodyConstraints2D.FreezeRotation;

        // 1️⃣ 完全冻结位置与旋转
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // 2️⃣ 等 3 秒（让 Tilemap Collider 完全生成）
        yield return new WaitForSeconds(0f);

        // 3️⃣ 恢复正常约束
        rb.constraints = originalConstraints;
    }

    /*==================== Update ================*/
    void Update()
    {
        if (!isDashing && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartDash();
            }
        }
        if (GameManager.Instance != null && GameManager.Instance.isInputLocked)
        {
            UpdateAnimator();
            return;
        }

        groundHitThisFrame = false;
        GroundCheckOverlap();
        if (isGrounded) extraJumpsRemaining = maxExtraJumps;

        HandleInput();

        if (!canStickToWall && isOnWall)
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
        }

        UpdateAnimator();
    }

    /*================== FixedUpdate ==============*/
    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.isInputLocked)
        {
            rb.velocity = new Vector2(rb.velocity.x * horizontalDamp, rb.velocity.y);
            return;
        }

        if (isOnWall && canStickToWall)
        {
            rb.gravityScale = 0f;
            StickToWall();
            MoveVertical();
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
            MoveHorizontal();
        }
    }

    /*==================== Input =================*/
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryJump();

        if (enableCrouch && circleCol)
        {
            bool crouchKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightShift);
            circleCol.radius = crouchKey ? crouchColliderRadius : normalColliderRadius;
            float scaleY = crouchKey ? crouchScale.y : originalScale.y;
            transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
        }
    }

    /*================ Horizontal Move ============*/
    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

        if (animator) animator.SetFloat("Speed", Mathf.Abs(h));

        if (h < 0)
            transform.localScale = new Vector3(-originalScale.x, transform.localScale.y, transform.localScale.z);
        else if (h > 0)
            transform.localScale = new Vector3(originalScale.x, transform.localScale.y, transform.localScale.z);
    }

    /*================ Vertical Move ==============*/
    void MoveVertical()
    {
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(0f, v * climbSpeed);
    }

    /*==================== Jump ===================*/
    void TryJump()
    {
        if (isOnWall) { WallJump(); return; }
        if (isGrounded) { DoJump(); return; }

        if (extraJumpsRemaining > 0)
        {
            extraJumpsRemaining--;
            DoJump();
        }
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    /*================== Wall Jump ================*/
    void WallJump()
    {
        Vector2 dir = Vector2.up +
                      (Input.GetKey(KeyCode.D) ? Vector2.right :
                      (Input.GetKey(KeyCode.A) ? Vector2.left : Vector2.zero));

        dir = dir.normalized;
        if (Vector2.Dot(dir, wallNormal) > 0.8f) return;

        rb.velocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale;
        rb.AddForce(dir * wallJumpForce, ForceMode2D.Impulse);

        isOnWall = false;
        isWallJumping = true;
        Invoke(nameof(ClearWallJumpFlag), jumpDisableCollisionTime);
    }
    void ClearWallJumpFlag() => isWallJumping = false;

    /*================ Ground / Wall ==============*/
    void GroundCheckOverlap()
    {
        if (groundCheckPoint)
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        else
        {
            Vector2 origin = transform.position;
            origin.y -= (circleCol ? circleCol.radius : 0.5f) + 0.05f;
            isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);
        }
    }

    void StickToWall() => rb.AddForce(-wallNormal * 20f);

    void OnCollisionEnter2D(Collision2D col) => EvaluateContacts(col);
    void OnCollisionStay2D(Collision2D col) => EvaluateContacts(col);
    void OnCollisionExit2D(Collision2D col) => isOnWall = false;

    void EvaluateContacts(Collision2D col)
    {
        foreach (var c in col.contacts)
        {
            Vector2 n = c.normal;

            if (Vector2.Dot(n, Vector2.up) > 0.5f) groundHitThisFrame = true;

            if (!isWallJumping && canStickToWall &&
                (Mathf.Abs(Vector2.Dot(n, Vector2.left)) > 0.5f ||
                 Mathf.Abs(Vector2.Dot(n, Vector2.right)) > 0.5f))
            {
                isOnWall = true;
                wallNormal = n.normalized;
            }
        }
        if (groundHitThisFrame) isGrounded = true;
    }

    /*================== Animator =================*/
    void UpdateAnimator()
    {
        if (animator) animator.SetBool("IsJumping", !isGrounded);
    }

    /*=========== 外部接口 (留空) ============*/
    public void BeginNaturalStop() { }


    

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;
        rb.gravityScale = 0f;
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 3f;
        rb.velocity = Vector2.zero;
    }






}


