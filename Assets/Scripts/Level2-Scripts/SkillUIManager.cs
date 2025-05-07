// ===============================
// SkillUIManager.cs
// ===============================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    [Header("技能 UI 槽位 (依序 1/2/3)")]
    public List<Image> slots; // 在 Inspector 拖入 Image（3 个）

    [Header("能力图标表 (顺序对应 AbilityType)")]
    public List<Sprite> iconTable; // 对应能力类型顺序放图

    [Header("空槽图片")]
    public Sprite emptySprite; // 没有能力时显示的图片

    [Header("灰度材质")]
    public Material grayscaleMaterial; // 灰度材质（用于未激活）

    private SlimeSwallow swallow;

    void Awake()
    {
        swallow = FindObjectOfType<SlimeSwallow>();
    }

    void LateUpdate()
    {
        if (swallow == null || slots.Count == 0) return;

        var current = swallow.GetCurrentIndex();
        var abilities = swallow.GetAbilities();

        for (int i = 0; i < slots.Count; i++)
        {
            Image img = slots[i];

            if (i >= abilities.Count)
            {
                img.sprite = emptySprite;
                img.material = grayscaleMaterial;
            }
            else
            {
                var ability = abilities[i];
                int idx = (int)ability.type;

                if (idx >= 0 && idx < iconTable.Count && iconTable[idx] != null)
                    img.sprite = iconTable[idx];
                else
                    img.sprite = emptySprite;

                img.material = (i == current) ? null : grayscaleMaterial;
            }
        }
    }
}

// ===============================
// SlimeSwallow.cs 中需提供如下接口：
// public int GetCurrentIndex() => currentIndex;
// public List<Ability> GetAbilities() => slots;
// ===============================
