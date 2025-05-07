using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Tilemaps;
using UnityEngine;

public class SlimeControllerL3 : MonoBehaviour
{
    private CircleCollider2D circle;
    /*──────────── Bounce Settings ────────────*/
    [Header("Bounce")]
    public float wallBounceXForce = 10f;
    public float wallBounceYForce = 12f;
    public float wallBounceCooldown = 0.2f;

    private bool isTouchingWall = false;
    private bool canWallBounce = true;
    private bool isBouncing = false;

    /*──────────── Dash Settings ────────────*/
    [Header("Dash")]
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
        circle = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        // 记录游戏中应保持的约束（只锁旋转）
        RigidbodyConstraints2D originalConstraints = RigidbodyConstraints2D.FreezeRotation;
        originalScale = transform.localScale;

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
        if (Input.GetButtonDown("Jump") && !isGrounded && canWallBounce)
        {
            TryWallBounce();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
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
        if (isBouncing) return;
        rb.gravityScale = defaultGravityScale;
        MoveHorizontal();
        
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
        if (isBouncing) return;
        float h = Input.GetAxisRaw("Horizontal");

        if (!isGrounded && isTouchingWall)
            h = 0;

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



    /*================== Dash =================*/
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

    /*================== Bounce =================*/
    public LayerMask wallLayer;

    void TryWallBounce()
    {
        Vector2 center = (Vector2)transform.position + circle.offset;
        float absScaleX = Mathf.Abs(transform.localScale.x);
        float radius = circle.radius * absScaleX;

        float boxWidth = 0.15f;
        float boxHeight = radius * 2.2f;
        Vector2 boxSize = new Vector2(boxWidth, boxHeight);

        float offsetX = radius + boxWidth / 2f;
        Vector2 centerLeft = new Vector2(center.x - offsetX, center.y);
        Vector2 centerRight = new Vector2(center.x + offsetX, center.y);

        bool wallLeft = Physics2D.OverlapBox(centerLeft, boxSize, 0f, wallLayer);
        bool wallRight = Physics2D.OverlapBox(centerRight, boxSize, 0f, wallLayer);

        float hInput = Input.GetAxisRaw("Horizontal");

        Debug.Log($"WallLeft: {wallLeft}, WallRight: {wallRight}, hInput: {hInput}");

        if (wallRight && hInput > 0)
        {
            WallBounce(-1); // Bounce left
        }
        else if (wallLeft && hInput < 0)
        {
            WallBounce(1);  // Bounce right
        }
    }
    void WallBounce(int bounceDirection)
    {
        if (!canWallBounce) return;

        canWallBounce = false;
        isBouncing = true;

        
        rb.velocity = new Vector2(bounceDirection * wallBounceXForce, wallBounceYForce);

       
        float newScaleX = bounceDirection == -1 ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
        transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);

        
        transform.position += new Vector3(bounceDirection * 0.05f, 0f, 0f);

        Debug.Log("Wall bounce executed");

        Invoke(nameof(ResetWallBounce), wallBounceCooldown);
        Invoke(nameof(EndBounce), 0.15f); 
    }




    void EndBounce()
    {
        isBouncing = false;
    }

    void ResetWallBounce()
    {
        canWallBounce = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isTouchingWall = true;
                return;
            }
        }
        isTouchingWall = false;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingWall = false;
        }
    }
}


