using UnityEngine;

public class CameraFollowXY : MonoBehaviour
{
    [Header("跟随目眮E")]
    public Transform target;           // 需要跟随的蝸E澹⊿lime）

    [Header("平滑系数 (0~1，越小越柔和)")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f; // 建襾E0.1~0.2 之紒E

    [Header("固定 Z 轴位置")]
    public float fixedZ = -10f;        // 2D 摄像机常用 -10

    void LateUpdate()
    {
        if (target == null) return;

        // 目眮E恢茫ǜ丒X 觼EY）
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, fixedZ);

        // 线性插值实现平滑
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);

        transform.position = smoothedPos;
    }
}
