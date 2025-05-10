using System.Collections;
using UnityEngine;

public class Level2SlimeController : MonoBehaviour
{
    /*──────── 公共参数 ────────*/
    [Header("Movement")] public float moveSpeed = 5f, climbSpeed = 3f, jumpForce = 8f;
    [Header("Jump / Wall")] public float wallJumpForce = 10f, defaultGravityScale = 3f, jumpDisableCollisionTime = 0.05f;
    public bool enableWallStick = true;

    [Header("Ground Check")]
    public Transform groundCheckPoint; public float groundCheckRadius = 0.05f;
    public LayerMask groundLayer, platformLayer, ladderLayer;

    [Header("Drop‑Through")] public float dropDuration = 0.25f;
    [Header("Air Jump")] public int maxExtraJumps = 1;
    [Header("Swallow")] public float swallowMoveSpeed = 2f, swallowDuration = 0.5f;
    [Header("Animator")] public Animator animator;

    [Header("Glide Settings")]
    public float glideGravityScale = 0.3f;   // 滑翔时使用的低重力
    public bool enableGlide = false;         // 是否允许滑翔能力

    [HideInInspector] public bool hasSwimAbility = false;
    [HideInInspector] public float waterDragFactor = 1f;   // 默认 1
    /*──────── 私有状态 ────────*/
    int extraJumps; bool isGrounded, isOnWall, isWallJumping;
    bool inLadderZone, climbingLadder, isSwallowing, platformIgnored;
    float swallowTimer, swallowDir, swallowOffset; Vector2 wallNormal;

    /*──────── 缓存 ───────────*/
    [HideInInspector] public Rigidbody2D rb;
    Collider2D col; int platformLayerIndex;

