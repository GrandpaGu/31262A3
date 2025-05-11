using UnityEngine;
using System.Collections.Generic;

public class Ability
{
    public AbilityType type;
    public Ability(AbilityType t) { type = t; }
}

public class SlimeSwallow : MonoBehaviour
{
    /*©§©§©§©§©§©§©§©§ ◊Èº˛“˝”√ ©§©§©§©§©§©§©§©§*/
    [Header("Õ‚≤ø◊Èº˛")]
    public SlimeAbilityManager abilityMgr;   // °ÅEInspector Õœ Slime Ω¯¿¥

    /*©§©§©§©§©§©§©§©§ ºÅE‚…Ë÷√ ©§©§©§©§©§©§©§©§*/
    [Header("ºÅE‚…Ë÷√")]
    public Transform swallowCenter;
    public float swallowRadius = 1f;
    public LayerMask swallowableLayer;

    /*©§©§©§©§©§©§©§©§ ÕÃ …∂Øª≠ ©§©§©§©§©§©§©§©§*/
    [Header("ÕÃ … ±ºÅE")]
    public float swallowDuration = 0.883f;
    public float swallowMoveSpeed = 2f;
    float swallowTimer, swallowMoveTimer;

    /*©§©§©§©§©§©§©§©§ Yue∂Øª≠ ©§©§©§©§©§©§©§©§©§*/
    [Header("Yue∂Øª≠…Ë÷√")]
    public string yueTriggerName = "Yue";
    public float yueDuration = 0.8f;
    public float yueMoveSpeed = 2f;
    float yueTimer, yueMoveTimer;

    /*©§©§©§©§©§©§©§©§ ƒ‹¡¶≤€ ©§©§©§©§©§©§©§©§©§©§*/
    [Header("ƒ‹¡¶≤€")]
    public int maxSlots = 3;
    List<Ability> slots = new();
    int currentIndex = 0;

    /*©§©§©§©§©§©§©§©§ ◊¥Ã¨±ÅEæ ©§©§©§©§©§©§©§©§*/
    bool swallowedThisPress = false;

    Animator anim;
    Rigidbody2D rb;

    /*================ Awake ================*/
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (!abilityMgr)
            abilityMgr = GetComponent<SlimeAbilityManager>(); // Õ¨ŒÅEÂ…œ◊‘∂Ø»°
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

    /*=========== ÕÃ …ºÅEÅE===========*/
    void DetectAndSwallow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            swallowCenter.position, swallowRadius, swallowableLayer);
        if (hits.Length == 0) return;

        Collider2D target = hits[0];
        Debug.Log($"ÕÃ …∂‘œÛ£∫{target.name}");

        AbilityType gained = AbilityType.None;
        SwallowableSkill swComp = target.GetComponent<SwallowableSkill>();
        if (swComp) gained = swComp.abilityGranted;

        if (gained != AbilityType.None)
            AddAbility(gained);

        Destroy(target.gameObject);
        swallowedThisPress = true;
    }

    /*=========== Yue∂Øª≠ + ∂™∆ÅE===========*/
    void PlayYueAndDrop()
    {
        if (anim) { anim.ResetTrigger(yueTriggerName); anim.SetTrigger(yueTriggerName); }

        yueTimer = yueDuration;
        yueMoveTimer = yueDuration;

        if (slots.Count > 0)
        {
            abilityMgr.SetAbility(AbilityType.None);     // «Â≥˝æ…–ßπÅE
            slots.RemoveAt(currentIndex);
            currentIndex = Mathf.Clamp(currentIndex, 0, slots.Count - 1);

            if (slots.Count > 0)                         // »Ùªπ”–ƒ‹¡¶‘Úº§ª˚Ï¬≤€
                abilityMgr.SetAbility(slots[currentIndex].type);

            PrintSlots();
        }
    }

    /*=========== ƒ‹¡¶≤€π‹¿ÅE===========*/
    void AddAbility(AbilityType type)
    {
        if (slots.Count >= maxSlots)
        {
            Debug.Log("<color=red>≤€Œª“—¬˙£¨Œﬁ∑®ªÒµ√–¬ƒ‹¡¶</color>");
            return;
        }

        slots.Add(new Ability(type));
        currentIndex = slots.Count - 1;
        PrintSlots();

        abilityMgr.SetAbility(type);     // »√ AbilityManager …˙–ß
    }

    void SwitchAbility(int idx)
    {
        currentIndex = idx;

        if (idx < slots.Count)
        {
            Debug.Log($"<color=cyan>«–ªªµΩ≤€Œª{idx + 1}: {slots[idx].type}</color>");
            abilityMgr.SetAbility(slots[idx].type);
        }
        else
        {
            Debug.Log($"<color=grey>≤€Œª{idx + 1}: µ±«∞Œﬁººƒ‹</color>");
            abilityMgr.SetAbility(AbilityType.None);
        }
    }

    void PrintSlots()
    {
        Debug.Log("°™°™ µ±«∞ƒ‹¡¶≤€ °™°™");
        for (int i = 0; i < slots.Count; i++)
            Debug.Log($"≤€{i + 1}: {slots[i].type}{(i == currentIndex ? " [º§ª˚y" : "")}");
    }

    /*=========== Scene Gizmo ===========*/
    void OnDrawGizmosSelected()
    {
        if (!swallowCenter) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(swallowCenter.position, swallowRadius);
    }

    public int GetCurrentIndex() => currentIndex;
    public List<Ability> GetAbilities() => slots;
}
