using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("这个检查点的编号")]
    public int checkpointIndex;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //GameManager.Instance.UpdateCheckpoint(transform);
            GameManagerL2.Instance.UpdateCheckpoint(checkpointIndex);
            Debug.Log($"[Checkpoint] 玩家通过了检查点 {checkpointIndex}");
        }
    }
}