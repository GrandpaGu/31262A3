using UnityEngine;
using System.Collections;

/// <summary>
/// 牛（RedBull）巡逻脚本：往返冲刺 + 顶飞 Slime（Trigger 版）。
/// — 斜向顶飞：可设定角度/力度
/// — 只有当 Slime 位于牛“前方”且牛处于高速状态时才触发，避免被碰瓷
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RedBullPatrol : MonoBehaviour
{
    /* ─────────── 巡逻相关 ─────────── */
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
    public float pointPauseDuration = 0.8f;

    /* ─────────── 顶飞相关 ─────────── */
    [Header("顶飞参数 (Trigger 版)")]
    public string slimeTag = "Player"; // Slime Tag
    public float knockForce = 12f;       // 冲量大小
    [Range(0f, 80f)] public float knockAngle = 30f; // 顶飞角度（0=水平，90=垂直）
    public float speedThreshold = 1.5f;    // 仅当前进速度超过此值才判定为冲锋

    /* ─────────── 内部状态 ─────────── */
    Transform targetPoint;
    Vector3 segmentStart;
    float segmentLength;

    bool isPaused = false;
    Animator animator;

    /* 当前帧速度 (由 MoveAlongCurve 更新) */
    float currentSpeed = 0f;

    void Start()
    {
        if (!patrolPointA || !patrolPointB)
        {
            Debug.LogError("[RedBullPatrol] 巡逻点未设置");
            enabled = false; return;
        }

        targetPoint = patrolPointB;
        segmentStart = transform.position;
        segmentLength = Vector2.Distance(segmentStart, targetPoint.position);

        animator = GetComponent<Animator>();
        FaceTo(targetPoint.position - transform.position);
    }

    void Update()
    {
        if (isPaused) return;
        MoveAlongCurve();
    }

    /* ───────── 行走 ───────── */
    void MoveAlongCurve()
    {
        float remain = Vector2.Distance(transform.position, targetPoint.position);
        if (segmentLength < 0.001f) segmentLength = remain;

        float progress = Mathf.Clamp01(1f - remain / segmentLength);
        float curveVal = Mathf.Max(speedCurve.Evaluate(progress), minCurveValue);
        currentSpeed = baseSpeed * curveVal;

        Vector3 dir = (targetPoint.position - transform.position).normalized;
        transform.position += dir * currentSpeed * Time.deltaTime;

        if (animator) animator.speed = currentSpeed / baseSpeed;

        if (remain <= 0.05f)
            StartCoroutine(PointPauseAndTurn());
    }

    IEnumerator PointPauseAndTurn()
    {
        Pause(true);
        yield return new WaitForSeconds(pointPauseDuration);

        targetPoint = (targetPoint == patrolPointA) ? patrolPointB : patrolPointA;
        segmentStart = transform.position;
        segmentLength = Vector2.Distance(segmentStart, targetPoint.position);
        FaceTo(targetPoint.position - transform.position);

        Pause(false);
    }

    /* ───────── Trigger 撞击 ───────── */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(slimeTag)) return;
        if (currentSpeed < speedThreshold) return;

        float dirX = Mathf.Sign(transform.localScale.x);
        float relativeX = other.transform.position.x - transform.position.x;
        if (relativeX * dirX < 0f) return;   // 背后 → 无效

        KnockbackReceiver recv = other.GetComponent<KnockbackReceiver>();
        if (!recv) return;

        // 计算斜向冲量
        float rad = knockAngle * Mathf.Deg2Rad;
        Vector2 local = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized * knockForce;
        Vector2 world = new Vector2(local.x * dirX, local.y);

        recv.Knock(world, 0.35f);   // 0.35 s 硬直
    }


    /* ───────── 通用 ───────── */
    void Pause(bool p)
    {
        isPaused = p;
        if (animator) animator.speed = p ? 0f : 1f;
    }

    void FaceTo(Vector3 dir)
    {
        if (dir.x == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
        transform.localScale = scale;
    }
}
