using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
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
    public float horizontalDamp = 0.9f;   // 0.9→快停，0.97→滑得远

    /*────────── 私有字段 ───────────────────────*/
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

    /*============= 首帧失重协程 (避免 WebGL 穿地) =============*/
    IEnumerator Start()
    {
        float originalG = rb.gravityScale;
        rb.gravityScale = 0f;              // 暂时失重
        yield return new WaitForSeconds(3f); // 等 Tilemap Collider bake
        rb.gravityScale = originalG;       // 恢复正常重力
    }

    /*==================== Update =================*/
    void Update()
    {
        // 锁输入阶段：只更新动画，不读键盘
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
        // 锁输入时：水平速度指数衰减，不再读取新输入
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
            bool crouchKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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

    /*=========== 接口：让 GameManager 调用 ===========*/
    public void BeginNaturalStop() { /* 预留扩展 */ }
}
