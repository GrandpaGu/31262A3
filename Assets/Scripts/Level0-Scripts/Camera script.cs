using UnityEngine;

public class CameraFollowFlexible : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("跟随设置")]
    public bool followX = false;           // 是否跟随X轴（默认否）
    public float smoothSpeed = 0.125f;     // 跟随平滑度
    public float fixedX = 0f;              // 固定X坐标
    public float fixedZ = -10f;            // 摄像机Z轴（通常为-10）

    void LateUpdate()
    {
        if (target == null) return;

        float x = followX ? target.position.x : fixedX;
        float y = target.position.y;
        Vector3 desiredPosition = new Vector3(x, y, fixedZ);
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothed;
    }
}