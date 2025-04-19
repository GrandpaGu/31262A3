using UnityEngine;

/*
 * SlimeController — v2.1
 * ---------------------
 * ● Supports configurable number of extra air jumps (maxExtraJumps)
 * ● Dual landing detection: OverlapCircle + OnCollisionStay2D normals
 * ● Resets extraJumpsRemaining upon landing to fully solve "can't jump after landing" issue
 * -------------------------------------------------------------------------*/

public class SlimeController : MonoBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 8f;
    public float wallJumpForce = 10f;
    public float defaultGravityScale = 3f;
    public float jumpDisableCollisionTime = 0.05f;

    [Header("Crouch Settings")] public bool enableCrouch = true;
    public float normalColliderRadius = 0.5f;
    public float crouchColliderRadius = 0.25f;
    private Vector3 originalScale;
    private Vector3 crouchScale;

    [Header("Wall Stick Settings")] public bool canStickToWall = false;

    [Header("Ground Check")]
    public Transform groundCheckPoint; // If null, uses offset below the character
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Air Jump Settings")] public int maxExtraJumps = 1;

    [Header("Animator")]
    public Animator animator;

    //-------------------------------- private fields ------------------------------
    private int extraJumpsRemaining;
    private bool isGrounded;           // Grounded status this frame
    private bool groundHitThisFrame;   // Set by OnCollisionStay, used as backup

    private Rigidbody2D rb;
    private CircleCollider2D circleCol;

    private bool isOnWall;
    private Vector2 wallNormal;
    private bool isWallJumping;

    //=============================================================================

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

        animator = animator ? animator : GetComponent<Animator>();
        extraJumpsRemaining = maxExtraJumps;
    }

    void Update()
    {
        //—— Reset landing flag at the beginning of each frame ——//
        groundHitThisFrame = false;

        GroundCheckOverlap();
        if (isGrounded) extraJumpsRemaining = maxExtraJumps;

        HandleInput();

        // Ensure wall stick state is disabled if not allowed
        if (!canStickToWall && isOnWall)
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
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

    //=============================================================================

    #region Input / Movement

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

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
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        if (animator) animator.SetFloat("Speed", Mathf.Abs(h));
    }

    void MoveVertical()
    {
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(0f, v * climbSpeed);
    }

    #endregion

    //=============================================================================

    #region Jump Logic

    void TryJump()
    {
        if (isOnWall)
        {
            WallJump();
            return;
        }

        if (isGrounded)
        {
            DoJump();
            return;
        }

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

    void WallJump()
    {
        Vector2 dir = Vector2.up + (Input.GetKey(KeyCode.D) ? Vector2.right : (Input.GetKey(KeyCode.A) ? Vector2.left : Vector2.zero));
        dir = dir.normalized;
        if (Vector2.Dot(dir, wallNormal) > 0.8f) return; // Prevent jumping into the wall

        rb.velocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale;
        rb.AddForce(dir * wallJumpForce, ForceMode2D.Impulse);
        isOnWall = false;
        isWallJumping = true;
        Invoke(nameof(ClearWallJumpFlag), jumpDisableCollisionTime);
    }

    void ClearWallJumpFlag() => isWallJumping = false;

    #endregion

    //=============================================================================
    void GroundCheckOverlap()
    {
        if (groundCheckPoint != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // 若未指定 groundCheckPoint，则使用当前位置偏移向下检测
            Vector2 origin = transform.position;
            origin.y -= (circleCol != null ? circleCol.radius : 0.5f) + 0.05f;
            isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);
        }
    }
    #region Collision Handling

    void StickToWall() => rb.AddForce(-wallNormal * 20f);

    void OnCollisionEnter2D(Collision2D col)
    {
        EvaluateContacts(col);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        EvaluateContacts(col);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        isOnWall = false;
    }

    void EvaluateContacts(Collision2D col)
    {
        foreach (var c in col.contacts)
        {
            Vector2 n = c.normal;

            //—— Handle landing ——//
            if (Vector2.Dot(n, Vector2.up) > 0.5f)
            {
                groundHitThisFrame = true;
            }

            //—— Handle wall stick ——//
            if (!isWallJumping && canStickToWall && (Mathf.Abs(Vector2.Dot(n, Vector2.left)) > 0.5f || Mathf.Abs(Vector2.Dot(n, Vector2.right)) > 0.5f))
            {
                isOnWall = true;
                wallNormal = n.normalized;
            }
        }

        // Finalize grounded state
        if (groundHitThisFrame) isGrounded = true;
    }

    #endregion

    //=============================================================================

    #region Animator Helper

    void UpdateAnimator()
    {
        if (animator) animator.SetBool("IsJumping", !isGrounded);
    }

    #endregion
}
