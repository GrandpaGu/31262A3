using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("这个检查点的编号（仅供日志使用）")]
    public int checkpointIndex;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log($"[Checkpoint] 玩家通过了检查点 {checkpointIndex}");

            // 这里传的是 Transform，不是 int！
            GameManagerL2.Instance.UpdateCheckpoint(transform);
        }
    }
}
