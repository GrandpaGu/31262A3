using UnityEngine;
using System.Collections.Generic;

public class Ability
{
    public AbilityType type;
    public Ability(AbilityType t) { type = t; }
}

public class SlimeSwallow : MonoBehaviour
{
    /*──────── 组件引用 ────────*/
    [Header("外部组件")]
    public SlimeAbilityManager abilityMgr;   // ← Inspector 拖 Slime 进来

    /*──────── 检测设置 ────────*/
    [Header("检测设置")]
    public Transform swallowCenter;
    public float swallowRadius = 1f;
    public LayerMask swallowableLayer;

    /*──────── 吞噬动画 ────────*/
    [Header("吞噬时间")]
    public float swallowDuration = 0.883f;
    public float swallowMoveSpeed = 2f;
    float swallowTimer, swallowMoveTimer;

    /*──────── Yue动画 ─────────*/
    [Header("Yue动画设置")]
    public string yueTriggerName = "Yue";
    public float yueDuration = 0.8f;
    public float yueMoveSpeed = 2f;
    float yueTimer, yueMoveTimer;

    /*──────── 能力槽 ──────────*/
    [Header("能力槽")]
    public int maxSlots = 3;
    List<Ability> slots = new();
    int currentIndex = 0;

    /*──────── 状态标志 ────────*/
    bool swallowedThisPress = false;

    Animator anim;
    Rigidbody2D rb;

    /*================ Awake ================*/
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (!abilityMgr)
            abilityMgr = GetComponent<SlimeAbilityManager>(); // 同物体上自动取
    }

    /*================ Update ===============*/
    void Update()
    {
        if (yueTimer <= 0f &&
            (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)))
        {
            swallowTimer = swallowDuration;
            swallowMoveTimer = swallowDuration;
            swallowedThisPress = false;
        }

        if (swallowTimer > 0f)
        {
            swallowTimer -= Time.deltaTime;
            if (!swallowedThisPress) DetectAndSwallow();
        }

        if (Input.GetKeyDown(KeyCode.R) && swallowTimer <= 0f && yueTimer <= 0f)
            PlayYueAndDrop();

        if (yueTimer > 0f) yueTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchAbility(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchAbility(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchAbility(2);
    }

    /*================ FixedUpdate ============*/
    void FixedUpdate()
    {
        if (swallowMoveTimer > 0f)
        {
            swallowMoveTimer -= Time.fixedDeltaTime;
            float dir = transform.localScale.x > 0 ? 1f : -1f;
            rb.position += Vector2.right * dir * swallowMoveSpeed * Time.fixedDeltaTime;
        }

        if (yueMoveTimer > 0f)
        {
            yueMoveTimer -= Time.fixedDeltaTime;
            float dir = transform.localScale.x > 0 ? -1f : 1f;
            rb.position += Vector2.right * dir * yueMoveSpeed * Time.fixedDeltaTime;
        }
    }

    /*=========== 吞噬检测 ===========*/
    void DetectAndSwallow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            swallowCenter.position, swallowRadius, swallowableLayer);
        if (hits.Length == 0) return;

        Collider2D target = hits[0];
        Debug.Log($"吞噬对象：{target.name}");

        AbilityType gained = AbilityType.None;
        SwallowableSkill swComp = target.GetComponent<SwallowableSkill>();
        if (swComp) gained = swComp.abilityGranted;

        if (gained != AbilityType.None)
            AddAbility(gained);

        Destroy(target.gameObject);
        swallowedThisPress = true;
    }

    /*=========== Yue动画 + 丢弃 ===========*/
    void PlayYueAndDrop()
    {
        if (anim) { anim.ResetTrigger(yueTriggerName); anim.SetTrigger(yueTriggerName); }

        yueTimer = yueDuration;
        yueMoveTimer = yueDuration;

        if (slots.Count > 0)
        {
            abilityMgr.SetAbility(AbilityType.None);     // 清除旧效果
            slots.RemoveAt(currentIndex);
            currentIndex = Mathf.Clamp(currentIndex, 0, slots.Count - 1);

            if (slots.Count > 0)                         // 若还有能力则激活新槽
                abilityMgr.SetAbility(slots[currentIndex].type);

            PrintSlots();
        }
    }

    /*=========== 能力槽管理 ===========*/
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

        abilityMgr.SetAbility(type);     // 让 AbilityManager 生效
    }

    void SwitchAbility(int idx)
    {
        currentIndex = idx;

        if (idx < slots.Count)
        {
            Debug.Log($"<color=cyan>切换到槽位{idx + 1}: {slots[idx].type}</color>");
            abilityMgr.SetAbility(slots[idx].type);
        }
        else
        {
            Debug.Log($"<color=grey>槽位{idx + 1}: 当前无技能</color>");
            abilityMgr.SetAbility(AbilityType.None);
        }
    }

    void PrintSlots()
    {
        Debug.Log("—— 当前能力槽 ——");
        for (int i = 0; i < slots.Count; i++)
            Debug.Log($"槽{i + 1}: {slots[i].type}{(i == currentIndex ? " [激活]" : "")}");
    }

    /*=========== Scene Gizmo ===========*/
    void OnDrawGizmosSelected()
    {
        if (!swallowCenter) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(swallowCenter.position, swallowRadius);
    }
}
