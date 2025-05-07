using UnityEngine;
using System.Collections;

public class BatPlatform : MonoBehaviour
{
    public float dropDelay = 0.5f;
    public float dropDistance = 2f;
    public float dropSpeed = 2f;
    public float returnDelay = 2f;
    public float returnSpeed = 1f;

    private Vector3 originalPosition;
    private bool isDropping = false;
    private bool isReturning = false;
    private bool playerOnPlatform = false;

    private Coroutine dropCoroutine;
    private Coroutine returnCoroutine;

    void Start()
    {
        originalPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        playerOnPlatform = true;

        if (isReturning)
        {
            StopCoroutine(returnCoroutine);
            isReturning = false;
        }

        if (!isDropping)
        {
            dropCoroutine = StartCoroutine(DropAfterDelay());
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        playerOnPlatform = false;

        if (!isReturning)
        {
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    IEnumerator DropAfterDelay()
    {
        isDropping = true;
        yield return new WaitForSeconds(dropDelay);

        Vector3 target = originalPosition - new Vector3(0, dropDistance, 0);

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dropSpeed * Time.deltaTime);
            yield return null;
        }

        isDropping = false;
    }

    IEnumerator ReturnAfterDelay()
    {
        isReturning = true;
        yield return new WaitForSeconds(returnDelay);

        if (playerOnPlatform)
        {
            isReturning = false;
            yield break;
        }

        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        isReturning = false;
    }
}
