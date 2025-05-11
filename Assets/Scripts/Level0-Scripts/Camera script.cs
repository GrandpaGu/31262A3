using UnityEngine;

public class CameraFollowFlexible : MonoBehaviour
{
    [Header("¸úËæÄ¿±E")]
    public Transform target;

    [Header("¸úËæÉèÖÃ")]
    public bool followX = false;           // ÊÇ·ñ¸úËæXÖá£¨Ä¬ÈÏ·ñ£©
    public float smoothSpeed = 0.125f;     // ¸úËæÆ½»¬¶È
    public float fixedX = 0f;              // ¹Ì¶¨X×ø±E
    public float fixedZ = -10f;            // ÉãÏñ»‡„Öá£¨Í¨³£Îª-10£©

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