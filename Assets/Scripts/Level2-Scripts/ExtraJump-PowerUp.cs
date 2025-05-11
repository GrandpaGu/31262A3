using UnityEngine;

public class JumpPowerUp : MonoBehaviour
{
    [Header("二段跳可用次数")]
    public int grantedExtraJumps = 1;  // 直接改成你想赋予的额外跳跃次数

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // 确保 Slime 物体的 Tag 是 "Player"
        {
            Level2SlimeController slime = other.GetComponent<Level2SlimeController>();
            if (slime != null)
            {
                slime.maxExtraJumps = grantedExtraJumps;
            }

            Destroy(gameObject);  // 拾取后销毁鞋子
        }
    }
}
