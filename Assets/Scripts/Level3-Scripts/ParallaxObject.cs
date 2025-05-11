using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    public float parallaxSpeed = 0.5f;

    void Update()
    {
        transform.localPosition += Vector3.left * parallaxSpeed * Time.deltaTime;
    }
}