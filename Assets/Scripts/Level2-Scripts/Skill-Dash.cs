using System.Collections;
using UnityEngine;

/// 按下 Shift 触发冲刺；挂在 Slime 上即可
[RequireComponent(typeof(Level2SlimeController))]
public class DashAbility : MonoBehaviour
{
    [Header("Dash 参数")]
    public float dashMultiplier = 3f;   // 冲刺倍速
    public float dashTime = 0.25f;// 冲刺持续
    public float cooldown = 1.0f; // 冷却

    readonly KeyCode dashKey = KeyCode.LeftShift;

    Level2SlimeController ctrl;
    float cdTimer = 0f;
    bool dashing = false;
    float savedSpeed;

    void Awake() => ctrl = GetComponent<Level2SlimeController>();

    void Update()
    {
        if (cdTimer > 0f) cdTimer -= Time.deltaTime;

        if (!dashing && cdTimer <= 0f && Input.GetKeyDown(dashKey))
            StartCoroutine(DoDash());
    }

    IEnumerator DoDash()
    {
        dashing = true;
        savedSpeed = ctrl.moveSpeed;
        ctrl.moveSpeed = savedSpeed * dashMultiplier;

        yield return new WaitForSeconds(dashTime);

        ctrl.moveSpeed = savedSpeed;
        dashing = false;
        cdTimer = cooldown;
    }
}
