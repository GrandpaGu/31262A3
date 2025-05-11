using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUpToggle : MonoBehaviour
{
    public float floatSpeed = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isFloating = false;
    private SlimeController slimeMovement;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeMovement = GetComponent<SlimeController>();

        if (slimeMovement != null)
        {
            slimeMovement.enabled = true;
        }
        else
        {
            Debug.LogWarning("SlimeMovementController is not attached to this GameObject!");
        }
    }

    void Update()
    {
        CheckIfGrounded();

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
         
            if (!isFloating && isGrounded)
            {
                isFloating = true;
            }
            else if (isFloating)
            {
                isFloating = false;
            }

            if (slimeMovement != null)
            {
                slimeMovement.enabled = !isFloating;
            }
        }
    }

    void FixedUpdate()
    {
        if (isFloating)
        {
            rb.velocity = new Vector2(0f, floatSpeed);
        }
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
