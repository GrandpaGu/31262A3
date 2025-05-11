using UnityEngine;

public class DestroyOffScreen : MonoBehaviour
{
    public float buffer = 30f;

    void Update()
    {
        float cameraLeft = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - buffer;

        if (transform.position.x < cameraLeft)
        {
            Destroy(gameObject);
        }
    }
}