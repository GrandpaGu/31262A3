#region BatBlindnessTrigger.cs

using UnityEngine;

/// <summary>
/// 只负责碰撞 → 触发 Slime 致盲，不处理移动。
/// 必须与 BatMovementController 位于同一 GameObject。
/// Collider2D 需设为 IsTrigger=true。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BatBlindnessTrigger : MonoBehaviour
{
    [Tooltip("失明时长 (秒)")]
    public float blindDuration = 2.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        BatBlind be = other.GetComponent<BatBlind>();
        if (be) be.Blind(blindDuration);
    }
}

#endregion