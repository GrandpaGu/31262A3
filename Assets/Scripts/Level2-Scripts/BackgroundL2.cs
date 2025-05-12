using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxTiler : MonoBehaviour
{
    [Header("References")]
    public Transform cam;  // 拖 Main Camera

    [Header("Parallax Settings")]
    public float parallaxMultiplierX = 0.1f;    // 横向视差比例
    public int redundancy = 2;                  // 冗余平铺数量（左右各几个）

    private SpriteRenderer srcSR;
    private float spriteW;
    private float camHalfW;
    private Transform[] tiles;
    private Vector3 startPos;

    void Awake()
    {
        if (!cam) cam = Camera.main.transform;

        srcSR = GetComponent<SpriteRenderer>();
        spriteW = srcSR.sprite.rect.width / srcSR.sprite.pixelsPerUnit * transform.localScale.x;
        camHalfW = Camera.main.orthographicSize * Camera.main.aspect;
        startPos = transform.position;

        int totalTiles = 1 + redundancy * 2;
        tiles = new Transform[totalTiles];

        // 创建冗余平铺对象
        for (int i = -redundancy; i <= redundancy; i++)
        {
            int index = i + redundancy;
            if (i == 0)
            {
                tiles[index] = transform; // 中心块直接用自己
            }
            else
            {
                tiles[index] = MakeClone($"BG_Tile_{i}", i * spriteW, transform.parent);
            }
        }
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
        if (!cam) return;

        float parallaxX = cam.position.x * parallaxMultiplierX;

        for (int i = 0; i < tiles.Length; i++)
        {
            float tileOffsetX = (i - redundancy) * spriteW;
            tiles[i].position = new Vector3(startPos.x + tileOffsetX + parallaxX, tiles[i].position.y, tiles[i].position.z);
        }

        // 摄像机边界检测，调整 tile 顺序
        float camLeftEdge = cam.position.x - camHalfW;
        float camRightEdge = cam.position.x + camHalfW;

        // 如果最右的 tile 左边已经进入摄像机左边界，则循环左移
        if (camLeftEdge > tiles[redundancy].position.x + spriteW * 0.5f)
        {
            ShiftRight();
        }
        // 如果最左的 tile 右边已经进入摄像机右边界，则循环右移
        else if (camRightEdge < tiles[redundancy].position.x - spriteW * 0.5f)
        {
            ShiftLeft();
        }
    }

    void ShiftRight()
    {
        Transform leftMost = tiles[0];
        for (int i = 0; i < tiles.Length - 1; i++)
        {
            tiles[i] = tiles[i + 1];
        }
        tiles[tiles.Length - 1] = leftMost;
        leftMost.position = tiles[tiles.Length - 2].position + Vector3.right * spriteW;
    }

    void ShiftLeft()
    {
        Transform rightMost = tiles[tiles.Length - 1];
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            tiles[i] = tiles[i - 1];
        }
        tiles[0] = rightMost;
        rightMost.position = tiles[1].position - Vector3.right * spriteW;
    }
}
