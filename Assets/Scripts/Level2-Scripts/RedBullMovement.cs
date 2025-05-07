using UnityEngine;
using System.Collections;

public class RedBullPatrol : MonoBehaviour
{
    [Header("Ñ²Âßµã")]
    public Transform patrolPointA;
    public Transform patrolPointB;

    [Header("Ñ²Âß²ÎÊý")]
    public float patrolSpeed = 2f;
    public float randomPauseChance = 0.1f;
    public float randomPauseDuration = 1f;
    public float pointPauseDuration = 2f;

    private Transform targetPoint;
    private bool isPaused = false;
    private Animator animator;

    void Start()
    {
        targetPoint = patrolPointB;
        animator = GetComponent<Animator>();

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Update()
    {
        if (!isPaused)
        {
            Patrol();
            TryRandomPause();
        }

        UpdateAnimatorSpeed();
    }

    void Patrol()
    {
        if (targetPoint == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            patrolSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPoint.position) <= 0.05f)
        {
            StartCoroutine(PointPauseAndTurn());
        }
    }

    void TryRandomPause()
    {
        if (Random.value < randomPauseChance * Time.deltaTime)
        {
            StartCoroutine(RandomPause());
        }
    }

    IEnumerator RandomPause()
    {
        isPaused = true;
        yield return new WaitForSeconds(randomPauseDuration);
        isPaused = false;
    }

    IEnumerator PointPauseAndTurn()
    {
        isPaused = true;
        yield return new WaitForSeconds(pointPauseDuration);

        targetPoint = (targetPoint == patrolPointA) ? patrolPointB : patrolPointA;
        Flip();

        isPaused = false;
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimatorSpeed()
    {
        if (animator == null) return;

        animator.speed = isPaused ? 0f : 1f;
    }
}