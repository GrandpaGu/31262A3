using UnityEngine;

/*
 * SlimeController — v2.4
 * -------------------------------------------------
 * ● 在地面检测中同时识别 Ground + Platform 图层
 * -------------------------------------------------*/

public class Level2SlimeController : MonoBehaviour
{
    /*────────────────── Movement ──────────────────*/
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 8f;
    public float wallJumpForce = 10f;
    public float defaultGravityScale = 3f;
    public float jumpDisableCollisionTime = 0.05f;

    /*────────────────── Crouch ────────────────────*/
    [Header("Crouch Settings")]
    public bool enableCrouch = true;
    public float normalColliderRadius = 0.5f;
    public float crouchColliderRadius = 0.25f;
    private Vector3 originalScale;
    private Vector3 crouchScale;

    /*────────────────── Wall Stick ────────────────*/
    [Header("Wall Stick Settings")]
    public bool canStickToWall = false;

    /*────────────────── Ground Check ──────────────*/
    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.05f;
    public LayerMask groundLayer;          // 原普通地面
    public LayerMask platformLayer;        // 新增：一面可穿平台

    /*────────────────── Air Jump ──────────────────*/
    [Header("Air Jump Settings")]
    public int maxExtraJumps = 1;

    /*────────────────── Swallow ───────────────────*/
    [Header("Swallow Settings")]
    public float swallowMoveSpeed = 2f;
    public float swallowDuration = 0.5f;

    /*────────────────── Animator ──────────────────*/
    [Header("Animator")]
    public Animator animator;

    /*────────────────── Internal State ────────────*/
    private int extraJumpsRemaining;
    private bool isGrounded, groundHitThisFrame;
    private bool isOnWall, isWallJumping;
    private Vector2 wallNormal;
    private bool isSwallowing = false;
    private float swallowTimer = 0f;
    private float swallowDirection = 1f;
    private float swallowOffsetX = 0f;

    /*────────────────── Cached ────────────────────*/
    private Rigidbody2D rb;
    private CircleCollider2D circleCol;

    // ======================================================================

    #region Unity Callbacks
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

    void Update()
    {
        groundHitThisFrame = false;
        GroundCheckOverlap();

        if (isGrounded) extraJumpsRemaining = maxExtraJumps;
        HandleInput();

        if (!canStickToWall && isOnWall)
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
        }

        if (animator) animator.SetBool("IsJumping", !isGrounded);
    }

    void FixedUpdate()
    {
        // Swallow 位移
        if (isSwallowing)
        {
            float step = swallowDirection * swallowMoveSpeed * Time.fixedDeltaTime;
            swallowOffsetX += step;
            swallowTimer -= Time.fixedDeltaTime;
            if (swallowTimer <= 0f) isSwallowing = false;
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
    #endregion

    // ======================================================================
    #region Input / Movement
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryJump();

        // Swallow
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            animator.SetTrigger("Swallow");
            isSwallowing = true;
            swallowTimer = swallowDuration;
            swallowDirection = transform.localScale.x > 0 ? 1f : -1f;
            swallowOffsetX = 0f;
        }

        // Crouch
        if (enableCrouch && circleCol)
        {
            bool crouchKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            circleCol.radius = crouchKey ? crouchColliderRadius : normalColliderRadius;
            float scaleY = crouchKey ? crouchScale.y : originalScale.y;
            transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
        }
    }

    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float baseVelX = h * moveSpeed;

        // 叠加吞噬位移
        baseVelX += swallowOffsetX / Time.fixedDeltaTime;
        swallowOffsetX = 0f;

        rb.velocity = new Vector2(baseVelX, rb.velocity.y);

        if (animator) animator.SetFloat("Speed", Mathf.Abs(h));

        if (h < 0) transform.localScale = new Vector3(-originalScale.x, transform.localScale.y, originalScale.z);
        else if (h > 0) transform.localScale = new Vector3(originalScale.x, transform.localScale.y, originalScale.z);
    }

    void MoveVertical()
    {
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(0f, v * climbSpeed);
    }
    #endregion

    // ======================================================================
    #region Jump Logic
    void TryJump()
    {
        if (isOnWall) { WallJump(); return; }
        if (isGrounded) { DoJump(); return; }
        if (extraJumpsRemaining > 0) { extraJumpsRemaining--; DoJump(); }
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

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
    #endregion

    // ======================================================================
    #region Ground / Wall Checks
    void GroundCheckOverlap()
    {
        // Ground + Platform 合并
        LayerMask combinedMask = groundLayer | platformLayer;

        if (groundCheckPoint)
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, combinedMask);
        else
        {
            Vector2 origin = transform.position;
            origin.y -= (circleCol ? circleCol.radius : 0.5f) + 0.05f;
            isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, combinedMask);
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
    #endregion
}
