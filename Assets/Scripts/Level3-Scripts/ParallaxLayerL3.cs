using UnityEngine;

public class ParallaxLayerL3 : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxFactor = 0.5f;

    private Vector3 startPosition;
    private Vector3 startCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        startPosition = transform.position;
        startCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - startCameraPosition;
        transform.position = startPosition + delta * parallaxFactor;
    }
}