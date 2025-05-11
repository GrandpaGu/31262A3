using UnityEngine;

public class BatController : MonoBehaviour
{
    [Header("ÉèÖÃÄ¿±EÔÏE")]
    public Transform slime;            // Ê·À³Ä·¶ÔÏE

    [Header("»½ĞÑ²ÎÊı")]
    public float wakeUpDistance = 5f;   // »½ĞÑµÄ¾àÀE

    [Header("Ñ²ÂßµE")]
    public Transform patrolPointA;      // Ñ²ÂßµãA
    public Transform patrolPointB;      // Ñ²ÂßµãB
    public float patrolSpeed = 2f;       // Ñ²ÂßËÙ¶È

    private Animator animator;          // òùòğµÄAnimator
    private bool hasWokenUp = false;     // ÊÇ·ñÒÑ¾­ĞÑÀ´
    private bool isPatrolling = false;   // ÊÇ·ñÒÑ¾­¿ªÊ¼Ñ²Âß
    private Transform targetPoint;       // µ±Ç°Ñ²ÂßµÄÄ¿±EE


    void Start()
    {
        animator = GetComponent<Animator>();   // »ñÈ¡Animator×é¼ş
        targetPoint = patrolPointB;             // ³õÊ¼³¯ÏòBµã·É
    }

    void Update()
    {
        if (!hasWokenUp)
        {
            float distance = Vector2.Distance(transform.position, slime.position);
            if (distance <= wakeUpDistance)
            {
                WakeUp();
            }
        }
        else if (isPatrolling)
        {
            Patrol();
        }
    }

    // »½ĞÑ·½·¨
    void WakeUp()
    {
        hasWokenUp = true;
        animator.SetTrigger("WakeUpTrigger");  // ´¥·¢¶¯»­ÀEÄWakeUpTrigger
        Invoke(nameof(StartPatrolling), 1.0f); // µÈ¶¯»­²¥ÍEªÊ¼Ñ²Âß£¬1.0ÃEù¾İ¶¯»­³¤¶Èµ÷ÕE
    }

    // ¿ªÊ¼Ñ²Âß
    void StartPatrolling()
    {
        isPatrolling = true;
    }

    // Ñ²ÂßÂß¼­
    void Patrol()
    {
        if (targetPoint == null) return;

        // ÏÈÒÆ¶¯òùòğ
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);

        // Èç¹û·Ç³£½Ó½E¿±Eã£¬²Å¿¼ÂÇÇĞ»»
        if (Vector2.Distance(transform.position, targetPoint.position) <= 0.05f)
        {
            // ÇĞ»»Ä¿±EE
            if (targetPoint == patrolPointA)
            {
                targetPoint = patrolPointB;
            }
            else
            {
                targetPoint = patrolPointA;
            }

            Flip(); // ÇĞ»»·½ÏE
        }
    }


    // ·­×ªòùòğ³¯ÏE
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
