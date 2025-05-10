using UnityEngine;

public enum AbilityType
{
    None,
    SpeedUp,      // Horse → 加速
    HighJump,     // Rabbit → 跳跃力
    Dash,         // Redbull → 蛮牛冲刺
    Glide,        // 鸟 → 滑翔
    WallJump,     //羊 → 蹬墙跳
    Swim          //鲨鱼  游泳
}

/// <summary>
/// 挂在可被吞噬对象上，用来告诉 Slime 吞掉后授予什么能力
/// </summary>
public class SwallowableSkill : MonoBehaviour
{
    public AbilityType abilityGranted = AbilityType.None;
}
