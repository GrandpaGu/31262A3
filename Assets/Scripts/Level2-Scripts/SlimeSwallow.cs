using UnityEngine;
using System.Collections.Generic;

public enum AbilityType
{
    None,
    SpeedUp   // 目前场景只有 Goat→SpeedUp，后续再扩充
}

public class Ability
{
    public AbilityType type;
    public Ability(AbilityType t) { type = t; }
}

public class SlimeSwallow : MonoBehaviour
{
    [Header("检测设置")]
    public Transform swallowCenter;          // 圆心（拖 Slime 自己或空子物体）
    public float swallowRadius = 1f;         // 半径
    public LayerMask swallowableLayer;       // “Swallowable” Layer

    [Header("时间设置")]
    public float swallowDuration = 0.883f;   // 一按 Ctrl 就持续这么久

    [Header("能力槽")]
    public int maxSlots = 3;
    private List<Ability> slots = new List<Ability>();
    private int currentIndex = 0;

    // —— 内部状态 ——
    float swallowTimer = 0f;     // >0 表示处于吞噬期
    bool swallowedThisPress = false; // 防止一次按键吞多次

    // --------------------------------------------------

    void Update()
    {
        // 1. 监听 Ctrl，启动计时
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            swallowTimer = swallowDuration;
            swallowedThisPress = false;
            Debug.Log($"<color=lime>开始吞噬窗口（{swallowDuration:F3}s）</color>");
        }

        // 2. 计时与检测
        if (swallowTimer > 0f)
        {
            swallowTimer -= Time.deltaTime;

            if (!swallowedThisPress)
            {
                DetectAndSwallow();      // 只尝试一次
            }
        }

        // 3. 能力切换 / 丢弃
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchAbility(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchAbility(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchAbility(2);
        if (Input.GetKeyDown(KeyCode.Q)) DropCurrent();
    }

    // --------------------------------------------------

    void DetectAndSwallow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(swallowCenter.position,
                                                       swallowRadius,
                                                       swallowableLayer);
        if (hits.Length == 0) return;

        // 只吞第一个检测到的目标
        var target = hits[0];
        Debug.Log($"<color=yellow>吞噬对象：{target.name}</color>");

        AbilityType gained = AbilityType.None;
        if (target.name.ToLower().Contains("goat")) gained = AbilityType.SpeedUp;

        if (gained != AbilityType.None) AddAbility(gained);
        Destroy(target.gameObject);

        swallowedThisPress = true; // 本次按键结束
    }

    // --------------------------------------------------
    #region 能力槽管理
    void AddAbility(AbilityType type)
    {
        if (slots.Count >= maxSlots)
        {
            Debug.Log("<color=red>槽位已满，无法获得新能力</color>");
            return;
        }
        slots.Add(new Ability(type));
        currentIndex = slots.Count - 1;   // 自动激活最新
        PrintSlots();
    }

    void SwitchAbility(int idx)
    {
        if (idx < slots.Count)
        {
            currentIndex = idx;
            Debug.Log($"<color=cyan>切换到槽位{idx + 1}: {slots[idx].type}</color>");
        }
    }

    void DropCurrent()
    {
        if (slots.Count == 0) return;
        Debug.Log($"<color=orange>丢弃槽位{currentIndex + 1}: {slots[currentIndex].type}</color>");
        slots.RemoveAt(currentIndex);
        currentIndex = Mathf.Clamp(currentIndex, 0, slots.Count - 1);
        PrintSlots();
    }

    void PrintSlots()
    {
        Debug.Log("—— 当前能力槽 ——");
        for (int i = 0; i < slots.Count; i++)
        {
            string active = i == currentIndex ? " [激活]" : "";
            Debug.Log($"槽{i + 1}: {slots[i].type}{active}");
        }
    }
    #endregion
    // --------------------------------------------------

    // Scene 视图可视化圆圈
    void OnDrawGizmosSelected()
    {
        if (swallowCenter == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(swallowCenter.position, swallowRadius);
    }
}
