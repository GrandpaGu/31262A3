using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstepSound2D : MonoBehaviour
{
    public float movementThreshold = 0.1f; 
    public int groundLayer = 3;       
    public float groundCheckDistance = 0.1f; 
    public AudioClip footstepClip;        
    public float footstepInterval = 0.5f;  

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private float stepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        audioSource.volume = 0.05f;
    }

    void Update()
    {

        bool isGrounded = CheckGrounded();
        bool isMoving = rb.velocity.magnitude > movementThreshold;


        stepTimer += Time.deltaTime;


        if (isGrounded && isMoving && stepTimer >= footstepInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }


    bool CheckGrounded()
    {

        Vector2 groundCheckOrigin = (Vector2)transform.position - new Vector2(0, GetComponent<Collider2D>().bounds.extents.y);


        return Physics2D.Raycast(groundCheckOrigin, Vector2.down, groundCheckDistance, 1 << groundLayer); 
    }

    void PlayFootstep()
    {
        if (footstepClip != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(footstepClip);
        }
    }
}

