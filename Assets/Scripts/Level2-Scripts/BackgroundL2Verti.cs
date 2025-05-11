using UnityEngine;

public class BackgroundVerticalMapping : MonoBehaviour
{
    public Transform slime;  // 拖入 Slime 对象

    [Header("世界坐标范围")]
    public float worldMinY = 0f;    // Slime 可以到的最低 Y 坐标（地面）
    public float worldMaxY = 100f;  // Slime 可以到的最高 Y 坐标（山顶）

    [Header("背景移动范围")]
    public float bgMinY = 0f;       // 背景最低时的 Y 坐标
    public float bgMaxY = 50f;      // 背景最高时的 Y 坐标

    [Header("视差控制")]
    public float horizontalParallax = 0.1f;  // X 方向视差，越小越“远景感”

    Vector3 startPos;

    void Start()
    {
        if (!slime) Debug.LogError("Slime 未赋值！");
        startPos = transform.position;
    }

    void LateUpdate()
    {
        if (!slime) return;

        // 横向视差（左右平移）
        float offsetX = slime.position.x * horizontalParallax;

        // 纵向比例映射
        float t = Mathf.InverseLerp(worldMinY, worldMaxY, slime.position.y);
        float offsetY = Mathf.Lerp(bgMinY, bgMaxY, t);

        transform.position = new Vector3(startPos.x + offsetX, offsetY, startPos.z);
    }
}
