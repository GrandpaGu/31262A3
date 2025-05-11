using UnityEngine;
using System.Collections;

/// <summary>
/// WolfPatrol - 参考牛的曲线冲刺移动方式（带加速/减速效果）+ 顶飞逻辑。
/// 狼在两点之间持续左右巡逻，移动速度基于曲线变化。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WolfPatrol : MonoBehaviour
{
    [Header("巡逻点 (必填)")]
    public Transform patrolPointA;
    public Transform patrolPointB;

    [Header("速度曲线 (X:0-1 进度, Y:速度比例)")]
    public AnimationCurve speedCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(0.15f, 1f),
        new Keyframe(0.85f, 1f),
        new Keyframe(1f, 0f, -2f, 0f));

    [Tooltip("曲线值过低时的最小比例，防止起步停滞")]
    public float minCurveValue = 0.05f;

    [Header("基础速度 / 端点停顿")]
    public float baseSpeed = 2f;
    public float pointPauseDuration = 1f;

    [Header("动画参数")]
    public Animator animator;
    public string moveSpeedParam = "Speed";

    [Header("顶飞参数")]
    public string slimeTag = "Player";
    public float knockForce = 10f;
    [Range(0f, 80f)] public float knockAngle = 30f;
    public float speedThreshold = 1.5f;

    Transform targetPoint;
    Vector3 segmentStart;
    float segmentLength;

    bool isPaused = false;
    float currentSpeed = 0f;

    void Start()
    {
        if (!patrolPointA || !patrolPointB)
        {
            Debug.LogError("[WolfPatrol] 巡逻点未设置");
            enabled = false; return;
        }

        targetPoint = patrolPointB;
        segmentStart = transform.position;
        segmentLength = Vector2.Distance(segmentStart, targetPoint.position);

        FaceTo(targetPoint.position - transform.position);
    }

    void Update()
    {
        if (!isPaused)
        {
            MoveAlongCurve();
        }

        if (animator)
        {
            animator.SetFloat(moveSpeedParam, Mathf.Abs(currentSpeed));
        }
    }

    void MoveAlongCurve()
    {
        float remain = Vector2.Distance(transform.position, targetPoint.position);
        if (segmentLength < 0.001f) segmentLength = remain;

        float progress = Mathf.Clamp01(1f - remain / segmentLength);
        float curveVal = Mathf.Max(speedCurve.Evaluate(progress), minCurveValue);
        currentSpeed = baseSpeed * curveVal;

        Vector3 dir = (targetPoint.position - transform.position).normalized;
        transform.position += dir * currentSpeed * Time.deltaTime;

        if (remain <= 0.1f)
            StartCoroutine(PointPauseAndTurn());

        UpdateFacingDirection(dir.x);
    }

    IEnumerator PointPauseAndTurn()
    {
        isPaused = true;
        yield return new WaitForSeconds(pointPauseDuration);

        targetPoint = (targetPoint == patrolPointA) ? patrolPointB : patrolPointA;
        segmentStart = transform.position;
        segmentLength = Vector2.Distance(segmentStart, targetPoint.position);

        FaceTo(targetPoint.position - transform.position);

        isPaused = false;
    }

    void UpdateFacingDirection(float dirX)
    {
        if (dirX == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x) * Mathf.Sign(dirX); // 保持视觉不变
        transform.localScale = scale;
    }

    void FaceTo(Vector3 dir)
    {
        if (dir.x == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x) * Mathf.Sign(dir.x); // 保持视觉不变
        transform.localScale = scale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(slimeTag)) return;
        if (currentSpeed < speedThreshold) return;

        // 修正判定方向：直接使用狼的移动方向而非图像翻转方向
        float dirX = Mathf.Sign(targetPoint.position.x - transform.position.x);
        float relativeX = other.transform.position.x - transform.position.x;
        if (relativeX * dirX < 0f) return; // 不是前方，忽略

        KnockbackReceiver recv = other.GetComponent<KnockbackReceiver>();
        if (!recv) return;

        float rad = knockAngle * Mathf.Deg2Rad;
        Vector2 local = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized * knockForce;
        Vector2 world = new Vector2(local.x * dirX, local.y);

        recv.Knock(world, 0.35f);
    }
}
