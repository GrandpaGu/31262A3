using UnityEngine;
using System.Collections.Generic;

public enum AbilityType
{
    None,
    SpeedUp
}

public class Ability
{
    public AbilityType type;
    public Ability(AbilityType t) { type = t; }
}

public class SlimeSwallow : MonoBehaviour
{
    [Header("检测设置")]
    public Transform swallowCenter;
    public float swallowRadius = 1f;
    public LayerMask swallowableLayer;

    [Header("吞噬时间")]
    public float swallowDuration = 0.883f;
    public float swallowMoveSpeed = 2f;    // 【新增】吞噬时往前位移速度（米/秒）
    private float swallowMoveTimer = 0f;

    [Header("Yue动画设置")]
    public string yueTriggerName = "Yue";
    public float yueDuration = 0.8f;
    public float yueMoveSpeed = 2f;         // 【新增】Yue时往后位移速度
    private float yueMoveTimer = 0f;

    [Header("能力槽")]
    public int maxSlots = 3;
    private List<Ability> slots = new();
    private int currentIndex = 0;

    /* 内部状态 */
    float swallowTimer = 0f;
    bool swallowedThisPress = false;
    float yueTimer = 0f;

    Animator anim;
    Rigidbody2D rb;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        /* === Ctrl 吞噬输入 === */
        if (yueTimer <= 0f)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                swallowTimer = swallowDuration;
                swallowMoveTimer = swallowDuration;  // ←开始位移
                swallowedThisPress = false;
                Debug.Log($"<color=lime>开始吞噬窗口 ({swallowDuration:F3}s)</color>");
            }
        }

        if (swallowTimer > 0f)
        {
            swallowTimer -= Time.deltaTime;
            if (!swallowedThisPress) DetectAndSwallow();
        }

        /* === R键触发 Yue 动画 + 删除能力 + 后退 === */
        if (Input.GetKeyDown(KeyCode.R) && swallowTimer <= 0f && yueTimer <= 0f)
        {
            PlayYueAnimationAndDropSkill();
        }

        if (yueTimer > 0f) yueTimer -= Time.deltaTime;

        /* === 槽位快捷键 === */
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchAbility(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchAbility(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchAbility(2);
    }

    void FixedUpdate()
    {
        // 固定帧移动，处理平移

        if (swallowMoveTimer > 0f)
        {
            swallowMoveTimer -= Time.fixedDeltaTime;
            float dir = transform.localScale.x > 0 ? 1f : -1f;  // 朝面朝方向前进
            rb.position += new Vector2(dir * swallowMoveSpeed * Time.fixedDeltaTime, 0f);
        }

        if (yueMoveTimer > 0f)
        {
            yueMoveTimer -= Time.fixedDeltaTime;
            float dir = transform.localScale.x > 0 ? -1f : 1f;  // 朝反方向后退
            rb.position += new Vector2(dir * yueMoveSpeed * Time.fixedDeltaTime, 0f);
        }
    }

    // ========== 播放 Yue 动画并丢弃当前技能 ==========
    void PlayYueAnimationAndDropSkill()
    {
        if (anim != null)
        {
            anim.ResetTrigger(yueTriggerName);
            anim.SetTrigger(yueTriggerName);
            Debug.Log($"<color=purple>播放动画 Trigger: {yueTriggerName}</color>");
        }

        yueTimer = yueDuration;
        yueMoveTimer = yueDuration;  // ←开始后退位移

        if (slots.Count > 0)
        {
            Debug.Log($"<color=orange>丢弃槽位{currentIndex + 1}: {slots[currentIndex].type}</color>");
            slots.RemoveAt(currentIndex);
            currentIndex = Mathf.Clamp(currentIndex, 0, slots.Count - 1);
            PrintSlots();
        }
        else
        {
            Debug.Log("<color=grey>当前无技能，只播放Yue动画</color>");
        }
    }

    // ========== 吞噬检测 ==========
    void DetectAndSwallow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(swallowCenter.position,
                                                       swallowRadius,
                                                       swallowableLayer);
        if (hits.Length == 0) return;

        var target = hits[0];
        Debug.Log($"<color=yellow>吞噬对象：{target.name}</color>");

        if (target.name.ToLower().Contains("goat"))
            AddAbility(AbilityType.SpeedUp);

        Destroy(target.gameObject);

        swallowedThisPress = true;
    }

    // ========== 能力槽管理 ==========
    void AddAbility(AbilityType type)
    {
        if (slots.Count >= maxSlots)
        {
            Debug.Log("<color=red>槽位已满，无法获得新能力</color>");
            return;
        }
        slots.Add(new Ability(type));
        currentIndex = slots.Count - 1;
        PrintSlots();
    }

    void SwitchAbility(int idx)
    {
        currentIndex = idx; // 无论如何都切换索引

        if (idx < slots.Count)
        {
            Debug.Log($"<color=cyan>切换到槽位{idx + 1}: {slots[idx].type}</color>");
        }
        else
        {
            Debug.Log($"<color=grey>切换到槽位{idx + 1}: 当前槽位没有技能</color>");
        }
    }

    void PrintSlots()
    {
        Debug.Log("—— 当前能力槽 ——");
        for (int i = 0; i < slots.Count; i++)
        {
            string active = (i == currentIndex) ? " [激活]" : "";
            Debug.Log($"槽{i + 1}: {slots[i].type}{active}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (swallowCenter == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(swallowCenter.position, swallowRadius);
    }
}