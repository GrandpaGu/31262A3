using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class CameraFollowYOnly : MonoBehaviour
{
    public Transform target;        // The object (like your player) to follow
    public float smoothSpeed = 0.125f;
    public float fixedX = 0f;       // Fixed X position for the camera
    public float fixedZ = -10f;     // Fixed Z position for the camera (usually -10 for 2D)

    void LateUpdate()
    {
        if (target != null)
        {
            // Only follow the target's Y position
            Vector3 desiredPosition = new Vector3(fixedX, target.position.y, fixedZ);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}