    /*================================================================*/
    #region Unity
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.freezeRotation = true; rb.gravityScale = defaultGravityScale;
        if (!animator) animator = GetComponent<Animator>();
        platformLayerIndex = Mathf.RoundToInt(Mathf.Log(platformLayer.value, 2));
        extraJumps = maxExtraJumps;
    }
    public void Die()
    {
        Debug.Log("[Player] 玩家死亡，触发 RespawnManager");
        RespawnManager.Instance.StartRespawn(gameObject);
    }
    void Update()
    {
        CheckGround();

        if (climbingLadder && !inLadderZone) ExitLadder();

        HandleInput();
        if (animator) animator.SetBool("IsJumping", !isGrounded);
    }

    void FixedUpdate()
    {
        if (isSwallowing)
        {
            swallowOffset += swallowDir * swallowMoveSpeed * Time.fixedDeltaTime;
            swallowTimer -= Time.fixedDeltaTime;
            if (swallowTimer <= 0f) isSwallowing = false;
        }

        if (climbingLadder) { LadderMove(); return; }

        // 滑翔逻辑
        if (!isGrounded && enableGlide && Input.GetKey(KeyCode.Space) && rb.velocity.y < 0f)
        {
            rb.gravityScale = glideGravityScale;
        }
        else if (!climbingLadder)
        {
            rb.gravityScale = defaultGravityScale;
        }

        if (isOnWall && enableWallStick)
        { StickToWall(); MoveVertical(); }
        else
        { MoveHorizontal(); }
    }
    #endregion

    #region Input
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryJump();
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) StartSwallow();

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (climbingLadder)
            {
                StartCoroutine(DropThrough());
            }
            else if (isGrounded && IsStandingOnPlatform())
            {
                StartCoroutine(DropThrough());
            }
        }

        float vRaw = Input.GetAxisRaw("Vertical");
        if (inLadderZone && !climbingLadder && Mathf.Abs(vRaw) > 0.01f)
            EnterLadder();
    }
    #endregion
    
    #region 移动
    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float vx = h * moveSpeed + swallowOffset / Time.fixedDeltaTime;
        swallowOffset = 0f;
        rb.velocity = new Vector2(vx, rb.velocity.y);

        if (animator) animator.SetFloat("Speed", Mathf.Abs(h));
        if (h != 0) transform.localScale = new Vector3(Mathf.Sign(h), transform.localScale.y, 1);
    }
    void MoveVertical() => rb.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * climbSpeed);
    #endregion

    #region Jump / Wall
    void TryJump()
    {
        if (climbingLadder) { ExitLadder(); DoJump(); return; }
        if (isOnWall) { WallJump(); return; }
        if (isGrounded) { DoJump(); return; }
        if (extraJumps > 0) { extraJumps--; DoJump(); }
    }
    void DoJump() { rb.velocity = new Vector2(rb.velocity.x, 0); rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); }
    void WallJump()
    {
        Vector2 dir = Vector2.up +
                      (Input.GetKey(KeyCode.D) ? Vector2.right :
                      (Input.GetKey(KeyCode.A) ? Vector2.left : Vector2.zero));
        if (Vector2.Dot(dir.normalized, wallNormal) > 0.8f) return;

        rb.velocity = Vector2.zero;
        rb.AddForce(dir.normalized * wallJumpForce, ForceMode2D.Impulse);
        isOnWall = false; isWallJumping = true;
        Invoke(nameof(ClearWallJump), jumpDisableCollisionTime);
    }
    void ClearWallJump() => isWallJumping = false;
    #endregion

    #region Ladder
    void EnterLadder()
    {
        climbingLadder = true;
        rb.velocity = Vector2.zero; rb.gravityScale = 0f;
        if (animator) animator.SetBool("IsClimbing", true);

        Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, true);
        platformIgnored = true;
    }

    void LadderMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(h * moveSpeed, v * climbSpeed);

        if (animator)
        {
            animator.SetBool("IsClimbing", Mathf.Abs(v) > 0.01f);
            animator.SetFloat("ClimbSpeed", Mathf.Abs(v));
        }
    }

    void ExitLadder()
    {
        climbingLadder = false; rb.gravityScale = defaultGravityScale;
        if (animator) animator.SetBool("IsClimbing", false);

        if (platformIgnored)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, false);
            platformIgnored = false;
        }
    }
    #endregion

    #region Swallow
    void StartSwallow()
    {
        animator.SetTrigger("Swallow");
        isSwallowing = true; swallowTimer = swallowDuration;
        swallowDir = transform.localScale.x > 0 ? 1 : -1; swallowOffset = 0f;
    }
    #endregion

    #region Ground & Wall
    void CheckGround()
    {
        Vector2 origin = groundCheckPoint ?
            (Vector2)groundCheckPoint.position :
            (Vector2)transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);

        isGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer | platformLayer);
        if (isGrounded) extraJumps = maxExtraJumps;
    }

    bool IsStandingOnPlatform()
    {
        Vector2 origin = groundCheckPoint ?
            (Vector2)groundCheckPoint.position :
            (Vector2)transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);
        return Physics2D.OverlapCircle(origin, groundCheckRadius, platformLayer);
    }

    void StickToWall() => rb.AddForce(-wallNormal * 10);

    void OnCollisionEnter2D(Collision2D c) => EvaluateContacts(c);
    void OnCollisionStay2D(Collision2D c) => EvaluateContacts(c);
    void OnCollisionExit2D(Collision2D c) => isOnWall = false;

    void EvaluateContacts(Collision2D col)
    {
        foreach (var ct in col.contacts)
        {
            Vector2 n = ct.normal;
            if (Vector2.Dot(n, Vector2.up) > 0.5f) isGrounded = true;

            if (!isWallJumping && enableWallStick &&
                Mathf.Abs(Vector2.Dot(n, Vector2.right)) > 0.5f)
            { isOnWall = true; wallNormal = n.normalized; }
        }
    }
    #endregion

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if ((ladderLayer.value & (1 << other.gameObject.layer)) != 0)
            inLadderZone = true;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if ((ladderLayer.value & (1 << other.gameObject.layer)) != 0)
            inLadderZone = false;
    }
    #endregion

    #region Drop‑Through
    IEnumerator DropThrough()
    {
        Debug.Log("平台穿透开始");
        Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, true);
        Debug.Log($"碰撞状态: {Physics2D.GetIgnoreLayerCollision(gameObject.layer, platformLayerIndex)}");
        yield return new WaitForSeconds(dropDuration);
        if (!climbingLadder)
        {
            Debug.Log("恢复平台碰撞");
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, false);
        }
    }
    #endregion
}
