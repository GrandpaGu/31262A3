using UnityEngine;

public class CameraFollowXY : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;           // 需要跟随的物体（Slime）

    [Header("平滑系数 (0~1，越小越柔和)")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f; // 建议 0.1~0.2 之间

    [Header("固定 Z 轴位置")]
    public float fixedZ = -10f;        // 2D 摄像机常用 -10

    void LateUpdate()
    {
        if (target == null) return;

        // 目标位置（跟随 X 与 Y）
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, fixedZ);

        // 线性插值实现平滑
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);

        transform.position = smoothedPos;
    }
}
