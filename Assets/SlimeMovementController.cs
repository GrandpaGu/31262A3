using UnityEngine;
using System.Collections;

public class SlimeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 12f;
    public float wallJumpForce = 10f;
    public float jumpDisableCollisionTime = 0.05f; // Time to ignore collision status after a wall jump

    private Rigidbody2D rb;

    private enum SurfaceType { None, Floor, Wall, Ceiling }
    private SurfaceType currentSurface = SurfaceType.None;

    private Vector2 lastWallNormal = Vector2.left; // Store last wall normal (left/right wall)

    private bool isJumping = false; // Indicates whether the player is temporarily ignoring collision updates after a wall jump

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleInput();

        // Draw a ray indicating the current surface normal (green if attached, red if in air)
        Color rayColor = currentSurface == SurfaceType.None ? Color.red : Color.green;
        Debug.DrawRay(transform.position, GetNormalFromSurface() * 0.5f, rayColor);

        Debug.Log("[Status] Current surface: " + currentSurface);
    }

    void FixedUpdate()
    {
        switch (currentSurface)
        {
            case SurfaceType.Floor:
                rb.gravityScale = 3f;
                MoveHorizontal();
                break;
            case SurfaceType.Wall:
                rb.gravityScale = 0f;
                MoveVertical();
                break;
            case SurfaceType.Ceiling:
                rb.gravityScale = 0f;
                MoveHorizontal();
                break;
            case SurfaceType.None:
                rb.gravityScale = 3f;
                break;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Skip collision update if in post-wall-jump phase
        if (isJumping) return;

        SurfaceType newSurface = SurfaceType.None;
        Vector2 newWallNormal = lastWallNormal;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            if (Vector2.Dot(normal, Vector2.up) > 0.5f)
            {
                newSurface = SurfaceType.Floor;
                break;
            }
            else if (Vector2.Dot(normal, Vector2.down) > 0.5f)
            {
                newSurface = SurfaceType.Ceiling;
            }
            else if (Mathf.Abs(Vector2.Dot(normal, Vector2.left)) > 0.5f ||
                     Mathf.Abs(Vector2.Dot(normal, Vector2.right)) > 0.5f)
            {
                newSurface = SurfaceType.Wall;
                newWallNormal = normal.normalized;
            }
        }

        if (newSurface != SurfaceType.None && newSurface != currentSurface)
        {
            currentSurface = newSurface;
            if (newSurface == SurfaceType.Wall)
                lastWallNormal = newWallNormal;
            ReportSurfaceChange(newSurface);
        }
        else if (newSurface == SurfaceType.None && currentSurface != SurfaceType.None)
        {
            currentSurface = SurfaceType.None;
            ReportSurfaceChange(currentSurface);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJumping) return;

        SurfaceType newSurface = SurfaceType.None;
        Vector2 newWallNormal = lastWallNormal;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            Debug.Log("[Collision] Contact normal: " + normal);

            if (Vector2.Dot(normal, Vector2.up) > 0.5f)
            {
                newSurface = SurfaceType.Floor;
                break;
            }
            else if (Vector2.Dot(normal, Vector2.down) > 0.5f)
            {
                newSurface = SurfaceType.Ceiling;
            }
            else if (Mathf.Abs(Vector2.Dot(normal, Vector2.left)) > 0.5f ||
                     Mathf.Abs(Vector2.Dot(normal, Vector2.right)) > 0.5f)
            {
                newSurface = SurfaceType.Wall;
                newWallNormal = normal.normalized;
            }
        }

        currentSurface = newSurface;
        if (newSurface == SurfaceType.Wall)
            lastWallNormal = newWallNormal;

        ReportSurfaceChange(newSurface);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (currentSurface)
            {
                case SurfaceType.Floor:
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    Debug.Log("[Jump] Floor jump");
                    break;

                case SurfaceType.Wall:
                    DoWallJump();
                    break;

                case SurfaceType.Ceiling:
                    rb.gravityScale = 3f;
                    currentSurface = SurfaceType.None;
                    Debug.Log("[Jump] Dropped from ceiling");
                    break;
            }
        }
    }

    void DoWallJump()
    {
        Vector2 jumpDir = Vector2.up;
        if (Input.GetKey(KeyCode.D))
            jumpDir += Vector2.right;
        else if (Input.GetKey(KeyCode.A))
            jumpDir += Vector2.left;
        jumpDir = jumpDir.normalized;

        Vector2 normal = GetNormalFromSurface();
        float dot = Vector2.Dot(jumpDir, normal);

        Debug.Log("[Wall Jump] jumpDir = " + jumpDir + " , normal = " + normal + " , dot = " + dot);

        // Prevent jumping directly into the wall
        if (dot > 0.8f)
        {
            Debug.Log("[Wall Jump] Blocked: Jump direction too close to wall");
            return;
        }

        isJumping = true;
        rb.gravityScale = 3f;
        rb.velocity = Vector2.zero;
        rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);
        currentSurface = SurfaceType.None;

        Debug.Log("[Wall Jump] Executed jump in direction: " + jumpDir);

        StartCoroutine(ResetJumpState(jumpDisableCollisionTime));
    }

    IEnumerator ResetJumpState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isJumping = false;
    }

    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (currentSurface == SurfaceType.Ceiling)
        {
            rb.velocity = new Vector2(h * moveSpeed, 0f);
        }
        else if (currentSurface == SurfaceType.Floor)
        {
            rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        }
    }

    void MoveVertical()
    {
        float v = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(0f, v * climbSpeed);
    }

    void ReportSurfaceChange(SurfaceType newSurface)
    {
        string msg = "[Surface] Now attached to: " + newSurface;
        if (newSurface == SurfaceType.Wall)
            msg += " | Wall Normal: " + lastWallNormal;
        Debug.Log(msg);
    }

    Vector2 GetNormalFromSurface()
    {
        switch (currentSurface)
        {
            case SurfaceType.Floor: return Vector2.up;
            case SurfaceType.Ceiling: return Vector2.down;
            case SurfaceType.Wall: return lastWallNormal;
            default: return Vector2.zero;
        }
    }
}
