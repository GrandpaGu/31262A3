using UnityEngine;

public class BatController : MonoBehaviour
{
    [Header("设置目标对象")]
    public Transform slime;            // 史莱姆对象

    [Header("唤醒参数")]
    public float wakeUpDistance = 5f;   // 唤醒的距离

    [Header("巡逻点")]
    public Transform patrolPointA;      // 巡逻点A
    public Transform patrolPointB;      // 巡逻点B
    public float patrolSpeed = 2f;       // 巡逻速度

    private Animator animator;          // 蝙蝠的Animator
    private bool hasWokenUp = false;     // 是否已经醒来
    private bool isPatrolling = false;   // 是否已经开始巡逻
    private Transform targetPoint;       // 当前巡逻的目标点


    void Start()
    {
        animator = GetComponent<Animator>();   // 获取Animator组件
        targetPoint = patrolPointB;             // 初始朝向B点飞
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

    // 唤醒方法
    void WakeUp()
    {
        hasWokenUp = true;
        animator.SetTrigger("WakeUpTrigger");  // 触发动画里的WakeUpTrigger
        Invoke(nameof(StartPatrolling), 1.0f); // 等动画播完开始巡逻，1.0秒根据动画长度调整
    }

    // 开始巡逻
    void StartPatrolling()
    {
        isPatrolling = true;
    }

    // 巡逻逻辑
    void Patrol()
    {
        if (targetPoint == null) return;

        // 先移动蝙蝠
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);

        // 如果非常接近目标点，才考虑切换
        if (Vector2.Distance(transform.position, targetPoint.position) <= 0.05f)
        {
            // 切换目标点
            if (targetPoint == patrolPointA)
            {
                targetPoint = patrolPointB;
            }
            else
            {
                targetPoint = patrolPointA;
            }

            Flip(); // 切换方向
        }
    }


    // 翻转蝙蝠朝向
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
