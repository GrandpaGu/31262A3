using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Level2SlimeController))]
public class SlimeAbilityManager : MonoBehaviour
{
    Level2SlimeController controller;

    [Header("倍率设置")]
    public float speedUpFactor = 1.5f;   // Goat
    public float highJumpFactor = 2f;   // Rabbit

    AbilityType current = AbilityType.None;

    void Awake()
    {
        controller = GetComponent<Level2SlimeController>();
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
        }

        current = AbilityType.None;
    }
}
