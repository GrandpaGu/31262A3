using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatSpeed = 2f;      // 上下浮动速度
    public float floatHeight = 0.2f;    // 浮动范围

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }
}
