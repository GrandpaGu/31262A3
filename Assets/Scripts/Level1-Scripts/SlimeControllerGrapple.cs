﻿using System.Collections;
using UnityEngine;

public class SlimeControllerGrapple : MonoBehaviour
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

    /*────────── Lock-Input Damping (0.85-0.97) ─────────────*/
    [Header("Lock-Input Damping (0.85-0.97)")]
    [Range(0.5f, 0.99f)]
    public float horizontalDamp = 0.9f;

    /*──────────── Grapple Settings ─────────────*/
    [Header("Grapple")]
    public LayerMask grappleLayer; 
    public LineRenderer grappleLine;
    public float grappleMaxDistance = 10f;
    public float grappleSwingForce = 15f;

    
    public GameObject crosshairPrefab;
    private GameObject crosshairInstance;

    
    [Header("Grapple Animation")]
    public AnimationCurve grappleShootAnimationCurve;  
    public float grappleShootDuration = 0.5f;         

    private bool isGrappling = false;
    private Vector2 grapplePoint;
    private DistanceJoint2D grappleJoint;

    /*────────── Private fields ────────────────*/
    int extraJumpsRemaining;
    bool isGrounded, groundHitThisFrame;
    bool isOnWall, isWallJumping;
    Vector2 wallNormal;

    private bool isAirborneAfterGrapple = false;

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

        grappleJoint = gameObject.AddComponent<DistanceJoint2D>();
        grappleJoint.enabled = false;
        grappleJoint.autoConfigureDistance = false;
        grappleJoint.autoConfigureConnectedAnchor = false;
        grappleJoint.enableCollision = true;
    }

    IEnumerator Start()
    {
        RigidbodyConstraints2D originalConstraints = RigidbodyConstraints2D.FreezeRotation;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(0f);
        rb.constraints = originalConstraints;
    }

    /*==================== Update ================*/
    void Update()
    {
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

        if (isGrappling && grappleLine)
        {
            grappleLine.SetPosition(0, transform.position);
            grappleLine.SetPosition(1, grapplePoint);
        }

       
        if (crosshairPrefab != null)
        {
            UpdateCrosshair();
        }
    }

    /*================== FixedUpdate ==============*/
    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isInputLocked)
        {
            rb.velocity = new Vector2(rb.velocity.x * horizontalDamp, rb.velocity.y);
            return;
        }

     
        if (isGrappling)
        {
            // Direction from player to anchor
            Vector2 toAnchor = grapplePoint - rb.position;
            Vector2 tangent = Vector2.Perpendicular(toAnchor).normalized;
            if (Vector2.Dot(tangent, Vector2.right) < 0) tangent = -tangent;
            float input = Input.GetAxisRaw("Horizontal");
            rb.AddForce(tangent * input * grappleSwingForce);
        }
        else if (isAirborneAfterGrapple)
        {
           
            Vector2 momentum = rb.velocity;
            rb.velocity = new Vector2(momentum.x, rb.velocity.y);

            // Keep the player in the air with swing momentum until they hit the ground
            if (isGrounded) 
            {
                isAirborneAfterGrapple = false;
            }
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
            MoveHorizontal();
        }

        if (isOnWall && canStickToWall)
        {
            rb.gravityScale = 0f;
            StickToWall();
            MoveVertical();
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

        if (Input.GetMouseButtonDown(0) && !isGrappling) TryGrapple();
        if (Input.GetMouseButtonDown(1) && isGrappling) CancelGrapple();
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

    /*================ Grapple Methods =================*/
    void TryGrapple()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        if (hit.collider != null)
        {
            isGrappling = true;
            grapplePoint = hit.point;

            grappleJoint.enabled = true;
            grappleJoint.connectedAnchor = grapplePoint;
            grappleJoint.distance = Vector2.Distance(transform.position, grapplePoint);

            if (grappleLine)
            {
                grappleLine.enabled = true;
                grappleLine.positionCount = 2;
                StartCoroutine(AnimateGrappleShoot()); 
            }
        }
    }

    void CancelGrapple()
    {
        if (isGrappling)
        {
            
            Vector2 momentum = rb.velocity;

            // Disable the grapple
            isGrappling = false;
            grappleJoint.enabled = false;
            if (grappleLine) grappleLine.enabled = false;

            rb.velocity = new Vector2(momentum.x, rb.velocity.y);

           
            isAirborneAfterGrapple = true;

          
            if (crosshairInstance != null)
            {
                Destroy(crosshairInstance);
            }
        }
    }

    /*=========== 外部接口 (留空) ============*/
    public void BeginNaturalStop() { }

   
    void UpdateCrosshair()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        Vector2 crosshairPos = transform.position + (Vector3)(direction * grappleMaxDistance);

       
        if (hit.collider != null)
        {
            crosshairPos = hit.point;
        }

        if (crosshairInstance == null)
        {
            crosshairInstance = Instantiate(crosshairPrefab, crosshairPos, Quaternion.identity);
        }
        else
        {
            crosshairInstance.transform.position = crosshairPos;
        }
    }

    // Grapple line shoot animation using AnimationCurve
    private IEnumerator AnimateGrappleShoot()
    {
        float timeElapsed = 0f;

       
        while (timeElapsed < grappleShootDuration)
        {
            timeElapsed += Time.deltaTime;

            
            float curveValue = grappleShootAnimationCurve.Evaluate(timeElapsed / grappleShootDuration);

            
            Vector2 extendedGrapplePoint = Vector2.Lerp(transform.position, grapplePoint, curveValue);

            
            grappleLine.SetPosition(1, extendedGrapplePoint);

            yield return null;
        }

        
        grappleLine.SetPosition(1, grapplePoint);
    }
}
