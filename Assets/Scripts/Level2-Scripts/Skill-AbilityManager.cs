using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Level2SlimeController))]
public class SlimeAbilityManager : MonoBehaviour
{
    Level2SlimeController controller;
    DashAbility dash;   // 新增
    [Header("倍率设置")]
    public float speedUpFactor = 1.5f;   // Goat
    public float highJumpFactor = 2f;   // Rabbit
    public float waterDrag = 0.8f;     //水中阻力

    AbilityType current = AbilityType.None;

    void Awake()
    {
        controller = GetComponent<Level2SlimeController>();
        dash = GetComponent<DashAbility>();          // 允许预先挂脚本
        if (dash) dash.enabled = false;                    // 默认关闭
    }

    /// <summary>SlimeSwallow 调用；AbilityType.None 表示清除效果</summary>
    public void SetAbility(AbilityType type)
    {
        RemoveCurrent();  // 移除旧的 buff
        current = type;

        switch (current)
        {
            case AbilityType.SpeedUp:
                controller.moveSpeed *= speedUpFactor;
                break;
            case AbilityType.HighJump:
                controller.jumpForce *= highJumpFactor;
                break;
            case AbilityType.Glide:
                controller.enableGlide = true;
                break;
            case AbilityType.Swim:                         // ★ 新增
                controller.hasSwimAbility = true;        // 可水下呼吸
                controller.moveSpeed *= waterDrag; // 整体减速
                break;
            case AbilityType.Dash:             // 牛 → Dash
                if (!dash) dash = gameObject.AddComponent<DashAbility>();
                dash.enabled = true;
                break;
            case AbilityType.WallJump:  // 羊的能力
                controller.enableWallStick = true;
                break;
        }

        Debug.Log($"[Ability] 当前能力: {current}");
    }

    void RemoveCurrent()
    {
        switch (current)
        {
            case AbilityType.SpeedUp:
                controller.moveSpeed /= speedUpFactor;
                break;
            case AbilityType.HighJump:
                controller.jumpForce /= highJumpFactor;
                break;
            case AbilityType.Glide:
                controller.enableGlide = false;
                controller.rb.gravityScale = controller.defaultGravityScale;
                break;
            case AbilityType.Swim:                         // ★ 新增
                controller.hasSwimAbility = false;
                controller.moveSpeed /= waterDrag;   // 速度恢复
                break;
            case AbilityType.Dash:
                if (dash) dash.enabled = false;
                break;
            case AbilityType.WallJump: // 取消羊的能力
                controller.enableWallStick = false;
                break;
        }

        current = AbilityType.None;
    }
}
