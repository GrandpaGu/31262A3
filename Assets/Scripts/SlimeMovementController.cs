using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 8f;
    public float wallJumpForce = 10f;
    public float defaultGravityScale = 3f;
    public float jumpDisableCollisionTime = 0.05f;

    [Header("Crouch Settings")]
    public bool enableCrouch = true;
    public float normalColliderRadius = 0.5f;
    public float crouchColliderRadius = 0.25f;
    private Vector3 originalScale;
    private Vector3 crouchScale;

    [Header("Wall Stick Settings")]
    public bool canStickToWall = false; // Toggle wall-sticking ability

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    private bool isOnWall = false;
    private Vector2 wallNormal;
    private bool isWallJumping = false;

    [Header("Animator")]
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        rb.freezeRotation = true;
        rb.gravityScale = defaultGravityScale;

        if (circleCollider != null)
        {
            circleCollider.radius = normalColliderRadius;
        }
        originalScale = transform.localScale;
        crouchScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        HandleInput();

        if (isGrounded)
        {
            Debug.Log("Character is grounded.");
        }

        if (!canStickToWall && isOnWall)
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
        }

        UpdateAnimationParameters();
    }

    void FixedUpdate()
    {
        UpdateAnimationParameters();

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

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (isOnWall)
            {
                Vector2 jumpDir = Vector2.up;
                if (Input.GetKey(KeyCode.D)) jumpDir += Vector2.right;
                if (Input.GetKey(KeyCode.A)) jumpDir += Vector2.left;
                jumpDir = jumpDir.normalized;

                float dot = Vector2.Dot(jumpDir, wallNormal);
                if (dot > 0.8f)
                {
                    Debug.Log("[Wall Jump] Blocked: Jump direction too close to wall");
                    return;
                }

                rb.velocity = Vector2.zero;
                rb.gravityScale = defaultGravityScale;
                rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);
                isOnWall = false;
                isWallJumping = true;
                Invoke(nameof(ResetWallJump), jumpDisableCollisionTime);
                Debug.Log("[Wall Jump] Executed");
            }
            else if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);                
                Debug.Log("[Jump] Normal jump");
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log("[Jump] Normal jump");
            }
        }

        if (enableCrouch && circleCollider != null)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                circleCollider.radius = crouchColliderRadius;
                transform.localScale = new Vector3(transform.localScale.x, crouchScale.y, transform.localScale.z);
            }
            else
            {
                circleCollider.radius = normalColliderRadius;
                transform.localScale = new Vector3(transform.localScale.x, originalScale.y, transform.localScale.z);
            }
        }
    }

    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

        animator.SetFloat("Speed", Mathf.Abs(h));

        if (h != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(h) * Mathf.Abs(originalScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void MoveVertical()
    {
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(0f, v * climbSpeed);
    }

    void ResetWallJump()
    {
        isWallJumping = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire"))
        {
            Debug.Log("[Trigger] Slime touched fire! Respawning...");
            // gameManager?.RespawnSlime(); // Remove GameManager reference temporarily
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isWallJumping) return;

        if (!canStickToWall) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            if (Mathf.Abs(Vector2.Dot(normal, Vector2.left)) > 0.5f || Mathf.Abs(Vector2.Dot(normal, Vector2.right)) > 0.5f)
            {
                isOnWall = true;
                wallNormal = normal.normalized;
                Debug.Log("[Wall] Slime attached to wall");
                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isOnWall = false;
    }

    void StickToWall()
    {
        rb.AddForce(-wallNormal * 20f);
    }

    void UpdateAnimationParameters()
    {
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false); 
        }
        else
        {
            animator.SetBool("IsJumping", true); 
        }
    }

}
