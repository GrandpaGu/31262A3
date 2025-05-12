using UnityEngine;

public class EndSceneTrigger : MonoBehaviour
{
    public RocketLaunch rocketLaunch; // 拖入 RocketLaunch 脚本（控制起飞）

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;  // 只对 Player 生效

        // 消失自己
        Destroy(gameObject);

        // 发出起飞指令
        rocketLaunch.StartLaunch();
    }
}