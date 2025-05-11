using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxTiler : MonoBehaviour
{
    [Header("References")]
    public Transform cam;  // 拖 MainCamera

    [Header("Parallax Settings")]
    public float parallaxMultiplierX = 0.1f;  // 横向视差比例

    SpriteRenderer srcSR;
    float spriteW;
    float camHalfW;
    Transform left, centre, right;
    Vector3 startPos;

    void Awake()
    {
        if (!cam) cam = Camera.main.transform;

        srcSR = GetComponent<SpriteRenderer>();
        spriteW = srcSR.sprite.rect.width / srcSR.sprite.pixelsPerUnit * transform.localScale.x;
        camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
        startPos = transform.position;

        left = MakeClone("BG_Left", -spriteW, transform.parent);
        centre = transform;
        right = MakeClone("BG_Right", spriteW, transform.parent);
    }

    Transform MakeClone(string name, float offsetX, Transform parent)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.position = transform.position + new Vector3(offsetX, 0, 0);
        g.transform.localScale = transform.localScale;

        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = srcSR.sprite;
        sr.sortingLayerID = srcSR.sortingLayerID;
        sr.sortingOrder = srcSR.sortingOrder;

        return g.transform;
    }

    void LateUpdate()
    {
        /* 横向视差计算，仅影响 X 方向 */
        float parallaxX = cam.position.x * parallaxMultiplierX;

        left.position = new Vector3(startPos.x - spriteW + parallaxX, left.position.y, left.position.z);
        centre.position = new Vector3(startPos.x + parallaxX, centre.position.y, centre.position.z);
        right.position = new Vector3(startPos.x + spriteW + parallaxX, right.position.y, right.position.z);

        /* 平铺循环逻辑 */
        float camX = cam.position.x;

        if (camX > centre.position.x + spriteW * 0.5f)
            ShiftRight();
        else if (camX < centre.position.x - spriteW * 0.5f)
            ShiftLeft();
    }

    void ShiftRight()
    {
        left.position = right.position + Vector3.right * spriteW;

        Transform temp = left;
        left = centre;
        centre = right;
        right = temp;
    }

    void ShiftLeft()
    {
        right.position = left.position - Vector3.right * spriteW;

        Transform temp = right;
        right = centre;
        centre = left;
        left = temp;
    }
}
