using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class EnvironmentDetector : MonoBehaviour
{
    [Header("水体 Tilemap")]
    public Tilemap waterTilemap;
    public float drownTime = 1f;

    [Header("岩浆 Tilemap")]
    public Tilemap lavaTilemap;
    public float lavaDeathDelay = 0.5f;

    Collider2D col;

    bool inWater, fullySubmerged;
    bool prevInWater, prevFully;
    float submergedTimer = 0f;

    bool inLava = false;
    float lavaTimer = 0f;

    void Awake() => col = GetComponent<Collider2D>();

    void Update()
    {
        HandleWaterDetection();
        HandleLavaDetection();
    }

    /*──────────────── 水体检测 ────────────────*/
    void HandleWaterDetection()
    {
        if (waterTilemap == null) return;

        (inWater, fullySubmerged) = SampleEnvironment(waterTilemap);

        bool canBreathe = false;
        var slime = GetComponent<Level2SlimeController>();
        if (slime != null) canBreathe = slime.hasSwimAbility;   // ★ 是否拥有 Swim

        if (fullySubmerged && !canBreathe)
        {
            submergedTimer += Time.deltaTime;
            if (submergedTimer >= drownTime)
            {
                Debug.Log("[EnvDetector] 玩家已淹死！");
                submergedTimer = 0f;

                // 直接调用死亡逻辑
                GetComponent<Level2SlimeController>()?.Die();
                // TODO：调用死亡逻辑
                // GetComponent<Level2SlimeController>()?.Die();
            }
        }
        else
        {
            submergedTimer = 0f;
        }

        // 调试日志（可删）
        if (inWater && !prevInWater) Debug.Log("玩家部分进入水体");
        if (fullySubmerged && !prevFully) Debug.Log("玩家完全浸入水体");
        if (!inWater && prevInWater) Debug.Log("玩家脱离水体");

        prevInWater = inWater;
        prevFully = fullySubmerged;
    }

    /*──────────────── 岩浆检测 ────────────────*/
    void HandleLavaDetection()
    {
        if (lavaTilemap == null) return;

        (bool any, _) = SampleEnvironment(lavaTilemap);

        if (any)
        {
            if (!inLava) Debug.Log("玩家接触岩浆！");
            inLava = true;
            lavaTimer += Time.deltaTime;

            if (lavaTimer >= lavaDeathDelay)
            {
                var slime = GetComponent<Level2SlimeController>();
                if (slime != null)
                {
                    if (slime.immuneToLava)
                    {
                        lavaTimer = 0f;
                        Debug.Log("[EnvDetector] 玩家已免疫岩浆，不受伤！");
                        return;
                    }
                    else
                    {
                        Debug.Log("[EnvDetector] 玩家被岩浆灼烧致死！");
                        lavaTimer = 0f;
                        slime.Die();
                    }
                }
            }
        }
        else
        {
            if (inLava) Debug.Log("玩家离开岩浆！");
            inLava = false;
            lavaTimer = 0f;
        }
    }

    /*──────────────── Tilemap 通用采样 ────────────────*/
    (bool any, bool all) SampleEnvironment(Tilemap map)
    {
        Bounds b = col.bounds;
        Vector3[] points = new Vector3[]
        {
            new Vector3(b.min.x, b.min.y, 0),
            new Vector3(b.min.x, b.max.y, 0),
            new Vector3(b.max.x, b.min.y, 0),
            new Vector3(b.max.x, b.max.y, 0),
            b.center
        };

        bool any = false, all = true;
        foreach (var p in points)
        {
            bool inside = map.HasTile(map.WorldToCell(p));
            any |= inside;
            all &= inside;
        }
        return (any, all);
    }

    /*──────────────── 提供外部查询 ────────────────*/
    public bool IsInWater() => inWater;
    public bool IsFullySubmerged() => fullySubmerged;
    public bool IsInLava() => inLava;
}
