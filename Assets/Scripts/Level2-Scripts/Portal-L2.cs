using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    /*──────────────── 公共参数 ────────────────*/
    [Header("与之配对的另一端 Portal")]
    public Portal pair;

    [Header("淡入淡出时长 (秒)")]
    public float fadeDuration = 0.25f;

    [Header("传送移动速度 (单位/秒)")]
    public float moveSpeed = 8f;              // ← 用速度而不是固定时长

    [Header("移动期间是否关闭碰撞")]
    public bool disableCollision = true;

    /*──────────────── 私有状态 ────────────────*/
    bool playerInside;
    GameObject player;
    Level2SlimeController playerCtrl;
    Rigidbody2D playerRb;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInside = true;
            player = col.gameObject;
            playerCtrl = player.GetComponent<Level2SlimeController>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInside = false;
            player = null;      // 字段可清空
        }
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TeleportSequence());
        }
    }

    IEnumerator TeleportSequence()
    {
        /*───── 0. 缓存引用 ─────*/
        GameObject localPlayer = player;
        Level2SlimeController localCtrl = playerCtrl;
        Rigidbody2D localRb = playerRb;

        if (localPlayer == null || pair == null)
        {
            Debug.LogError("[Portal] 玩家或 Pair 为空，终止传送！");
            yield break;
        }

        Transform targetPoint = pair.transform.Find("TeleportPoint");
        if (targetPoint == null)
        {
            Debug.LogError($"[Portal] {pair.name} 下未找到 TeleportPoint！");
            yield break;
        }

        /*───── 1. 禁用移动控制 ─────*/
        if (localCtrl) localCtrl.enabled = false;

        /*───── 2. 渐隐 ─────*/
        yield return Fade(localPlayer, 1f, 0f);

        /*───── 3. 关闭碰撞 + 暂停物理 ─────*/
        Collider2D[] cachedCols = localPlayer.GetComponentsInChildren<Collider2D>(true);
        if (disableCollision)
        {
            foreach (var c in cachedCols) c.enabled = false;
        }

        bool rbHadGravity = false;
        float savedGravity = 0f;
        if (localRb)
        {
            rbHadGravity = true;
            savedGravity = localRb.gravityScale;
            localRb.gravityScale = 0f;        // 防止掉落
            localRb.velocity = Vector2.zero;
            localRb.isKinematic = true;      // 暂停物理响应
        }

        /*───── 4. 移动（速度控制） ─────*/
        Vector3 endPos = targetPoint.position;
        while ((endPos - localPlayer.transform.position).sqrMagnitude > 0.0001f)
        {
            localPlayer.transform.position =
                Vector3.MoveTowards(localPlayer.transform.position,
                                    endPos,
                                    moveSpeed * Time.deltaTime);
            yield return null;
        }

        /*───── 5. 开启碰撞 ─────*/
        if (disableCollision)
        {
            foreach (var c in cachedCols) if (c) c.enabled = true;
        }

        /*───── 6. 渐显 ─────*/
        yield return Fade(localPlayer, 0f, 1f);

        /*───── 7. 恢复物理与移动 ─────*/
        if (rbHadGravity && localRb)
        {
            localRb.isKinematic = false;
            localRb.gravityScale = savedGravity;
        }
        if (localCtrl) localCtrl.enabled = true;
    }

    /*──────────────── 渐隐/渐显 ────────────────*/
    IEnumerator Fade(GameObject obj, float fromA, float toA)
    {
        if (obj == null) yield break;

        SpriteRenderer[] srs = obj.GetComponentsInChildren<SpriteRenderer>();
        float t = 0f;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(fromA, toA, t / fadeDuration);
            foreach (var sr in srs)
            {
                if (sr)
                {
                    var c = sr.color;
                    c.a = a;
                    sr.color = c;
                }
            }
            t += Time.deltaTime;
            yield return null;
        }

        foreach (var sr in srs)
        {
            if (sr)
            {
                var c = sr.color;
                c.a = toA;
                sr.color = c;
            }
        }
    }
}
