using UnityEngine;

/// 让对象沿 Catmull‑Rom 样条循环运动 + 自动朝向调整
public class BirdCurveFly : MonoBehaviour
{
    [Tooltip("至少 4 个控制点；若想闭合请把首尾额外复制 2 个点")]
    public Transform[] points;
    public float speed = 1f;

    float t = 0f;
    int seg = 0;
    Vector3 lastPos;   // 上一帧位置

    void Start()
    {
        if (points.Length >= 4)
        {
            transform.position = CatmullRom(0f, points[0].position, points[1].position, points[2].position, points[3].position);
            lastPos = transform.position;
        }
    }

    void Update()
    {
        if (points.Length < 4) return;

        t += Time.deltaTime * speed;
        if (t > 1f)
        {
            t = 0f;
            seg++;
            if (seg > points.Length - 4) seg = 0;
        }

        Vector3 newPos = CatmullRom(t,
            points[seg].position,
            points[seg + 1].position,
            points[seg + 2].position,
            points[seg + 3].position);

        // ========== 朝向判断 ==========
        Vector3 dir = newPos - lastPos;
        if (Mathf.Abs(dir.x) > 0.001f) // 避免除以0
        {
            float facing = Mathf.Sign(dir.x);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facing; // 正x朝右，负x朝左
            transform.localScale = scale;
        }

        transform.position = newPos;
        lastPos = newPos;
    }

    static Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (points == null || points.Length < 4) return;

        Gizmos.color = Color.yellow;
        const int stepsPerSegment = 20;

        for (int seg = 0; seg <= points.Length - 4; seg++)
        {
            Vector3 prev = points[seg + 1].position;
            for (int i = 1; i <= stepsPerSegment; i++)
            {
                float t = i / (float)stepsPerSegment;
                Vector3 cur = CatmullRom(t,
                    points[seg].position,
                    points[seg + 1].position,
                    points[seg + 2].position,
                    points[seg + 3].position);

                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }
        }
    }
#endif
}
