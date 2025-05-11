using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("渐隐/渐显时间 (秒)")]
    public float fadeDuration = 0.25f;

    [Header("重生移动速度 (单位/秒)")]
    public float moveSpeed = 8f;

    [Header("移动期间是否关闭碰撞")]
    public bool disableCollision = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartRespawn(GameObject player)
    {
        Transform checkpoint = GameManagerL2.Instance.GetCurrentCheckpoint();
        if (checkpoint == null)
        {
            Debug.LogWarning("[RespawnManager] 没有有效检查点，默认原地重生！");
            checkpoint = player.transform;
        }

        StartCoroutine(RespawnSequence(player, checkpoint));
    }

    IEnumerator RespawnSequence(GameObject player, Transform checkpoint)
    {
        if (player == null || checkpoint == null)
        {
            Debug.LogError("[Respawn] 玩家或检查点为空！");
            yield break;
        }

        var playerCtrl = player.GetComponent<Level2SlimeController>();
        var playerRb = player.GetComponent<Rigidbody2D>();

        // 1. 禁用输入
        if (playerCtrl) playerCtrl.enabled = false;

        // 2. 渐隐
        yield return Fade(player, 1f, 0f);

        // 3. 关闭碰撞和物理
        Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>(true);
        if (disableCollision)
        {
            foreach (var c in colliders) c.enabled = false;
        }

        bool hadGravity = false;
        float savedGravity = 0f;
        if (playerRb)
        {
            hadGravity = true;
            savedGravity = playerRb.gravityScale;
            playerRb.gravityScale = 0f;
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
        }

        // 4. 移动到检查点
        Vector3 targetPos = checkpoint.position;
        while ((targetPos - player.transform.position).sqrMagnitude > 0.0001f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 5. 开启碰撞
        if (disableCollision)
        {
            foreach (var c in colliders) if (c) c.enabled = true;
        }

        // 6. 渐显
        yield return Fade(player, 0f, 1f);

        // 7. 恢复物理和输入
        if (hadGravity && playerRb)
        {
            playerRb.isKinematic = false;
            playerRb.gravityScale = savedGravity;
        }
        if (playerCtrl) playerCtrl.enabled = true;
    }

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
                if (sr != null)
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
            if (sr != null)
            {
                var c = sr.color;
                c.a = toA;
                sr.color = c;
            }
        }
    }
}
