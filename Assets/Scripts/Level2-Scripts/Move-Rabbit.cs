using UnityEngine;
using UnityEngine.UIElements;

public class BunnyPatrol : MonoBehaviour
{
    [Header("跳跃设置")]
    public float jumpForceX = 3f;
    public float jumpForceY = 5f;
    public float jumpInterval = 2f;

    [Header("巡逻边界")]
    public Transform leftBound;
    public Transform rightBound;

    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("动画（可选）")]
    public Animator animator;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool movingRight = true;
    private float jumpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        jumpTimer = jumpInterval;
    }

    void Update()
    {
        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (animator) animator.SetBool("IsGrounded", isGrounded);

        // 跳跃冷却
        if (isGrounded)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f)
            {
                Jump();
                jumpTimer = jumpInterval;
            }
        }

        // 更新朝向
        UpdateFacingDirection();
    }

    void Jump()
    {
        // 边界检测
        if (movingRight && transform.position.x > rightBound.position.x)
            movingRight = false;
        else if (!movingRight && transform.position.x < leftBound.position.x)
            movingRight = true;

        // 应用跳跃力
        float direction = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * jumpForceX, jumpForceY);

        if (animator) animator.SetTrigger("Jump");
    }

    void UpdateFacingDirection()
    {
        Vector3 scale = transform.localScale;
        if (movingRight)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // 可视化地面检测点
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
